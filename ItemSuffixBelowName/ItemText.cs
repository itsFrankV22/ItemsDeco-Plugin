using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Newtonsoft.Json;

namespace ItemTextPlugin
{
    [ApiVersion(2, 1)]
    public class ItemTextPlugin : TerrariaPlugin
    {
        public override string Name => "ItemDecoration";
        public override string Author => "FrankV22, Soofa";
        public override string Description => "Muestra el nombre del ítem que el jugador sostiene como mensaje flotante al cambiar de objeto";
        public override Version Version => new Version(2, 0, 5);

        private Dictionary<string, int> lastSelectedItems = new();
        private ItemTextConfig config;

        public ItemTextPlugin(Main game) : base(game)
        {
            LoadConfig();
        }

        public override void Initialize()
        {
            LoadConfig();

            ServerApi.Hooks.NetGreetPlayer.Register(this, OnNetGreetPlayer);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
            TShockAPI.GetDataHandlers.PlayerUpdate += OnPlayerUpdate;
            Commands.ChatCommands.Add(new Command("itemSuffix.reload", ReloadConfig, "reload"));
        }

        private void LoadConfig()
        {
            string configDirectory = Path.Combine(TShock.SavePath, "ItemDeco");

            if (!Directory.Exists(configDirectory))
            {
                Directory.CreateDirectory(configDirectory);
            }

            string filePath = Path.Combine(configDirectory, "ItemTextConfig.json");

            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    config = JsonConvert.DeserializeObject<ItemTextConfig>(json);
                }
                else
                {
                    // Configuración por defecto
                    config = new ItemTextConfig
                    {
                        ItemFloatingMsg = true,
                        ITEMS = new ItemConfig
                        {
                            specificColors = new Dictionary<string, ColorConfig>(),
                            defaultColor = new ColorConfig { r = 255, g = 255, b = 255 }
                        }
                    };
                    SaveConfig(filePath);
                }
            }
            catch (Exception ex)
            {
                TShock.Log.Error("Error al cargar la configuración de ItemTextPlugin: " + ex.Message);
                // Configuración predeterminada en caso de error
                config = new ItemTextConfig
                {
                    ItemFloatingMsg = true,
                    ITEMS = new ItemConfig
                    {
                        specificColors = new Dictionary<string, ColorConfig>(),
                        defaultColor = new ColorConfig { r = 255, g = 255, b = 255 }
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
                TShock.Log.Error("Error al guardar la configuración de ItemTextPlugin: " + ex.Message);
            }
        }

        private void OnNetGreetPlayer(GreetPlayerEventArgs args)
        {
            var player = TShock.Players[args.Who];
            if (player != null && !lastSelectedItems.ContainsKey(player.Name))
            {
                lastSelectedItems.Add(player.Name, player.TPlayer.inventory[player.TPlayer.selectedItem].type);
            }
        }

        private void OnServerLeave(LeaveEventArgs args)
        {
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

            if (lastSelectedItems.ContainsKey(player.Name) && selectedItem.type != lastSelectedItems[player.Name])
            {
                if (config.ItemFloatingMsg)
                {
                    SendFloatingMsg(player, selectedItem.Name);
                }
                lastSelectedItems[player.Name] = selectedItem.type;
            }
        }

        private void SendFloatingMsg(TSPlayer plr, string msg)
        {
            if (config.ItemFloatingMsg)
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
            LoadConfig();
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

    public class ItemTextConfig
    {
        public bool ItemFloatingMsg { get; set; }
        public bool ItemChatSuffix { get; set; }
        public ItemConfig ITEMS { get; set; }
    }

    public class ItemConfig
    {
        public Dictionary<string, ColorConfig> specificColors { get; set; }
        public ColorConfig defaultColor { get; set; }
    }

    public class ColorConfig
    {
        public int r { get; set; }
        public int g { get; set; }
        public int b { get; set; }
    }
}