using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Newtonsoft.Json;

namespace ItemSuffixPlugin
{
    [ApiVersion(2, 1)]
    public class ItemSuffixPlugin : TerrariaPlugin
    {
        public override string Name => "ItemDecoration";
        public override string Author => "FrankV22, Soofa";
        public override string Description => "Muestra el nombre del ítem que el jugador sostiene como mensaje flotante al cambiar de objeto";
        public override Version Version => new Version(2, 0, 0);

        private Dictionary<string, int> lastSelectedItems = new();
        private Config config;

        public ItemSuffixPlugin(Main game) : base(game)
        {
            LoadConfig();
        }

        public override void Initialize()
        {
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnNetGreetPlayer);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
            TShockAPI.GetDataHandlers.PlayerUpdate += OnPlayerUpdate;
            Commands.ChatCommands.Add(new Command("itemSuffix.reload", ReloadConfig, "reload"));
        }

        private void LoadConfig()
        {
            // Define la ruta de la carpeta ItemDeco dentro de TShock
            string configDirectory = Path.Combine(TShock.SavePath, "ItemDeco");

            // Crear la carpeta si no existe
            if (!Directory.Exists(configDirectory))
            {
                Directory.CreateDirectory(configDirectory);
            }

            // Ruta del archivo de configuración dentro de la carpeta ItemDeco
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

        private void OnNetGreetPlayer(GreetPlayerEventArgs args)
        {
            var player = TShock.Players[args.Who];
            if (player != null && !lastSelectedItems.ContainsKey(player.Name))
            {
                lastSelectedItems.Add(player.Name, player.TPlayer.inventory[player.TPlayer.selectedItem].netID);
            }
        }

        private void OnServerLeave(LeaveEventArgs args)
        {
            // Verificar que args y args.Who no sean null
            var player = TShock.Players[args.Who];
            if (player != null && lastSelectedItems.ContainsKey(player.Name))
            {
                lastSelectedItems.Remove(player.Name);
            }
        }

        private void OnPlayerUpdate(object sender, GetDataHandlers.PlayerUpdateEventArgs args)
        {
            var player = args.Player;
            if (player == null) return;

            var selectedItem = player.TPlayer.inventory[player.TPlayer.selectedItem];

            if (lastSelectedItems.ContainsKey(player.Name) && selectedItem.netID != lastSelectedItems[player.Name])
            {
                if (config.CONFIGURATION.ItemFloatingMsg)
                {
                    SendFloatingMsg(player, selectedItem.Name);
                }
                lastSelectedItems[player.Name] = selectedItem.netID;
            }
        }

        private void SendFloatingMsg(TSPlayer plr, string msg)
        {
            if (config.CONFIGURATION.ItemFloatingMsg)  // Solo ejecuta si está activado
            {
                ColorConfig color;

                if (config.ITEMS.specificColors.TryGetValue(msg, out color))
                {
                    NetMessage.SendData((int)PacketTypes.CreateCombatTextExtended, -1, -1,
                        Terraria.Localization.NetworkText.FromLiteral(msg), (int)new Microsoft.Xna.Framework.Color(color.r, color.g, color.b).PackedValue,
                        plr.TPlayer.position.X + 16, plr.TPlayer.position.Y + 32);
                }
                else
                {
                    NetMessage.SendData((int)PacketTypes.CreateCombatTextExtended, -1, -1,
                        Terraria.Localization.NetworkText.FromLiteral(msg), (int)new Microsoft.Xna.Framework.Color(config.ITEMS.defaultColor.r, config.ITEMS.defaultColor.g, config.ITEMS.defaultColor.b).PackedValue,
                        plr.TPlayer.position.X + 16, plr.TPlayer.position.Y + 32);
                }
            }
        }

        private void ReloadConfig(CommandArgs args)
        {
            LoadConfig();  // Recarga la configuración desde el archivo
            args.Player.SendMessage("[ ITEMS DECORATION ] La configuración ha sido recargada.", Microsoft.Xna.Framework.Color.LightGreen);
            TShock.Log.Info("[ ITEMS DECORATION ] La configuración ha sido recargada correctamente.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnNetGreetPlayer);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
                TShockAPI.GetDataHandlers.PlayerUpdate -= OnPlayerUpdate;
            }
            base.Dispose(disposing);
        }
    }
}
