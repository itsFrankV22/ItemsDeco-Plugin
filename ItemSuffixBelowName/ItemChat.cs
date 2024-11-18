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
        public static ConfigChat Config;  // Hacer la configuración estática

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
            if (args.Text.StartsWith("/") || args.Text.StartsWith(".") || args.Text.StartsWith("!"))
                return;

            if (Config.CONFIGURATION.ItemChatSuffix)
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
                    Config = JsonConvert.DeserializeObject<ConfigChat>(json);
                }
                else
                {
                    Config = new ConfigChat
                    {
                        CONFIGURATION = new Configuration
                        {
                            ItemChatSuffix = true,
                            ShowName = true,
                            ShowDamage = true
                        }
                    };
                    SaveConfig(filePath);
                }
            }
            catch (Exception ex)
            {
                TShock.Log.Error("Error al cargar la configuración de ItemChatPlugin: " + ex.Message);
                Config = new ConfigChat
                {
                    CONFIGURATION = new Configuration
                    {
                        ItemChatSuffix = true,
                        ShowName = true,
                        ShowDamage = true
                    }
                };
            }
        }

        private void SaveConfig(string filePath)
        {
            try
            {
                string json = JsonConvert.SerializeObject(Config, Formatting.Indented);
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

            // Verificar si el objeto seleccionado es válido
            if (selectedItem != null && selectedItem.type > 0)
            {
                string suffix = "";

                // Mostrar el nombre si está configurado
                if (ItemChatPlugin.Config.CONFIGURATION.ShowName)
                {
                    suffix += $"[i:{selectedItem.netID}]";
                }

                // Mostrar el daño si está configurado
                if (ItemChatPlugin.Config.CONFIGURATION.ShowDamage && selectedItem.damage > 0)
                {
                    suffix += $" [{selectedItem.damage}]";
                }

                // Si hay un sufijo, agregarlo al mensaje
                if (!string.IsNullOrEmpty(suffix))
                {
                    return $"[c/00ffff:[] {suffix} [c/00ffff:]>] {message}";
                }
            }

            // Si no hay un objeto válido o no se muestra nada, devolver el mensaje original
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
        public bool ShowName { get; set; }
        public bool ShowDamage { get; set; }
    }
}
