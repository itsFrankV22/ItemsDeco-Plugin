# ItemDeco 插件

- 现已兼容 **[Floating-MessageChat](https://github.com/itsFrankV22/FloatingText-Chat)**

Si hablas otro idioma por favor visita: **[Spanish](https://github.com/itsFrankV22/ItemSuffixBelowName/blob/master/READMESpanish.md)**
If you speak another language please visit: **[English](https://github.com/itsFrankV22/ItemSuffixBelowName/blob/master/README.md)**

## 描述

**Item Suffix 插件**是 **TShock** 的一个扩展插件，它会在聊天中以及玩家头顶以浮动消息的形式显示玩家手中正在持有的物品。可以通过配置文件进行设置。

## 功能

- **显示物品名称**：当玩家切换持有的物品时，该物品的名称会以浮动消息的形式出现在玩家头顶，并在聊天中显示。此功能还支持显示伤害值，并可通过配置文件启用或禁用。
- **颜色自定义**：浮动消息的颜色默认为白色（RGB 值为 255,255,255），可以自定义。
- **支持外部配置**：插件支持从外部 JSON 文件 `ItemDecoConfig.json` 加载配置文件，文件位于 `tshock/ItemDeco/` 文件夹中，可自定义颜色和显示内容。

## 安装方法

1. **下载插件**：获取插件的 DLL 文件。
2. **放置到插件文件夹**：将 DLL 文件复制到 TShock 安装目录下的 `plugins` 文件夹。
3. **重启服务器**：重启服务器以加载插件。

## 使用方法

- 插件是即插即用的，安装后无需额外操作。
- 插件激活后，会自动开始运行。当玩家切换物品时，物品名称会自动显示。
- 配置文件 `ItemDecoConfig.json` 位于 `tshock/ItemDeco/` 文件夹中，可用于自定义浮动消息的颜色及其他参数。如果文件不存在，插件会自动生成一个默认配置文件。

## 权限

无需额外权限，所有玩家都可以看到自己持有物品的名称。

## 要求

- **TShock**：该插件需要 TShock 环境支持。
- **TShock 版本**：设计与 TShock API v2.1 兼容。

## 贡献

如果您希望为此插件开发做出贡献，可以自由地 fork 仓库并提交改进或修复。任何反馈都非常欢迎！

## 作者

- **[FrankV22](https://github.com/itsFrankV22)**：主开发者  
- **[Soofa](https://github.com/Soof4)**：贡献者  

## 许可协议

此项目基于 MIT 许可协议。更多细节请参考 LICENSE 文件。

## 支持

如果您在使用过程中遇到问题或有疑问，可以在仓库中提交 Issue 或直接联系我们。

---

使用 **ItemDeco**，提升您的 Terraria 游戏体验！
