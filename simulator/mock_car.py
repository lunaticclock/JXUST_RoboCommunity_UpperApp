#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
小车上位机模拟下位机脚本
==========================
配合 UpperApp (C# WPF) 使用，无需真实小车即可测试通信、姿态仪表、
航程仪表盘、流量曲线、地图轨迹等功能。

协议格式（与上位机 ProtocolHandler 保持一致）:
    YAW:{value}/OVER\r\n
    ROLL:{value}/OVER\r\n
    PITCH:{value}/OVER\r\n
    DISTANCE:{value}/OVER\r\n
发送指令格式（上位机 ProtocolFormatter）:
    FB:{value}:OVER\r\n          前后速度 (0-100, 50=停止)
    RL:{value}:OVER\r\n          左右方向 (0-100, 50=居中)
    FR:{speed}:{direction}:OVER\r\n  组合控制

运行方式:
    1) TCP 客户端模式（连接上位机 TCP 服务端）:
       python mock_car.py --mode tcp-client --host 127.0.0.1 --port 1234

    2) TCP 服务端模式（上位机作为客户端连接本脚本）:
       python mock_car.py --mode tcp-server --port 1234

    3) UDP 模式:
       python mock_car.py --mode udp --host 127.0.0.1 --port 1234

    4) 串口模式（需要 com0com 虚拟串口对或真实串口回环）:
       python mock_car.py --mode serial --port COM2 --baud 115200

依赖:
    pip install pyserial

作者: AI Assistant
日期: 2026-06-17
"""

import argparse
import math
import random
import socket
import sys
import threading
import time
from dataclasses import dataclass
from typing import Optional

try:
    import serial
except ImportError:
    serial = None


ENCODING = "gb2312"  # 与上位机 TouchSocketSerialAdapter/TCP/UDP 保持一致


@dataclass
class CarState:
    """模拟小车状态，所有角度/距离单位与上位机约定一致。"""

    yaw: float = 0.0        # 偏航角 0-360
    roll: float = 0.0       # 横滚角，模拟小幅摆动
    pitch: float = 0.0      # 俯仰角
    distance: float = 0.0   # 累计行走距离（cm 或 mm，与地图标定相关）

    # 目标控制量
    target_speed: float = 54.0      # 50=停止，默认 54 让小车缓慢前进，DISTANCE 持续增长
    target_direction: float = 50.0  # 50=居中

    # 运动模型参数
    speed: float = 0.0              # 当前实际速度
    heading: float = 0.0            # 当前运动方向（弧度）
    drift_phase: float = 0.0        # 偏航漂移相位，模拟路线偏移-纠正周期

    def update(self, dt: float):
        """每 dt 秒更新一次状态，模拟小车惯性与周期性偏航纠正。"""
        # 速度平滑：目标速度 50 为中位，50 以下倒车，50 以上前进
        desired = (self.target_speed - 50.0) * 0.3  # 映射到 -15 ~ +15 速度单位
        self.speed += (desired - self.speed) * 0.2

        # 周期性偏航漂移：用一个缓慢变化的正弦波模拟小车逐渐跑偏，
        # 再自动纠正。周期约 20 秒，偏航幅度约 ±25°。
        self.drift_phase += dt * 0.3  # 角速度 rad/s，周期约 20s
        drift = math.sin(self.drift_phase) * math.radians(25.0)

        # 方向控制：50 为中位，加入自动纠正量（抵消漂移）+ 用户目标方向
        user_steer = (self.target_direction - 50.0) * 0.6  # 用户输入 -30~30°
        auto_correct = -drift * 0.4                         # 自动纠正项
        steer = user_steer + math.degrees(auto_correct)
        self.heading += math.radians(steer) * 0.05

        # 加入一点噪声，让仪表有真实波动
        self.roll = 3.0 * math.sin(time.time() * 2.5) + random.uniform(-0.5, 0.5)
        self.pitch = 2.0 * math.cos(time.time() * 1.7) + random.uniform(-0.3, 0.3)

        # 根据速度更新距离和偏航
        step = self.speed * dt * 10.0  # 距离增量
        self.distance += abs(step)

        # 偏航 = 航向积分 + 漂移项，模拟偏移与纠正过程
        self.yaw = (self.yaw + math.degrees(self.heading) * dt * 0.5 + math.degrees(drift) * dt * 0.3) % 360.0


class ProtocolEncoder:
    """生成符合上位机协议的字符串。"""

    @staticmethod
    def attitude_frame(kind: str, value: float) -> bytes:
        """发送单条姿态数据，避免上位机目前不做粘包拆分导致只解析第一条。"""
        if kind == "DISTANCE":
            text = f"DISTANCE:{value:.2f}/OVER\r\n"
        else:
            text = f"{kind}:{value:.1f}/OVER\r\n"
        return text.encode(ENCODING)

    @staticmethod
    def attitude(yaw: float, roll: float, pitch: float, distance: float) -> bytes:
        # 已弃用：上位机目前不拆分 /OVER 分隔的多条消息，合并发送会丢失后三条。
        # 请使用 attitude_frame 逐条发送。
        lines = [
            f"YAW:{yaw:.1f}/OVER\r\n",
            f"ROLL:{roll:.1f}/OVER\r\n",
            f"PITCH:{pitch:.1f}/OVER\r\n",
            f"DISTANCE:{distance:.2f}/OVER\r\n",
        ]
        return "".join(lines).encode(ENCODING)

    @staticmethod
    def echo(raw: bytes) -> bytes:
        """把收到的指令原样回显，方便验证上位机发送。"""
        return raw


class CommandParser:
    """解析上位机发来的运动指令。"""

    def __init__(self, state: CarState):
        self.state = state

    def feed(self, data: bytes):
        try:
            text = data.decode(ENCODING, errors="ignore")
        except Exception:
            return

        # 上位机一条指令以 \r\n 结尾，可能多条粘连
        for line in text.split("\r\n"):
            line = line.strip()
            if not line:
                continue

            if line.startswith("FB:") and line.endswith(":OVER"):
                try:
                    self.state.target_speed = float(line[3:-5])
                except ValueError:
                    pass

            elif line.startswith("RL:") and line.endswith(":OVER"):
                try:
                    self.state.target_direction = float(line[3:-5])
                except ValueError:
                    pass

            elif line.startswith("FR:") and line.endswith(":OVER"):
                try:
                    parts = line[3:-5].split(":")
                    self.state.target_speed = float(parts[0])
                    self.state.target_direction = float(parts[1])
                except (ValueError, IndexError):
                    pass


class BaseLink:
    """通信链路抽象。"""

    def read(self, timeout: float = 0.05) -> Optional[bytes]:
        raise NotImplementedError

    def write(self, data: bytes):
        raise NotImplementedError

    def close(self):
        pass


class TcpClientLink(BaseLink):
    def __init__(self, host: str, port: int):
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.sock.connect((host, port))
        self.sock.settimeout(0.05)
        print(f"[TCP 客户端] 已连接 {host}:{port}")

    def read(self, timeout: float = 0.05) -> Optional[bytes]:
        try:
            return self.sock.recv(1024)
        except socket.timeout:
            return None
        except OSError:
            return b""

    def write(self, data: bytes):
        try:
            self.sock.sendall(data)
        except OSError:
            pass

    def close(self):
        self.sock.close()


class TcpServerLink(BaseLink):
    def __init__(self, port: int):
        self.server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.server.bind(("0.0.0.0", port))
        self.server.listen(1)
        self.server.settimeout(1.0)
        self.client: Optional[socket.socket] = None
        print(f"[TCP 服务端] 监听 0.0.0.0:{port}，等待上位机连接...")
        self._accept()

    def _accept(self):
        try:
            self.client, addr = self.server.accept()
            self.client.settimeout(0.05)
            print(f"[TCP 服务端] 上位机已连接: {addr}")
        except socket.timeout:
            pass

    def read(self, timeout: float = 0.05) -> Optional[bytes]:
        if self.client is None:
            self._accept()
            return None
        try:
            return self.client.recv(1024)
        except socket.timeout:
            return None
        except OSError:
            self.client = None
            return b""

    def write(self, data: bytes):
        if self.client is None:
            return
        try:
            self.client.sendall(data)
        except OSError:
            self.client = None

    def close(self):
        if self.client:
            self.client.close()
        self.server.close()


class UdpLink(BaseLink):
    def __init__(self, host: str, port: int):
        self.host = host
        self.port = port
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.sock.settimeout(0.05)
        self.peer: Optional[tuple] = None
        print(f"[UDP] 目标 {host}:{port}，等待接收上位机首条消息以学习对端地址...")

    def read(self, timeout: float = 0.05) -> Optional[bytes]:
        try:
            data, addr = self.sock.recvfrom(1024)
            self.peer = addr
            return data
        except socket.timeout:
            return None
        except OSError:
            return b""

    def write(self, data: bytes):
        # 上位机 UDP 先绑定本地端口并发送，我们根据收到的消息记录对端
        if self.peer is None:
            # 如果还没收到消息，尝试直接发给目标
            self.sock.sendto(data, (self.host, self.port))
        else:
            self.sock.sendto(data, self.peer)

    def close(self):
        self.sock.close()


class SerialLink(BaseLink):
    def __init__(self, port: str, baud: int):
        if serial is None:
            raise RuntimeError("缺少 pyserial，请执行: pip install pyserial")
        self.ser = serial.Serial(port, baud, timeout=0.05)
        print(f"[串口] 已打开 {port} @ {baud}")

    def read(self, timeout: float = 0.05) -> Optional[bytes]:
        if self.ser.in_waiting:
            return self.ser.read(min(self.ser.in_waiting, 1024))
        return None

    def write(self, data: bytes):
        self.ser.write(data)

    def close(self):
        self.ser.close()


def create_link(args) -> BaseLink:
    if args.mode == "tcp-client":
        return TcpClientLink(args.host, args.port)
    if args.mode == "tcp-server":
        return TcpServerLink(args.port)
    if args.mode == "udp":
        return UdpLink(args.host, args.port)
    if args.mode == "serial":
        return SerialLink(args.port, args.baud)
    raise ValueError(f"未知模式: {args.mode}")


def main():
    parser = argparse.ArgumentParser(description="模拟小车下位机")
    parser.add_argument(
        "--mode",
        choices=["tcp-client", "tcp-server", "udp", "serial"],
        default="tcp-client",
        help="通信模式",
    )
    parser.add_argument("--host", default="127.0.0.1", help="TCP/UDP 目标 IP")
    parser.add_argument(
        "--port",
        type=int,
        default=1234,
        help="端口号（TCP/UDP）或串口号（serial 模式下为 COMx）",
    )
    parser.add_argument("--baud", type=int, default=115200, help="串口波特率")
    parser.add_argument(
        "--hz",
        type=float,
        default=10.0,
        help="姿态数据发送频率（Hz）",
    )
    parser.add_argument(
        "--no-echo",
        dest="echo",
        action="store_false",
        default=True,
        help="关闭收到指令的回显",
    )
    parser.add_argument(
        "--quiet",
        action="store_true",
        default=False,
        help="安静模式，只打印姿态数据",
    )
    args = parser.parse_args()

    state = CarState()
    parser_cmd = CommandParser(state)
    link = create_link(args)

    # 姿态字段轮询顺序；hz 表示“完整姿态组”的频率，每条消息间隔 period/4
    attitude_kinds = ["YAW", "ROLL", "PITCH", "DISTANCE"]
    attitude_index = 0
    group_period = 1.0 / args.hz
    frame_period = group_period / len(attitude_kinds)
    last_send = 0.0
    running = True

    # 捕获 Ctrl+C 优雅退出
    try:
        while running:
            now = time.time()

            # 1) 读上位机指令
            data = link.read()
            if data:
                parser_cmd.feed(data)
                if args.echo:
                    link.write(ProtocolEncoder.echo(data))
                if not args.quiet:
                    print(f"[收←上] {data.decode(ENCODING, errors='ignore').strip()!r}")

            # 2) 更新小车状态
            state.update(frame_period)

            # 3) 定时发送单条姿态数据，轮流发送 YAW/ROLL/PITCH/DISTANCE
            #    上位机目前按 evt.Content 整段解析，不拆分 /OVER 分隔的多条消息，
            #    因此必须逐条发送，否则只有第一条被识别。
            if now - last_send >= frame_period:
                kind = attitude_kinds[attitude_index]
                value = getattr(state, kind.lower())
                payload = ProtocolEncoder.attitude_frame(kind, value)
                link.write(payload)
                attitude_index = (attitude_index + 1) % len(attitude_kinds)
                last_send = now
                if not args.quiet and attitude_index == 0:
                    # 每完成一组（4 条）打印一次汇总
                    print(
                        f"[发→上] YAW={state.yaw:.1f} ROLL={state.roll:.1f} "
                        f"PITCH={state.pitch:.1f} DIST={state.distance:.2f} "
                        f"SPD={state.target_speed:.0f} DIR={state.target_direction:.0f}"
                    )

            # 4) 小睡避免 CPU 空转
            time.sleep(max(0.001, frame_period / 4))

    except KeyboardInterrupt:
        print("\n[退出] 模拟器被用户中断")
    finally:
        link.close()
        print("[退出] 链路已关闭")


if __name__ == "__main__":
    main()
