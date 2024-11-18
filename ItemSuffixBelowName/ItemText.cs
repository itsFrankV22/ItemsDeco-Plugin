using System;
using System.Collections.Generic;
using System.IO;
using ConfigPlugin;
using Newtonsoft.Json;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace ItemTextPlugin
{
    [ApiVersion(2, 1)]
    public class InfoPlugin : TerrariaPlugin
    {
        private Dictionary<string, int> lastSelectedItems = new();
        private ItemTextConfig config;

        public InfoPlugin(Main game) : base(game)
        {
            string configDirectory = Path.Combine(TShock.SavePath, "ItemDeco");
            string configPath = Path.Combine(configDirectory, "ItemTextConfig.json");

            // Cargar configuración o regenerar si está desactualizada
            config = ConfigManager.LoadConfig(
                configDirectory,
                "ItemTextConfig.json",
                new ItemTextConfig
                {
                    ShowName = true,
                    ShowDamage = true,
                    DefaultColor = new ColorConfig { r = 255, g = 255, b = 255 },
                    RarityColors = new Dictionary<int, ColorConfig>
                    {
                { -1, new ColorConfig { r = 128, g = 128, b = 128 } }, // Gris para sin rareza
                { 0, new ColorConfig { r = 255, g = 255, b = 255 } }, // Blanco
                { 1, new ColorConfig { r = 0, g = 128, b = 0 } },     // Verde
                { 2, new ColorConfig { r = 0, g = 112, b = 221 } },   // Azul
                { 3, new ColorConfig { r = 128, g = 0, b = 128 } },   // Morado
                { 4, new ColorConfig { r = 255, g = 128, b = 0 } },   // Naranja
                { 5, new ColorConfig { r = 255, g = 0, b = 0 } },     // Rojo
                    }
                });

            // Verificar si la configuración está desactualizada
            if (IsConfigOutdated(config))
            {
                // Borrar la configuración desactualizada
                if (File.Exists(configPath))
                {
                    File.Delete(configPath);
                }

                // Regenerar configuración predeterminada
                config = ConfigManager.LoadConfig(
                    configDirectory,
                    "ItemTextConfig.json",
                    new ItemTextConfig
                    {
                        ShowName = true,
                        ShowDamage = true,
                        DefaultColor = new ColorConfig { r = 255, g = 255, b = 255 },
                        RarityColors = new Dictionary<int, ColorConfig>
                        {
                    { -1, new ColorConfig { r = 128, g = 128, b = 128 } }, // Gris para sin rareza
                    { 0, new ColorConfig { r = 255, g = 255, b = 255 } }, // Blanco
                    { 1, new ColorConfig { r = 0, g = 128, b = 0 } },     // Verde
                    { 2, new ColorConfig { r = 0, g = 112, b = 221 } },   // Azul
                    { 3, new ColorConfig { r = 128, g = 0, b = 128 } },   // Morado
                    { 4, new ColorConfig { r = 255, g = 128, b = 0 } },   // Naranja
                    { 5, new ColorConfig { r = 255, g = 0, b = 0 } },     // Rojo
                        }
                    });
                TShock.Log.Info("[ ITEMS DECORATION ] Configuración desactualizada detectada. Archivo regenerado.");
            }
        }
        private bool IsConfigOutdated(ItemTextConfig config)
        {
            // Comprobar si las rarezas están definidas
            return config.RarityColors == null || config.RarityColors.Count == 0;
        }


        public override void Initialize()
        {
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnNetGreetPlayer);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
            TShockAPI.GetDataHandlers.PlayerUpdate += OnPlayerUpdate;
            Commands.ChatCommands.Add(new Command("itemSuffix.reload", ReloadConfig, "reload"));
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
                if (config.ShowName || config.ShowDamage)
                {
                    string message = "";
                    if (config.ShowName)
                    {
                        message += selectedItem.Name;
                    }
                    if (config.ShowDamage && selectedItem.damage > 0) // Verificamos que el daño sea mayor que 0
                    {
                        if (!string.IsNullOrEmpty(message))
                            message += " - ";
                        message += $"Daño: {selectedItem.damage}";
                    }

                    SendFloatingMsg(player, message, GetColorByRarity(selectedItem.rare));
                }
                lastSelectedItems[player.Name] = selectedItem.type;
            }
        }
        private void SendFloatingMsg(TSPlayer plr, string msg, Microsoft.Xna.Framework.Color color)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                NetMessage.SendData((int)PacketTypes.CreateCombatTextExtended, -1, -1,
                    Terraria.Localization.NetworkText.FromLiteral(msg),
                    (int)color.PackedValue,
                    plr.TPlayer.position.X + 16, plr.TPlayer.position.Y + 33);
            }
        }

        private Microsoft.Xna.Framework.Color GetColorByRarity(int rarity)
        {
            if (config.RarityColors.TryGetValue(rarity, out var colorConfig))
            {
                return new Microsoft.Xna.Framework.Color(colorConfig.r, colorConfig.g, colorConfig.b);
            }
            return new Microsoft.Xna.Framework.Color(config.DefaultColor.r, config.DefaultColor.g, config.DefaultColor.b);
        }

        private void ReloadConfig(CommandArgs args)
        {
            string configDirectory = Path.Combine(TShock.SavePath, "ItemDeco");
            config = ConfigManager.LoadConfig(
                configDirectory,
                "ItemTextConfig.json",
                new ItemTextConfig
                {
                    ShowName = true,
                    ShowDamage = true,
                    DefaultColor = new ColorConfig { r = 255, g = 255, b = 255 },
                    RarityColors = new Dictionary<int, ColorConfig>()
                });

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
        public bool ShowName { get; set; }
        public bool ShowDamage { get; set; }
        public ColorConfig DefaultColor { get; set; }
        public Dictionary<int, ColorConfig> RarityColors { get; set; }
    }

    public class ColorConfig
    {
        public int r { get; set; }
        public int g { get; set; }
        public int b { get; set; }
    }
}
