using System.Text;
using TShockAPI;
using Microsoft.Xna.Framework;
using static OnlineGift.OnlineGift;

namespace OnlineGift;

internal class Commands
{
    #region 礼包指令方法
    public static void GiftCMD(CommandArgs args)
    {
        if (Config is null || !Config.Enabled) return;

        var plr = args.Player;

        if (args.Parameters.Count == 0)
        {
            HelpCmd(plr);
        }

        if (args.Parameters.Count >= 1)
        {
            switch (args.Parameters[0].ToLower())
            {
                case "on":
                case "启用":
                    {
                        if (!plr.HasPermission(Config!.IsAdamin)) return;

                        Config.Enabled = true;
                        Config.Write();
                        plr.SendMessage("在线礼包已启用", color);
                    }
                    break;
                case "off":
                case "关闭":
                    {
                        if (!plr.HasPermission(Config!.IsAdamin)) return;

                        Config.Enabled = false;
                        Config.Write();
                        plr.SendMessage("在线礼包已关闭", color);
                    }
                    break;
                case "t":
                case "time":
                case "时间":
                    {
                        if (!plr.HasPermission(Config!.IsAdamin)) return;

                        if (args.Parameters.Count > 1 && int.TryParse(args.Parameters[1], out var num))
                        {
                            var oldTimer = FormatTime(Config!.SendTimer * 60);
                            Config.SendTimer = num;
                            Config.Write();
                            string nextTime = FormatTime(Config!.SendTimer * 60);
                            plr.SendMessage($"在线礼包发放时间: [c/47D3C3:{oldTimer}] → [c/F38152:{nextTime}] ", color);
                        }
                        else
                        {
                            plr.SendMessage("用法：/gift t 秒数", color);
                        }
                    }
                    break;
                case "s":
                case "show":
                case "剩余":
                    {
                        if (plr == TSPlayer.Server)
                        {
                            plr.SendInfoMessage("请进入游戏后再使用该指令");
                            return;
                        }

                        if (!players.ContainsKey(plr.Index))
                        {
                            plr.SendInfoMessage("您还未开始计时，请稍等片刻。");
                            players[plr.Index] = 0;
                            return;
                        }

                        string remaining = FormatTime(Config!.SendTimer * 60 - players[plr.Index]);
                        plr.SendInfoMessage($"距离下次礼包发放还有: [c/F38152:{remaining}]");
                    }
                    break;
                case "l":
                case "ls":
                case "list":
                case "列表":
                    {
                        StringBuilder mess = new StringBuilder();

                        // 添加标题行
                        mess.AppendLine("在线礼包概率表：\n");

                        // 显示所有礼包的获取概率，按每5个一组分批显示
                        for (int i = 0; i < Config!.GiftList.Count; i++)
                        {
                            GiftData gift = Config.GiftList[i];
                            double rate = 100.0 * ((double)gift.Rate / Config.Total);
                            mess.Append($"[c/47D3C3:{i + 1:00}.][i/s1:{gift.ItemType}]{rate:0}% ");

                            // 每显示5个礼包后换行
                            if ((i + 1) % 5 == 0)
                            {
                                mess.AppendLine();
                            }
                        }

                        mess.AppendLine($"\n{Config.GiftList.Count}个礼包的总概率为：{Config.TotalRate()}%");
                        plr.SendMessage(mess.ToString(), color);
                    }
                    break;
                case "a":
                case "add":
                case "添加":
                    {
                        if (!plr.HasPermission(Config!.IsAdamin)) return;
                        AddGiftItem(args);
                    }
                    break;
                case "d":
                case "del":
                case "删除":
                    {
                        if (!plr.HasPermission(Config!.IsAdamin)) return;
                        RemoveGiftItem(args);
                    }
                    break;
                case "rs":
                case "reset":
                case "重置":
                    {
                        if (!plr.HasPermission(Config!.IsAdamin)) return;
                        Config.SetDefault();
                        Config.Write();
                        plr.SendMessage("在线礼包已恢复默认", color);
                    }
                    break;

                default:
                    HelpCmd(plr);
                    break;
            }
        }
    }
    #endregion

    #region 添加礼包物品
    private static void AddGiftItem(CommandArgs args)
    {
        var plr = args.Player;

        if (args.Parameters.Count < 5)
        {
            plr.SendErrorMessage("用法：/gift add <物品名> <概率> <最小数量> <最大数量>");
            plr.SendMessage($"例如:/gift add 金币 1 1 1", color);
            return;
        }

        // 获取物品
        var itemName = args.Parameters[1];
        var items = TShock.Utils.GetItemByIdOrName(itemName);

        if (items.Count == 0)
        {
            plr.SendErrorMessage($"未找到物品: {itemName}");
            return;
        }

        if (items.Count > 1)
        {
            args.Player.SendMultipleMatchError(items.Select(i => $"{i.Name}(ID:{i.netID})"));
            return;
        }

        var item = items[0];

        // 解析参数
        if (!int.TryParse(args.Parameters[2], out int rate) || rate <= 0)
        {
            plr.SendErrorMessage("概率必须为正整数");
            return;
        }

        if (!int.TryParse(args.Parameters[3], out int minStack) || minStack <= 0)
        {
            plr.SendErrorMessage("最小数量必须为正整数");
            return;
        }

        if (!int.TryParse(args.Parameters[4], out int maxStack) || maxStack <= 0 || maxStack < minStack)
        {
            plr.SendErrorMessage("最大数量必须为正整数且不小于最小数量");
            return;
        }

        // 检查是否已存在
        if (Config!.GiftList.Any(g => g.ItemType == item.netID))
        {
            plr.SendErrorMessage($"物品 [i/s1:{item.netID}] 已存在于礼包列表中");
            return;
        }

        // 添加物品
        var giftData = new GiftData(item.Name, item.netID, rate, new int[] { minStack, maxStack });
        Config.GiftList.Add(giftData);
        Config.UpdateTotalRate();

        plr.SendMessage($"已添加物品: {Config.GiftList.Count}.[i/s1:{item.netID}] 概率:{rate} 数量:{minStack}-{maxStack}",color);
    }
    #endregion

    #region 删除礼包物品
    private static void RemoveGiftItem(CommandArgs args)
    {
        var plr = args.Player;

        if (args.Parameters.Count < 2)
        {
            plr.SendErrorMessage("用法：/gift del <物品名|索引>");
            return;
        }

        var target = args.Parameters[1];

        // 按索引删除
        if (int.TryParse(target, out int index) && index > 0 && index <= Config!.GiftList.Count)
        {
            var removedItem = Config.GiftList[index - 1];
            Config.GiftList.RemoveAt(index - 1);
            Config.UpdateTotalRate();
            plr.SendSuccessMessage($"已删除第{index}个物品: [i/s1:{removedItem.ItemType}]");
            return;
        }

        // 按物品名删除
        var itemsToRemove = Config.GiftList
            .Where(g => g.ItemName.Contains(target, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (itemsToRemove.Count == 0)
        {
            plr.SendErrorMessage($"未找到包含 '{target}' 的物品");
            return;
        }

        if (itemsToRemove.Count > 1)
        {
            plr.SendErrorMessage($"找到多个匹配物品，请使用索引删除:");
            for (int i = 0; i < itemsToRemove.Count; i++)
            {
                var item = itemsToRemove[i];
                var originalIndex = Config.GiftList.IndexOf(item) + 1;
                plr.SendInfoMessage($"{originalIndex}. [i/s1:{item.ItemType}] - {item.ItemName}");
            }
            return;
        }

        // 删除单个匹配物品
        var itemToRemove = itemsToRemove[0];
        Config.GiftList.Remove(itemToRemove);
        Config.UpdateTotalRate();
        plr.SendSuccessMessage($"已删除物品: [i/s1:{itemToRemove.ItemType}] - {itemToRemove.ItemName}");
    }
    #endregion

    #region 菜单方法
    private static void HelpCmd(TSPlayer plr)
    {
        var mess = new StringBuilder(); //用于存储指令内容

        // 先构建消息内容
        if (plr.RealPlayer)
        {
            plr.SendMessage("[i:3455][c/AD89D5:在][c/D68ACA:线][c/DF909A:礼][c/E5A894:包][i:3454] " +
            "[i:3456][C/F2F2C7:开发] [C/BFDFEA:by] [c/00FFFF:羽学] | 星夜神花 [i:3459]", color);

            if (plr.HasPermission(Config!.IsAdamin))
            {
                mess.Append($"/gift on与off ——启用关闭在线礼包\n" +
                            $"/gift l ——显示当前礼包物品\n" +
                            $"/gift s ——显示距离下次礼包剩余时间\n" +
                            $"/gift t ——设置礼包发放时间\n" +
                            $"/gift add 物品名 [概率] [数量] ——添加物品\n" +
                            $"/gift del 物品名|索引 ——移除物品\n" +
                            $"/gift rs ——还原默认配置\n");
            }
            else
            {
                mess.Append($"/gift s ——显示距离下次礼包剩余时间\n" +
                            $"/gift l ——显示当前礼包物品\n");

            }

            // 现在对消息内容应用渐变色
            var Text = mess.ToString();
            var lines = Text.Split('\n');
            var GradMess = new StringBuilder();
            var start = new Color(166, 213, 234);
            var end = new Color(245, 247, 175);
            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrEmpty(lines[i]))
                {
                    float ratio = (float)i / (lines.Length - 1);
                    var gradColor = Color.Lerp(start, end, ratio);

                    // 将颜色转换为十六进制格式
                    string colorHex = $"{gradColor.R:X2}{gradColor.G:X2}{gradColor.B:X2}";

                    // 使用颜色标签包装每一行
                    GradMess.AppendLine($"[c/{colorHex}:{lines[i]}]");
                }
            }

            plr.SendMessage(GradMess.ToString(), color);
        }
        else
        {
            plr.SendMessage("《在线礼包》\n" +
                            $"/gift on与off ——启用关闭在线礼包\n" +
                            $"/gift l ——显示当前礼包物品\n" +
                            $"/gift t ——设置礼包发放时间\n" +
                            $"/gift add 物品名 [概率] [数量] ——添加物品\n" +
                            $"/gift del 物品名|索引 ——移除物品\n" +
                            $"/gift rs ——还原默认配置\n", color);
        }
    }
    #endregion
}
