using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using UpperApp.Core;

namespace UpperApp.Services
{
    internal interface IConfigStorage
    {
        Task<AppSettings> LoadAsync();
        Task SaveAsync(AppSettings settings);
        void SaveSync(AppSettings settings);
    }

    internal class JsonFileConfigStorage : IConfigStorage
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonFileConfigStorage()
        {
            // 使用 AppDomain.CurrentDomain.BaseDirectory 替代 Application.StartupPath
            // 该属性在所有 .NET 平台上均可用，且返回应用程序的基目录（通常与 StartupPath 相同）
            _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<AppSettings> LoadAsync()
        {
            if (!File.Exists(_filePath))
                return new AppSettings();

            try
            {
                string json = await File.ReadAllTextAsync(_filePath);
                return JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();
            }
            catch (Exception ex)
            {
                // 记录日志（可选）
                System.Diagnostics.Debug.WriteLine($"加载配置失败: {ex.Message}");
                return new AppSettings();
            }
        }

        public async Task SaveAsync(AppSettings settings)
        {
            try
            {
                string json = JsonSerializer.Serialize(settings, _jsonOptions);
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存配置失败: {ex.Message}");
            }
        }

        public void SaveSync(AppSettings settings)
        {
            try
            {
                string json = JsonSerializer.Serialize(settings, _jsonOptions);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存配置失败: {ex.Message}");
            }
        }
    }
}
