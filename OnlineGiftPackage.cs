﻿using Microsoft.Xna.Framework;
using System.Collections.Concurrent;
using System.Text;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

// 插件命名空间
namespace 在线礼包
{
    // 插件使用TShockAPI v2.1
    [ApiVersion(2, 1)]
    public class OnlineGiftPackage : TerrariaPlugin
    {
        // 插件作者信息
        public override string Author => "星夜神花 羽学适配";
        // 插件描述
        public override string Description => "在线礼包插件 ";
        // 插件名称
        public override string Name => "在线礼包";
        // 插件版本号
        public override Version Version => new Version(1, 0, 1, 1);
        // 构造函数，初始化插件与游戏关联
        public OnlineGiftPackage(Main game) : base(game)
        {
        }

        // 当前系统日期的天数
        public static int Day = DateTime.Now.Day;

        // 定时器实例，用于定期检查在线玩家并发放礼包
        Timer timer;

        // 使用线程安全的字典存储玩家在线时长
        private ConcurrentDictionary<string, int> players = new ConcurrentDictionary<string, int>();

        // 配置文件加载实例
        private static Configuration? config; // 将config声明为静态字段

        private object syncRoot = new object(); // 用于锁定发放礼包的临界区

        // 插件初始化方法
        public override void Initialize()
        {
            // 调用LoadConfig方法获取配置实例并赋值给config
            LoadConfig();
            // 注册命令，用于显示礼包获取概率
            Commands.ChatCommands.Add(new Command(GetProbability, "在线礼包"));
            // 监听服务器重载事件，以便在重载后重新设置定时器
            TShockAPI.Hooks.GeneralHooks.ReloadEvent += ReloadEvent;
            timer = new Timer(Timer_Elapsed, "", config.发放间隔 * 1000, config.发放间隔 * 1000); // 注意转换为毫秒
        }

        //加载并创建配置文件
        private void LoadConfig()
        {
            //如果配置文件存在，只读不覆盖
            if (File.Exists(Configuration.FilePath))
            {
                config = Configuration.Read(Configuration.FilePath);
                config.CalculateTotalProbability();
            }
            // 如果配置文件不存在或加载失败，则创建并保存默认配置文件
            else
            {
                config = Configuration.CreateDefaultConfig();
                config.Write(Configuration.FilePath);
            }
        }

        // 重载事件处理程序
        private void ReloadEvent(ReloadEventArgs e)
        {
            LoadConfig();

            // 调用UpdateTotalProbabilityOnReload方法来更新总概率
            config.UpdateTotalProbabilityOnReload();

            // 更新定时器触发间隔
            timer.Change(config.发放间隔 * 1000, config.发放间隔 * 1000); // 注意转换为毫秒
            Console.WriteLine($"已重载 [在线礼包] 配置文件,下次发放将在{config.发放间隔}秒后");
            int totalProbability = config.CalculateTotalProbability();
            Console.WriteLine($"所有礼包的总概率为：{totalProbability}");
        }

        private void Timer_Elapsed(object? state)
        {
            lock (syncRoot)
            {
                // 获取当前日期，并清理在线时长记录（每天只在第一次执行时清空）
                if (DateTime.Now.Day != Day)
                {
                    Day = DateTime.Now.Day;
                    players.Clear();
                }

                foreach (var player in TShock.Players.Where(p => p != null && p.Active && p.IsLoggedIn && p.TPlayer.statLifeMax < config.SkipStatLifeMax))
                {
                    if (!config.启用)
                    {
                        return;
                    }

                    // 记录或增加玩家在线时长
                    players.AddOrUpdate(player.Name, 1, (_, currentCount) => currentCount + 1);

                    // 跳过生命值大于多少的玩家
                    if (player.TPlayer.statLifeMax >= config.SkipStatLifeMax)
                    {
                        continue;
                    }

                    // 根据玩家在线时长发放对应礼包
                    if (players[player.Name] >= config.触发序列.Keys.Min())
                    {
                        Gift gift = RandGift();
                        if (gift == null)
                        {
                            Console.WriteLine($"无法获取有效礼包，玩家 {player.Name} 的在线时长：{players[player.Name]} 秒");
                            continue;
                        }

                        // 获取随机物品数量
                        int itemCount = new Random().Next(minValue: gift.物品数量[0], gift.物品数量[1]);

                        // 给玩家发放物品
                        player.GiveItem(gift.物品ID, itemCount);

                        // 构建礼包发放提示消息
                        string playerMessageFormat = config.触发序列[players[player.Name]];
                        string packageInfoMessage = string.Format(playerMessageFormat + " [i/s{0}:{1}] ", players[player.Name], gift.物品ID, itemCount);

                        // 添加发放间隔信息
                        int intervalForDisplay = config.发放间隔;
                        string intervalMessage = $"下次发放将在{intervalForDisplay}秒后";

                        // 合并两条消息
                        string combinedMessage = $"{packageInfoMessage} {intervalMessage}";

                        // 将合并后的消息发送给玩家
                        player.SendMessage(combinedMessage, Color.GreenYellow);

                        // 控制台输出
                        if (config.每次发放礼包记录后台)
                        {
                            Console.WriteLine($"执行在线礼包发放任务，下次发放将在{config.发放间隔}秒后");
                            int totalProbability = config.CalculateTotalProbability();
                            Console.WriteLine($"所有礼包的总概率为：{totalProbability}");
                        }

                        // 发放成功后重置玩家在线时长
                        players[player.Name] %= config.发放间隔;
                    }
                }
            }
        }

        // 显示礼包获取概率的命令处理程序
        private void GetProbability(CommandArgs args)
        {
            if (args.Player.HasPermission("在线礼包"))
            {
                Task.Run(() =>
                {
                    StringBuilder sb = new StringBuilder();

                    // 添加标题行
                    sb.AppendLine("在线礼包概率表：\n");

                    // 显示所有礼包的获取概率，按每5个一组分批显示
                    for (int i = 0; i < config.礼包列表.Count; i++)
                    {
                        Gift gift = config.礼包列表[i];
                        sb.Append("[i/s1:{0}]:{1:0.##}% ".SFormat(gift.物品ID, 100.0 * ((double)gift.所占概率 / config.总概率)));

                        // 每显示5个礼包后换行
                        if ((i + 1) % 5 == 0)
                        {
                            sb.AppendLine();
                        }
                    }

                    // 计算并添加总概率信息
                    int totalProbability = config.CalculateTotalProbability();
                    sb.AppendLine($"\n所有礼包的总概率为：{totalProbability}%");

                    // 发送给玩家
                    args.Player.SendMessage(sb.ToString(), Color.Cornsilk);
                });
            }
            else
            {
                args.Player.SendMessage("你没有足够的权限来查看礼包获取概率。", Color.Red); // 或者使用您的错误提示方式
            }
        }

        // 随机选取礼包的方法
        Random rand = new Random();
        public Gift? RandGift()
        {
            int index = rand.Next(config.总概率);
            int sum = 0;

            // 从索引0开始遍历，修正for循环起点
            for (int i = 0; i < config.礼包列表.Count; i++)
            {
                sum += config.礼包列表[i].所占概率;
                if (index < sum)
                {
                    return config.礼包列表[i];
                }
            }
            return null;
        }
    }
}