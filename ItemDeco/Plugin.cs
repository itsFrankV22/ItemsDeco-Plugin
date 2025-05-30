﻿using ItemDecoration.Configured;
using LazyAPI;
using LazyAPI.Extensions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace ItemDecoration;

[ApiVersion(2, 1)]
public class Plugin : LazyPlugin
{
    public override string Author => "FrankV22, Soofa, 少司命";
    public override string Description => "Show Item Decoration and More!!!";
    public override string Name => System.Reflection.Assembly.GetExecutingAssembly().GetName().Name!;
    public override Version Version => new Version(3, 1, 2);

    private int serverPort;

    public Plugin(Main game) : base(game) { }

    public override void Initialize()
    {
        try
        {
            ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnPlayerJoin);
            PlayerHooks.PlayerChat += OnPlayerChat;
            On.OTAPI.Hooks.MessageBuffer.InvokeGetData += this.MessageBuffer_InvokeGetData;

            Console.WriteLine($"\x1b[106;30;1m {Name} {Version} by {Author} \x1b[0m");
        }
        catch (Exception ex)
        {
            Task.Run(() => Telemetry.Report(ex));
            TShock.Log.ConsoleError($"[{nameof(ItemDecoration)}] Error Initialize: {ex.Message}");
        }
    }

    private async void OnPostInitialize(EventArgs e)
    {
        try
        {
            Telemetry.Start(this);

            await CheckUpdates.CheckUpdates.CheckUpdateVerbose(this);
        }
        catch (Exception ex)
        {
            await Telemetry.Report(ex);
            TShock.Log.ConsoleError($"[ {Name} ] Error OnPostInitialize: {ex.Message}");
        }
    }

    private async void OnPlayerJoin(GreetPlayerEventArgs args)
    {
        try
        {
            await Task.Delay(1500);
            var player = TShock.Players[args.Who];
            player.SendInfoMessage($"[c/44FFC2:[] [c/10F131:+] [c/44FFC2:]] [c/44FFC2:This server is powered by ItemDecoration v{Version} by {Author}]");
        }
        catch (Exception ex)
        {
            await Telemetry.Report(ex);
            TShock.Log.ConsoleError($"[{nameof(ItemDecoration)}] Error en OnPlayerJoin: {ex.Message}");
        }
    }

    private void OnPlayerChat(PlayerChatEventArgs args)
    {
        try
        {
            var player = args.Player;
            if (args.Handled || player == null
                || args.RawText.StartsWith(TShock.Config.Settings.CommandSilentSpecifier)
                || args.RawText.StartsWith(TShock.Config.Settings.CommandSpecifier)
                || string.IsNullOrWhiteSpace(args.RawText))
            {
                return;
            }

            var msg = ReplacePlaceholderWithItem(player, args.RawText);
            if (!string.IsNullOrWhiteSpace(msg))
            {
                TShock.Utils.Broadcast(string.Format(
                    TShock.Config.Settings.ChatFormat,
                    player.Group.Name,
                    player.Group.Prefix,
                    player.Name,
                    player.Group.Suffix,
                    msg),
                    player.Group.R, player.Group.G, player.Group.B);
            }
            args.Handled = true;
        }
        catch (Exception ex)
        {
            Task.Run(() => Telemetry.Report(ex));
            TShock.Log.ConsoleError($"[{nameof(ItemDecoration)}] Error OnPlayerChat: {ex.Message}");
        }
    }

    private bool MessageBuffer_InvokeGetData(On.OTAPI.Hooks.MessageBuffer.orig_InvokeGetData orig, MessageBuffer instance, ref byte packetId, ref int readOffset, ref int start, ref int length, ref int messageType, int maxPackets)
    {
        try
        {
            if (packetId == 13) // Packet ID 13 - Item select
            {
                using var ms = new MemoryStream(instance.readBuffer);
                ms.Position = readOffset;
                using var reader = new BinaryReader(ms);
                var index = reader.ReadByte();
                var player = TShock.Players[index];

                if (player == null || !player.Active || player.Dead)
                    return orig(instance, ref packetId, ref readOffset, ref start, ref length, ref messageType, maxPackets);

                reader.BaseStream.Seek(4, SeekOrigin.Current);
                var selectSlot = reader.ReadByte();

                if (player.TPlayer.selectedItem != selectSlot)
                {
                    var newSelectItem = player.TPlayer.inventory[selectSlot];

                    if (Setting.Instance.ItemTextConfig.ShowName || Setting.Instance.ItemTextConfig.ShowDamage)
                    {
                        var message = "";
                        if (Setting.Instance.ItemTextConfig.ShowName)
                        {
                            message += newSelectItem.Name;
                        }
                        if (Setting.Instance.ItemTextConfig.ShowDamage && newSelectItem.damage > 0)
                        {
                            if (!string.IsNullOrEmpty(message))
                            {
                                message += " - ";
                            }
                            message += $"{Setting.Instance.ItemTextConfig.DamageText}: {newSelectItem.damage}";
                        }
                        player.SendCombatText(message, GetColorByRarity(newSelectItem.rare));
                    }

                    if (Setting.Instance.ItemAboveHeadConfig.ItemAboveHead)
                    {
                        if (newSelectItem != null && newSelectItem.type != ItemID.None)
                        {
                            if (!lastSelectedItem.ContainsKey(player.Index) || lastSelectedItem[player.Index] != newSelectItem.type)
                            {
                                lastSelectedItem[player.Index] = newSelectItem.type;

                                ParticleOrchestraSettings settings = new()
                                {
                                    IndexOfPlayerWhoInvokedThis = (byte)player.Index,
                                    MovementVector = new Vector2(0, -24),
                                    PositionInWorld = player.TPlayer.Center + new Vector2(0, -24),
                                    UniqueInfoPiece = newSelectItem.type
                                };
                                ParticleOrchestrator.BroadcastParticleSpawn(ParticleOrchestraType.ItemTransfer, settings);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Task.Run(() => Telemetry.Report(ex));
            TShock.Log.ConsoleError($"[{nameof(ItemDecoration)}] Error en MessageBuffer_InvokeGetData: {ex.Message}");
        }
        return orig(instance, ref packetId, ref readOffset, ref start, ref length, ref messageType, maxPackets);
    }

    private Dictionary<int, int> lastSelectedItem = new Dictionary<int, int>();

    private static Microsoft.Xna.Framework.Color GetColorByRarity(int rarity)
    {
        try
        {
            return Setting.Instance.ItemTextConfig.RarityColors.TryGetValue(rarity, out var colorConfig)
                ? new Microsoft.Xna.Framework.Color(colorConfig.R, colorConfig.G, colorConfig.B)
                : new Microsoft.Xna.Framework.Color(Setting.Instance.ItemTextConfig.DefaultColor.R, Setting.Instance.ItemTextConfig.DefaultColor.G, Setting.Instance.ItemTextConfig.DefaultColor.B);
        }
        catch (Exception ex)
        {
            Task.Run(() => Telemetry.Report(ex));
            return new Microsoft.Xna.Framework.Color(255, 255, 255);
        }
    }

    public static string ReplacePlaceholderWithItem(TSPlayer player, string message)
    {
        try
        {
            var selectedItem = player.TPlayer.inventory[player.TPlayer.selectedItem];
            var suffix = "";

            if (selectedItem != null && selectedItem.type > 0)
            {
                var damageColorHex = Setting.Instance.ItemChatConfig.DamageColor.ToHex();

                if (Setting.Instance.ItemChatConfig.ShowName)
                {
                    suffix += $"[i:{selectedItem.netID}]";
                }

                if (Setting.Instance.ItemChatConfig.ShowDamage && selectedItem.damage > 0)
                {
                    if (!string.IsNullOrEmpty(suffix))
                    {
                        suffix += " ";
                    }
                    suffix += $"[c/{damageColorHex}:{selectedItem.damage}]";
                }
            }
            return !string.IsNullOrEmpty(suffix) ? $"[ {suffix} ] {message}." : message;
        }
        catch (Exception ex)
        {
            Task.Run(() => Telemetry.Report(ex));
            return message;
        }
    }
}