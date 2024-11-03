using System;
using System.Collections.Generic;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace ItemSuffixPlugin
{
    [ApiVersion(2, 1)]
    public class ItemSuffixPlugin : TerrariaPlugin
    {
        public override string Name => "Item Suffix Plugin";
        public override string Author => "FrankV22, Soofa";
        public override string Description => "Muestra el nombre del ítem que el jugador sostiene como mensaje flotante al cambiar de objeto";
        public override Version Version => new Version(1, 0, 4);

        private Dictionary<string, int> lastSelectedItems = new();

        public ItemSuffixPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnNetGreetPlayer);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
            TShockAPI.GetDataHandlers.PlayerUpdate += OnPlayerUpdate;
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
                SendFloatingMsg(player, selectedItem.Name, 0, 255, 255);

                // Actualizar el último ítem seleccionado en el diccionario
                lastSelectedItems[player.Name] = selectedItem.netID;
            }
        }

        private static void SendFloatingMsg(TSPlayer plr, string msg, byte r, byte g, byte b)
        {
            NetMessage.SendData((int)PacketTypes.CreateCombatTextExtended, -1, -1,
                Terraria.Localization.NetworkText.FromLiteral(msg), (int)new Microsoft.Xna.Framework.Color(r, g, b).PackedValue,
                plr.TPlayer.position.X + 16, plr.TPlayer.position.Y + 32);
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
