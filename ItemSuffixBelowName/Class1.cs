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
        public override string Name => "Item Suffix Plugin";
        public override string Author => "FrankV22, Soofa";
        public override string Description => "Muestra el nombre del ítem que el jugador sostiene como mensaje flotante al cambiar de objeto";
        public override Version Version => new Version(1, 0, 7);

        private Dictionary<string, int> lastSelectedItems = new();
        private FloatingMsgColorConfig colorsConfig;

        public ItemSuffixPlugin(Main game) : base(game)
        {
            // Se carga la configuración al crear la instancia del plugin
            LoadColorsConfig();
        }

        public override void Initialize()
        {
            // Se registra el evento de saludo a los jugadores
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnNetGreetPlayer);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
            TShockAPI.GetDataHandlers.PlayerUpdate += OnPlayerUpdate;
            Commands.ChatCommands.Add(new Command("itemSuffix.reload", ReloadConfig, "reloadItemSuffix"));
        }

        private void LoadColorsConfig()
        {
            string filePath = Path.Combine(TShock.SavePath, "itembelowcolor.json");
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                colorsConfig = JsonConvert.DeserializeObject<FloatingMsgColorConfig>(json);
            }
            else
            {
                // Crear un archivo con la configuración predeterminada
                colorsConfig = new FloatingMsgColorConfig
                {
                    defaultColor = new ColorConfig { r = 0, g = 255, b = 255 },
                    specificColors = new Dictionary<string, ColorConfig>()
                };
                SaveColorsConfig(filePath);
            }
        }

        private void SaveColorsConfig(string filePath)
        {
            string json = JsonConvert.SerializeObject(colorsConfig, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        private void OnNetGreetPlayer(GreetPlayerEventArgs args)
        {
            var player = TShock.Players[args.Who];
            if (!lastSelectedItems.ContainsKey(player.Name))
            {
                lastSelectedItems.Add(player.Name, player.TPlayer.inventory[player.TPlayer.selectedItem].netID);
            }
        }

        private void OnServerLeave(LeaveEventArgs args)
        {
            var player = TShock.Players[args.Who];
            if (lastSelectedItems.ContainsKey(player.Name))
            {
                lastSelectedItems.Remove(player.Name);
            }
        }

        private void OnPlayerUpdate(object sender, GetDataHandlers.PlayerUpdateEventArgs args)
        {
            var player = args.Player;
            var selectedItem = player.TPlayer.inventory[player.TPlayer.selectedItem];

            // Verificar si el jugador ha cambiado el objeto en la mano
            if (lastSelectedItems.ContainsKey(player.Name) && selectedItem.netID != lastSelectedItems[player.Name])
            {
                // Enviar mensaje flotante con el nombre del ítem sostenido
                SendFloatingMsg(player, selectedItem.Name);

                // Actualizar el último ítem seleccionado en el diccionario
                lastSelectedItems[player.Name] = selectedItem.netID;
            }
        }

        private void SendFloatingMsg(TSPlayer plr, string msg)
        {
            ColorConfig color;

            // Comprueba si hay un color específico para el ítem
            if (colorsConfig.specificColors.TryGetValue(msg, out color))
            {
                // Usa el color específico
                NetMessage.SendData((int)PacketTypes.CreateCombatTextExtended, -1, -1,
                    Terraria.Localization.NetworkText.FromLiteral(msg), (int)new Microsoft.Xna.Framework.Color(color.r, color.g, color.b).PackedValue,
                    plr.TPlayer.position.X + 16, plr.TPlayer.position.Y + 32);
            }
            else
            {
                // Usa el color por defecto
                NetMessage.SendData((int)PacketTypes.CreateCombatTextExtended, -1, -1,
                    Terraria.Localization.NetworkText.FromLiteral(msg), (int)new Microsoft.Xna.Framework.Color(colorsConfig.defaultColor.r, colorsConfig.defaultColor.g, colorsConfig.defaultColor.b).PackedValue,
                    plr.TPlayer.position.X + 16, plr.TPlayer.position.Y + 32);
            }
        }

        private void ReloadConfig(CommandArgs args)
        {
            LoadColorsConfig();
            args.Player.SendMessage("La configuración de colores ha sido recargada.", Microsoft.Xna.Framework.Color.LightGreen);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnNetGreetPlayer);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
                TShockAPI.GetDataHandlers.PlayerUpdate -= OnPlayerUpdate;
                Commands.ChatCommands.RemoveAll(c => c.Name == "reloadItemSuffix");
            }
            base.Dispose(disposing);
        }
    }

    // Clase para manejar los colores del mensaje flotante
    public class FloatingMsgColorConfig
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