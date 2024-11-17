using TerrariaApi.Server;
using TShockAPI;
using Terraria;
using Microsoft.Xna.Framework;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;

namespace ItemChatPlugin
{
    [ApiVersion(2, 1)]
    public class ItemChatPlugin : TerrariaPlugin
    {
        private ConfigChat config;

        public ItemChatPlugin(Main game) : base(game)
        {
            LoadConfig();
        }

        public override void Initialize()
        {
            ServerApi.Hooks.ServerChat.Register(this, OnServerChat);
        }

        private void OnServerChat(ServerChatEventArgs args)
        {
            if (args.Text.StartsWith("/")) return;

            if (config.CONFIGURATION.ItemChatSuffix)
            {
                TSPlayer player = TShock.Players[args.Who];
                if (player == null) return;

                string message = args.Text;
                message = ItemSuffixPlaceholder.ReplacePlaceholderWithItem(player, message);

                PropertyInfo? propertyInfo = args.GetType().GetProperty("Text");
                propertyInfo?.SetValue(args, message);
            }
        }

        private void LoadConfig()
        {
            string configDirectory = Path.Combine(TShock.SavePath, "ItemDeco");

            if (!Directory.Exists(configDirectory))
                Directory.CreateDirectory(configDirectory);

            string filePath = Path.Combine(configDirectory, "ItemChatConfig.json");

            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    config = JsonConvert.DeserializeObject<ConfigChat>(json);
                }
                else
                {
                    config = new ConfigChat
                    {
                        CONFIGURATION = new Configuration
                        {
                            ItemChatSuffix = true
                        }
                    };
                    SaveConfig(filePath);
                }
            }
            catch (Exception ex)
            {
                TShock.Log.Error("Error al cargar la configuración de ItemChatPlugin: " + ex.Message);
                config = new ConfigChat
                {
                    CONFIGURATION = new Configuration
                    {
                        ItemChatSuffix = true
                    }
                };
            }
        }

        private void SaveConfig(string filePath)
        {
            try
            {
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                TShock.Log.Error("Error al guardar la configuración de ItemChatPlugin: " + ex.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                ServerApi.Hooks.ServerChat.Deregister(this, OnServerChat);

            base.Dispose(disposing);
        }
    }

    public static class ItemSuffixPlaceholder
    {
        public static string ReplacePlaceholderWithItem(TSPlayer player, string message)
        {
            var selectedItem = player.TPlayer.inventory[player.TPlayer.selectedItem];

            if (selectedItem != null && selectedItem.type > 0)
                return $"[i:{selectedItem.netID}] < {message}";

            return message;
        }
    }

    public class ConfigChat
    {
        public Configuration CONFIGURATION { get; set; }
    }

    public class Configuration
    {
        public bool ItemChatSuffix { get; set; }
    }
}
