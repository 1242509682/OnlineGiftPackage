using System.Text;
using TShockAPI;
using Microsoft.Xna.Framework;
using static OnlineGift.OnlineGift;
using static OnlineGift.Condition;

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
                            var oldTimer = FormatTime(Config!.SendTimer);
                            Config.SendTimer = num;
                            Config.Write();
                            string nextTime = FormatTime(Config!.SendTimer);
                            plr.SendMessage($"在线礼包发放时间: [c/47D3C3:{oldTimer}] → [c/F38152:{nextTime}] ", color);
                        }
                        else
                        {
                            plr.SendMessage("用法：/gift t 秒数", color);
                        }
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
                            string conditionText = gift.Conditions.Count > 0 ? $"[c/FF6B6B:条件:{string.Join(",", gift.Conditions)}]" : "";

                            mess.Append($"[c/47D3C3:{i + 1:00}.][i/s1:{gift.ItemType}]{rate:0}% {conditionText}");

                            // 每显示3个礼包后换行（因为现在信息更多）
                            if ((i + 1) % 3 == 0)
                            {
                                mess.AppendLine();
                            }
                            else
                            {
                                mess.Append("   ");
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

                case "st":
                case "stats":
                case "统计":
                    ShowPlayerStats(plr);
                    break;

                case "top":
                case "排行":
                case "排行榜":
                    ShowLeaderboard(plr, args.Parameters);
                    break;

                case "cond":
                case "进度":
                case "条件":
                    ShowConditions(plr, args.Parameters);
                    break;

                case "rs":
                case "reset":
                case "重置":
                    {
                        if (!plr.HasPermission(Config!.IsAdamin)) return;
                        ResetGiftSystem(plr);
                    }
                    break;

                default:
                    HelpCmd(plr);
                    break;
            }
        }
    }
    #endregion

    #region 添加礼包物品（支持条件）
    private static void AddGiftItem(CommandArgs args)
    {
        var plr = args.Player;

        if (args.Parameters.Count < 5)
        {
            plr.SendErrorMessage("用法：/gift add <物品名> <概率> <最小数量> <最大数量> [条件1,条件2...]");
            plr.SendMessage($"例如:/gift add 金币 1 1 1", color);
            plr.SendMessage($"例如:/gift add 神圣锭 5 1 3 困难模式,世纪之花", color);
            plr.SendMessage($"例如:/gift add 血泪 2 1 1 血月", color);
            plr.SendMessage($"使用 /gift 进度 查看所有可用条件", color);
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

        // 解析条件（可选参数）
        List<string> conditions = new List<string>();
        if (args.Parameters.Count > 5)
        {
            // 支持多种分隔符：空格、逗号、竖线
            var conditionStr = string.Join(" ", args.Parameters.Skip(5));
            conditions = conditionStr.Split(new[] { ' ', ',', '|' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(c => c.Trim())
                                   .Where(c => !string.IsNullOrEmpty(c))
                                   .ToList();
        }

        // 检查是否已存在
        if (Config!.GiftList.Any(g => g.ItemType == item.netID))
        {
            plr.SendErrorMessage($"物品 [i/s1:{item.netID}] 已存在于礼包列表中");
            return;
        }

        // 添加物品
        var giftData = new GiftData(item.Name, item.netID, rate, new int[] { minStack, maxStack }, conditions);
        Config.GiftList.Add(giftData);
        Config.UpdateTotalRate();

        string conditionText = conditions.Count > 0 ? $" 条件:{string.Join(",", conditions)}" : "";
        plr.SendMessage($"已添加物品: {Config.GiftList.Count}.[i/s1:{item.netID}] 概率:{rate} 数量:{minStack}-{maxStack}{conditionText}", color);
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

    #region 显示玩家统计
    private static void ShowPlayerStats(TSPlayer plr)
    {
        if (!Config!.EnableStats)
        {
            plr.SendErrorMessage("统计功能未启用");
            return;
        }

        if (plr == TSPlayer.Server)
        {
            plr.SendInfoMessage("请进入游戏后查看统计");
            return;
        }

        var stats = StatsManager.GetStats(plr);
        var mess = new StringBuilder();

        mess.AppendLine($"[c/47D3C3:═══ {plr.Name}的礼包统计 ═══]");
        mess.AppendLine($"累计在线时间: [c/F38152:{FormatTime((int)stats.TotalOnlineSeconds)}]");
        mess.AppendLine($"累计获得礼包: [c/F38152:{stats.TotalGiftsReceived}个]");
        mess.AppendLine($"今日获得礼包: [c/F38152:{stats.TodayGiftsReceived}个]");
        mess.AppendLine($"连续领取天数: [c/F38152:{stats.ConsecutiveDays}天]");
        mess.AppendLine($"最后领取时间: [c/F38152:{(stats.LastGiftTime == DateTime.MinValue ? "从未领取" : stats.LastGiftTime.ToString("yyyy-MM-dd HH:mm"))}]");

        plr.SendMessage(mess.ToString(), color);
    }
    #endregion

    #region 显示排行榜
    private static void ShowLeaderboard(TSPlayer plr, List<string> parameters)
    {
        if (!Config!.EnableLeaderboard)
        {
            plr.SendErrorMessage("排行榜功能未启用");
            return;
        }

        string type = "gifts"; // 默认按礼包数量排行
        if (parameters.Count > 1)
        {
            switch (parameters[1].ToLower())
            {
                case "t":
                case "time":
                case "时间":
                    type = "time";
                    break;
                case "d":
                case "days":
                case "天数":
                    type = "days";
                    break;
                default:
                    type = "gifts";
                    break;
            }
        }

        var leaderboard = StatsManager.GetLeaderboard(type, Config.LeaderboardSize);
        var mess = new StringBuilder();

        string title = type switch
        {
            "time" => "在线时间排行榜",
            "days" => "连续天数排行榜",
            _ => "礼包数量排行榜"
        };

        mess.AppendLine($"[c/47D3C3:═══ {title} ═══]");

        for (int i = 0; i < leaderboard.Count; i++)
        {
            var playerStat = leaderboard[i];
            string rankColor = i switch
            {
                0 => "[c/FFD700:①]", // 金色
                1 => "[c/C0C0C0:②]", // 银色  
                2 => "[c/CD7F32:③]", // 铜色
                _ => $"[c/47D3C3:{i + 1:00}]"
            };

            string value = type switch
            {
                "time" => FormatTime((int)playerStat.TotalOnlineSeconds),
                "days" => $"{playerStat.ConsecutiveDays}天",
                _ => $"{playerStat.TotalGiftsReceived}个"
            };

            string playerName = string.IsNullOrEmpty(playerStat.PlayerName) ? "未知玩家" : playerStat.PlayerName;
            mess.AppendLine($"{rankColor} {playerName} - [c/F38152:{value}]");
        }

        if (leaderboard.Count == 0)
        {
            mess.AppendLine("[c/F38152:暂无排行榜数据]");
        }

        plr.SendMessage(mess.ToString(), color);
        plr.SendMessage("时间排行:/gift top t", color);
        plr.SendMessage("天数排行:/gift top d", color);
    }
    #endregion

    #region 获取剩余时间
    private static string GetRemainingTime(TSPlayer plr)
    {
        if (!plr.RealPlayer) return string.Empty;
        var stats = StatsManager.GetStats(plr);
        var elapsed = (DateTime.Now - stats.LastGiftSendTime).TotalSeconds;
        var remainingSeconds = Config!.SendTimer - elapsed;
        string remaining = FormatTime((int)Math.Max(0, remainingSeconds));

        return $"距离下次礼包发放还有: {remaining}";
    }
    #endregion

    #region 显示进度条件列表
    private static void ShowConditions(TSPlayer plr, List<string> parameters)
    {
        // 检查是否请求帮助
        if (parameters.Count >= 2 && parameters[1].ToLower() == "help")
        {
            ShowConditionHelp(plr);
            return;
        }

        // 解析页码
        int page = 1;
        if (parameters.Count >= 2)
        {
            if (!int.TryParse(parameters[1], out page) || page < 1)
            {
                plr.SendErrorMessage("页码必须是正整数！");
                return;
            }
        }

        // 显示条件列表
        int itemsPerPage = 10; // 每页显示10个条件组
        int totalGroups = ConditionGroups.Count;
        int totalPages = (int)Math.Ceiling(totalGroups / (double)itemsPerPage);

        if (page > totalPages)
        {
            plr.SendErrorMessage($"只有 {totalPages} 页可用！");
            return;
        }

        int startIndex = (page - 1) * itemsPerPage;
        int endIndex = Math.Min(startIndex + itemsPerPage, totalGroups);

        var groupsToShow = ConditionGroups.Skip(startIndex).Take(itemsPerPage);

        var mess = new StringBuilder();
        mess.AppendLine($"[c/47D3C3:═══ 进度条件列表 (第 {page}/{totalPages} 页) ═══]");
        mess.AppendLine($"[c/FFA500:共 {totalGroups} 个条件组，使用 /gift cond <页码> 翻页]");
        mess.AppendLine();

        int index = startIndex + 1;
        foreach (var group in groupsToShow)
        {
            string mainName = group.Key;
            var aliases = group.Value;

            // 构建显示字符串：主名称 + (别名1,别名2)
            string displayString = mainName;
            if (aliases.Count > 1)
            {
                // 排除主名称本身，只显示其他别名
                var otherNames = aliases.Where(a => a != mainName).ToList();
                if (otherNames.Count > 0)
                {
                    displayString += $" ({string.Join(", ", otherNames)})";
                }
            }

            mess.AppendLine($"[c/47D3C3:{index}.] [c/FFFFFF:{displayString}]");
            index++;
        }

        mess.AppendLine();

        // 显示翻页提示
        if (totalPages > 1)
        {
            string pageInfo = $"[c/FFFF00:第 {page}/{totalPages} 页]";
            if (page < totalPages)
            {
                pageInfo += $"[c/00FFFF: - 输入 /gift cond {page + 1} 查看下一页]";
            }
            if (page > 1)
            {
                pageInfo += $"[c/00FFFF: - 输入 /gift cond {page - 1} 查看上一页]";
            }
            mess.AppendLine(pageInfo);
        }

        mess.AppendLine($"[c/FFA500:提示: /gift add 物品名 概率 最小数量 最大数量 条件1,条件2...]");
        mess.AppendLine($"[c/FFA500:例如: /gift add 神圣锭 5 1 3 困难模式,世纪之花]");
        mess.AppendLine($"[c/FFA500:帮助: /gift cond help]");

        plr.SendMessage(mess.ToString(), color);
    }

    // 显示条件帮助信息
    private static void ShowConditionHelp(TSPlayer plr)
    {
        var mess = new StringBuilder();
        mess.AppendLine($"[c/47D3C3:═══ 进度条件使用说明 ═══]");
        mess.AppendLine($"  [c/FFFFFF:1.添加礼包时设置条件:]");
        mess.AppendLine($"  [c/FFFF00:/gift add 神圣锭 5 1 3 困难模式,世纪之花]");
        mess.AppendLine($"  [c/FFFF00:/gift add 血泪 2 1 1 血月]");
        mess.AppendLine($"  [c/FFFF00:/gift add 金币 1 1 1]");

        mess.AppendLine($"  [c/FFFFFF:2. 条件检查规则:]");
        mess.AppendLine($"  [c/FFFF00:只有满足所有条件的玩家才能获得该礼包]");
        mess.AppendLine($"  [c/FFFF00:多个条件用逗号或空格分隔]");
        mess.AppendLine($"  [c/FFFF00:括号内的名称是等效的别名，可以互换使用]");

        mess.AppendLine($"  [c/FFFFFF:3. 查看条件列表:]");
        mess.AppendLine($"  [c/FFFF00:/gift cond - 查看第一页]");
        mess.AppendLine($"  [c/FFFF00:/gift cond 2 - 查看第二页]");
        mess.AppendLine($"  [c/FFFF00:/gift cond help - 显示此帮助]");

        mess.AppendLine($"  [c/FFFFFF:4. 条件示例:]");
        mess.AppendLine($"  [c/FFFF00:困难模式 - 世界进入困难模式后]");
        mess.AppendLine($"  [c/FFFF00:世纪之花 - 击败世纪之花后]");
        mess.AppendLine($"  [c/FFFF00:血月 - 血月事件期间]");
        mess.AppendLine($"  [c/FFFF00:雨天 - 下雨天气时]");

        plr.SendMessage(mess.ToString(), color);
    }
    #endregion

    #region 重置礼包系统
    private static void ResetGiftSystem(TSPlayer plr)
    {
        var resultMessages = new List<string>();

        // 根据配置决定是否删除统计文件
        if (Config!.ResetDeleteStats)
        {
            StatsManager.DeleteStatsFiles();
            resultMessages.Add("已删除所有统计文件");
        }
        else
        {
            resultMessages.Add("保留统计文件");
        }

        // 根据配置决定是否还原默认配置
        if (Config.ResetToDefault)
        {
            Config.SetDefault();
            Config.Write();
            resultMessages.Add("已还原默认配置");
        }
        else
        {
            resultMessages.Add("保留当前配置");
        }

        // 发送结果消息
        string result = string.Join("，", resultMessages);
        plr.SendMessage($"在线礼包重置完成: {result}", color);

        // 记录日志
        TShock.Log.ConsoleInfo($"[在线礼包] 玩家 {plr.Name} 执行重置: {result}");
    }
    #endregion

    #region 菜单方法
    private static void HelpCmd(TSPlayer plr)
    {
        var mess = new StringBuilder();

        // 先构建消息内容
        if (plr.RealPlayer)
        {
            plr.SendMessage("[i:3455][c/AD89D5:在][c/D68ACA:线][c/DF909A:礼][c/E5A894:包][i:3454] " +
            "[i:3456][C/F2F2C7:开发] [C/BFDFEA:by] [c/00FFFF:羽学] | 星夜神花 [i:3459]", color);

            // 构建指令菜单
            if (plr.HasPermission(Config!.IsAdamin))
            {
                mess.Append($"/gift on与off ——启用关闭在线礼包\n" +
                            $"/gift l ——显示当前礼包物品\n" +
                            $"/gift t ——设置礼包发放时间\n" +
                            $"/gift add 物品名 概率 数量 ——添加物品\n" +
                            $"/gift del 物品名|索引 ——移除物品\n");
            }
            else
            {
                mess.Append($"/gift l ——显示当前礼包物品\n");
            }

            // 为真实玩家添加统计和排行榜指令说明
            if (Config.EnableStats)
            {
                mess.Append($"/gift st ——查看个人统计\n");
            }
            if (Config.EnableLeaderboard)
            {
                mess.Append($"/gift top ——查看排行榜\n");
            }

            // 新增进度条件指令说明
            mess.Append($"/gift cond ——进度条件参考\n");

            mess.Append($"/gift rs ——重置在线礼包\n");

            // 在菜单指令下面显示剩余时间
            string remainingTime = GetRemainingTime(plr);
            if (!string.IsNullOrEmpty(remainingTime))
            {
                mess.Append($"{remainingTime}");
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
            // 控制台显示 - 不包含个人统计指令
            plr.SendMessage("《在线礼包》\n" +
                            $"/gift on与off ——启用关闭在线礼包\n" +
                            $"/gift l ——显示当前礼包物品\n" +
                            $"/gift t ——设置礼包发放时间\n" +
                            $"/gift add 物品名 概率 数量 ——添加物品\n" +
                            $"/gift del 物品名|索引 ——移除物品\n" +
                            $"/gift cond ——进度条件参考\n" +
                            $"/gift top ——查看排行榜\n" +
                            $"/gift rs ——重置在线礼包\n", color);
        }
    }
    #endregion
}