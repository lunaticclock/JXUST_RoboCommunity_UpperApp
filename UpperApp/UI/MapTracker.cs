using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace UpperApp.UI
{
    internal class MapTracker
    {
        private readonly Canvas _canvas;
        private float _calibratedDistance = 1.0f;
        private float _aspectRatio = 1.0f;
        private Point? _startPoint;
        private Point? _endPoint;
        private readonly List<Polyline> _tracks = [];
        private Polyline _currentTrack;
        private double _lastX = -1, _lastY = -1;

        public string StartPoint => _startPoint.HasValue ? $"{_startPoint.Value.X:F0},{_startPoint.Value.Y:F0}" : "0,0";
        public string EndPoint => _endPoint.HasValue ? $"{_endPoint.Value.X:F0},{_endPoint.Value.Y:F0}" : "0,0";

        public MapTracker(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void SetCalibratedDistance(float distance)
        {
            _calibratedDistance = distance;
        }

        public void SetAspectRatio(float ratio)
        {
            _aspectRatio = ratio;
        }

        public void OnDistanceChanged(string distText, string yawText, Action<string> logCallback)
        {
            if (!float.TryParse(distText, out float dist)) return;
            if (!float.TryParse(yawText, out float yaw)) return;

            double rad = yaw * Math.PI / 180.0;
            double dx = dist * Math.Sin(rad) * _aspectRatio;
            double dy = -dist * Math.Cos(rad);

            if (_lastX < 0)
            {
                // Canvas 未完成布局时 Bounds 可能为 0，跳过本次避免起点落在左上角
                if (_canvas.Bounds.Width <= 0 || _canvas.Bounds.Height <= 0) return;
                _lastX = _canvas.Bounds.Width / 2;
                _lastY = _canvas.Bounds.Height / 2;
            }

            double newX = _lastX + dx;
            double newY = _lastY + dy;

            if (_currentTrack == null)
            {
                _currentTrack = new Polyline
                {
                    Stroke = Brushes.Cyan,
                    StrokeThickness = 2
                };
                _currentTrack.Points.Add(new Point(_lastX, _lastY));
                _canvas.Children.Add(_currentTrack);
                _tracks.Add(_currentTrack);
            }

            _currentTrack.Points.Add(new Point(newX, newY));

            var dot = new Ellipse { Width = 4, Height = 4, Fill = Brushes.Orange };
            Canvas.SetLeft(dot, newX - 2);
            Canvas.SetTop(dot, newY - 2);
            _canvas.Children.Add(dot);

            _lastX = newX;
            _lastY = newY;
        }

        public void SetBackgroundImage(IImageBrushSource source)
        {
            _canvas.Background = new ImageBrush { Source = source };
        }

        public void OnMapClick(Point p, Action<string> logCallback)
        {
            if (!_startPoint.HasValue)
            {
                _startPoint = p;
                var dot = new Ellipse { Width = 8, Height = 8, Fill = Brushes.Lime };
                Canvas.SetLeft(dot, p.X - 4);
                Canvas.SetTop(dot, p.Y - 4);
                _canvas.Children.Add(dot);
            }
            else
            {
                _endPoint = p;
                var dot = new Ellipse { Width = 8, Height = 8, Fill = Brushes.Red };
                Canvas.SetLeft(dot, p.X - 4);
                Canvas.SetTop(dot, p.Y - 4);
                _canvas.Children.Add(dot);

                var line = new Line
                {
                    StartPoint = _startPoint.Value,
                    EndPoint = _endPoint.Value,
                    Stroke = Brushes.Yellow,
                    StrokeThickness = 1,
                    StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 4, 2 }
                };
                _canvas.Children.Add(line);
            }
        }

        private Line _previewLine;

        public void OnMouseMove(Point p)
        {
            if (_startPoint.HasValue && !_endPoint.HasValue)
            {
                if (_previewLine == null)
                {
                    _previewLine = new Line
                    {
                        Stroke = Brushes.Yellow,
                        StrokeThickness = 1,
                        StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 4, 2 },
                        Opacity = 0.5
                    };
                    _canvas.Children.Add(_previewLine);
                }
                _previewLine.StartPoint = _startPoint.Value;
                _previewLine.EndPoint = p;
            }
        }

        public string GetMousePosition(Point p)
        {
            return $"{p.X:F0},{p.Y:F0}";
        }

        public void Clear()
        {
            _canvas.Children.Clear();
            _tracks.Clear();
            _currentTrack = null;
            _previewLine = null;
            _startPoint = null;
            _endPoint = null;
            _lastX = -1;
            _lastY = -1;
        }
    }
}
