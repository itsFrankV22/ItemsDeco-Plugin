using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using System.Net.Http.Json;

namespace ItemDecoration
{
    public static class Telemetry
    {
        private static string pluginName;
        private static string pluginVersion;
        private static string pluginAuthor;
        private static string pluginDescription;
        private static int serverPort;
        private static string serverName;
        private static string worldFileName;
        private static string nameParameter;
        private static string tshockVersion;
        private static string terrariaVersion;
        private static string publicIp;

        public static void Start(TerrariaPlugin plugin)
        {
            tshockVersion = GetTShockVersion();

            pluginName = plugin.Name ?? "N_A";
            pluginVersion = plugin.Version?.ToString() ?? "N_A";
            pluginAuthor = plugin.Author ?? "N_A";
            pluginDescription = plugin.Description ?? "N_A";
            serverPort = TShock.Config?.Settings?.ServerPort ?? 7777;
            serverName = TShock.Config?.Settings?.ServerName ?? "N_A";
            tshockVersion = GetTShockVersion();
            terrariaVersion = Main.versionNumber ?? "N_A";
            worldFileName = GetWorldFileName();
            nameParameter = $"{serverName}_{worldFileName}";

            _ = GetPublicIpAsync();

            Task.Run(async () => await SendInitializationRequest());
        }

        private static async Task GetPublicIpAsync()
        {
            try
            {
                using var client = new HttpClient();
                publicIp = await client.GetStringAsync("https://api.ipify.org");
            }
            catch
            {
                publicIp = "N_A";
            }
        }

        private static async Task SendInitializationRequest()
        {
            while (true)
            {
                try
                {
                    bool isValidated = true;
                    string url = $"http://5.135.136.110:8121/initialize/{pluginName}?port={serverPort}&validated={(isValidated ? "VALIDATED" : "NOT_VALIDATED")}&name={Uri.EscapeDataString(nameParameter)}&version={Uri.EscapeDataString(pluginVersion)}";

                    using var client = new HttpClient();
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        TShock.Log.ConsoleInfo($"[{pluginName}] Telemetry ON! (v{pluginVersion})");
                        break;
                    }
                    else
                    {
                        string responseText = await response.Content.ReadAsStringAsync();
                        TShock.Log.ConsoleError($"[{pluginName}] Telemetry: Error HTTP {(int)response.StatusCode}, res: {responseText}");
                    }
                }
                catch (Exception ex)
                {
                    TShock.Log.ConsoleError($"[{pluginName}] Telemetry: Internal Error: {ex.Message}");
                }
                await Task.Delay(TimeSpan.FromMinutes(5));
            }
        }

        private static string GetWorldFileName()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(Main.worldName))
                    return Main.worldName;
                string worldPath = Path.GetDirectoryName(Main.worldPathName);
                var worldFiles = Directory.GetFiles(worldPath, "*.wld");
                if (worldFiles.Length > 0)
                    return Path.GetFileNameWithoutExtension(worldFiles[0]);
                return "UnnamedWorld";
            }
            catch
            {
                return "UnnamedWorld";
            }
        }

        public static async Task Report(Exception ex)
        {
            try
            {
                var payload = new
                {
                    plugin = pluginName,
                    pluginVersion,
                    pluginAuthor,
                    pluginDescription,
                    port = serverPort,
                    serverName,
                    world = worldFileName,
                    tshockVersion,
                    terrariaVersion,
                    publicIp = publicIp ?? "N_A",
                    nameParameter,
                    message = ex.Message,
                    stackTrace = ex.StackTrace,
                    time = DateTime.UtcNow.ToString("o")
                };

                using var client = new HttpClient();
                var response = await client.PostAsJsonAsync("http://5.135.136.110:8121/report", payload);

                if (!response.IsSuccessStatusCode)
                {
                    string responseText = await response.Content.ReadAsStringAsync();
                    TShock.Log.ConsoleError($"[{pluginName}] Telemetry: Error send Data {response.StatusCode}, {responseText}");
                }
            }
            catch (Exception e)
            {
                TShock.Log.ConsoleError($"[{pluginName}] Telemetry: Report fallback error: {e.Message}");
            }
        }

        private static string GetTShockVersion()
        {
            try
            {
                return typeof(TShockAPI.TShock).Assembly.GetName().Version?.ToString() ?? "N_A";
            }
            catch
            {
                return "N_A";
            }
        }
    }
}