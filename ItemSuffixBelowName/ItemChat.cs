using TerrariaApi.Server;
using TShockAPI;
using Terraria;
using Microsoft.Xna.Framework;  // Para usar Color
using System.Reflection;  // Para trabajar con PropertyInfo
using System.IO;
using Newtonsoft.Json;

namespace ItemSuffixPlugin
{
    [ApiVersion(2, 1)]
    public class ItemChatPlugin : TerrariaPlugin
    {
        private Config config;

        public ItemChatPlugin(Main game) : base(game)
        {
            LoadConfig();
        }

        public override void Initialize()
        {
            // Registrar el evento para interceptar los mensajes de chat
            ServerApi.Hooks.ServerChat.Register(this, OnServerChat);
        }

        // Evento que se ejecuta cuando un jugador envía un mensaje
        private void OnServerChat(ServerChatEventArgs args)
        {
            // Solo se ejecuta si está habilitado el sufijo en el chat
            if (config.CONFIGURATION.ItemChatSuffix)
            {
                // Obtener el jugador usando el índice 'Who'
                TSPlayer player = TShock.Players[args.Who];
                if (player == null) return;

                string message = args.Text;

                // Modificar el mensaje añadiendo el sufijo con el ID del ítem que el jugador sostiene
                message = ItemSuffixPlaceholder.ReplacePlaceholderWithItem(player, message);

                // Actualizar el texto del mensaje con el sufijo
                PropertyInfo? propertyInfo = args.GetType().GetProperty("Text");
                propertyInfo?.SetValue(args, message);
            }
        }
        private void LoadConfig()
        {
            // Ruta de la carpeta "ItemDeco" dentro del directorio de TShock
            string configDirectory = Path.Combine(TShock.SavePath, "ItemDeco");

            // Crear la carpeta si no existe
            if (!Directory.Exists(configDirectory))
            {
                Directory.CreateDirectory(configDirectory);
            }

            // Ruta del archivo de configuración dentro de la carpeta "ItemDeco"
            string filePath = Path.Combine(configDirectory, "ItemDecoConfig.json");

            // Cargar la configuración si el archivo existe
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                config = JsonConvert.DeserializeObject<Config>(json);
            }
            else
            {
                // Configuración por defecto si no existe el archivo
                config = new Config
                {
                    CONFIGURATION = new Configuration
                    {
                        ItemFloatingMsg = true,
                        ItemChatSuffix = true
                    },
                    ITEMS = new ItemConfig
                    {
                        defaultColor = new ColorConfig { r = 0, g = 255, b = 255 },
                        specificColors = new Dictionary<string, ColorConfig>
                {
                    { "MagicItem", new ColorConfig { r = 255, g = 100, b = 100 } },
                    { "RareItem", new ColorConfig { r = 100, g = 100, b = 255 } }
                }
                    }
                };
                SaveConfig(filePath);
            }
        }

        private void SaveConfig(string filePath)
        {
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Desregistrar el evento cuando el plugin se desactive
                ServerApi.Hooks.ServerChat.Deregister(this, OnServerChat);
            }
            base.Dispose(disposing);
        }
    }

    public static class ItemSuffixPlaceholder
    {
        public static string ReplacePlaceholderWithItem(TSPlayer player, string message)
        {
            // Obtener el ítem que el jugador sostiene
            var selectedItem = player.TPlayer.inventory[player.TPlayer.selectedItem];

            // Asegurarnos de que el jugador tiene un ítem válido en la mano
            if (selectedItem != null && selectedItem.type > 0)
            {
                // Agregar el sufijo con el ID del ítem en el formato [i:id]
                return $"{message} [i:{selectedItem.netID}]";
            }

            // Si no tiene ítem, devolver el mensaje original
            return message;
        }
    }

    public class Config
    {
        public Configuration CONFIGURATION { get; set; }
        public ItemConfig ITEMS { get; set; }
    }

    public class Configuration
    {
        public bool ItemFloatingMsg { get; set; }
        public bool ItemChatSuffix { get; set; }
    }

    public class ItemConfig
    {
        public ColorConfig defaultColor { get; set; }
        public Dictionary<string, ColorConfig> specificColors { get; set; }
    }

    public class ColorConfig
    {
        public byte r { get; set; }
        public byte g { get; set; }
        public byte b { get; set; }
    }
}
