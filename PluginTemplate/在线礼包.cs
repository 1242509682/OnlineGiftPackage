using Microsoft.Xna.Framework;
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
    public class 在线礼包 : TerrariaPlugin
    {
        // 插件作者信息
        public override string Author => "星夜神花 羽学适配";

        // 插件描述
        public override string Description => "在线礼包插件 ";

        // 插件名称
        public override string Name => "在线礼包";

        // 插件版本号
        public override Version Version => new Version(1, 0, 0, 8);

        // 构造函数，初始化插件与游戏关联
        public 在线礼包(Main game) : base(game)
        {
        }

        // 定时器实例，用于定期检查在线玩家并发放礼包
        Timer timer;

        // 配置文件加载实例
        Config config;

        // 插件初始化方法
        public override void Initialize()
        {
            // 加载配置文件
            config = Config.LoadConfig;

            // 注册命令，用于显示礼包获取概率
            Commands.ChatCommands.Add(new Command(GetProbability, "在线礼包"));

            // 监听服务器重载事件，以便在重载后重新设置定时器
            TShockAPI.Hooks.GeneralHooks.ReloadEvent += GeneralHooks_ReloadEvent;

            // 初始化定时器，根据配置的触发时间执行Timer_Elapsed方法
            timer = new Timer(Timer_Elapsed, null, Timeout.Infinite, (int)config.触发时间.TotalMilliseconds);
            timer.Change(0, (int)config.触发时间.TotalMilliseconds);
        }

        // 重载事件处理程序
        private void GeneralHooks_ReloadEvent(ReloadEventArgs e)
        {
            // 重载配置文件
            config = Config.LoadConfig;

            // 更新定时器触发间隔
            timer.Change(config.触发时间, config.触发时间);
        }

        // 玩家在线时长字典，用于记录玩家在线时长（按天）
        readonly Dictionary<string, int> players = new Dictionary<string, int>();

        // 当前系统日期的天数
        public static int Day = DateTime.Now.Day;

        // 定时器回调方法，负责发放在线礼包
        private void Timer_Elapsed(object state)
        {
            if (config.启用 == false)
            {
                return;
            }

            // 如果日期改变，则清除玩家在线时长记录并更新当前日期
            if (DateTime.Now.Day != Day)
            {
                Day = DateTime.Now.Day;
                players.Clear();
            }

            // 遍历所有在线且已登录的玩家
            for (int i = 0; i < TShock.Players.Length; i++)
            {
                var player = TShock.Players[i];

                if (player == null || !player.Active || !player.IsLoggedIn)
                {
                    continue;
                }

                // 跳过生命值大于等于2000的玩家
                if (player.TPlayer.statLifeMax >= 2000)
                {
                    continue;
                }

                // 记录或增加玩家在线时长
                if (!players.ContainsKey(player.Name))
                {
                    players[player.Name] = 0;
                }
                else
                {
                    players[player.Name] += 1;
                }

                // 根据玩家在线时长发放对应礼包
                if (config.触发序列.ContainsKey(players[player.Name]))
                {
                    Gift gift = RandGift();
                    if (gift == null)
                    {
                        return;
                    }

                    // 获取随机物品数量
                    int itemCount = new Random().Next(minValue: gift.物品数量[0], gift.物品数量[1]);

                    // 给玩家发放物品
                    player.GiveItem(gift.物品ID, itemCount);

                    // 发送提示消息给玩家
                    player.SendMessage(string.Format(config.触发序列[players[player.Name]] + " [c/55CDFF:服主]送了个在线礼包 [i/s{2}:{1}]", players[player.Name], gift.物品ID, itemCount), Color.GreenYellow);
                }
                if (players[player.Name] % config.广播间隔时间 == 0)
                {
                    player.SendMessage(string.Format(config.广播消息, players[player.Name]), Color.GreenYellow);
                }
            }
        }

        // 显示礼包获取概率的命令处理程序
        private void GetProbability(CommandArgs args)
        {
            // 异步任务显示礼包概率
            Task.Run(() =>
            {
                // 分批显示礼包概率
                for (int i = 0; i < config.礼包列表.Count; i += 10)
                {
                    StringBuilder sb = new StringBuilder();

                    // 组装每批礼包的概率信息
                    for (int j = 0; j < 10 && i + j < config.礼包列表.Count; j++)
                    {
                        Gift gift = config.礼包列表[i + j];
                        double probability = (double)gift.所占概率 / config.总概率;
                        sb.Append($"[i/s1:{gift.物品ID}]:{probability:0.##}% ");
                    }

                    // 发送给玩家
                    args.Player.SendMessage(sb.ToString(), Color.Cornsilk);
                }
            });
        }

        // 随机选取礼包的方法
        readonly Random rand = new Random();
        public Gift? RandGift()
        {
            // 随机生成一个索引值
            int index = rand.Next(config.总概率);
            int sum = 0;

            // 根据概率分布随机选择一个礼包
            for (int i = 1; i < config.礼包列表.Count; i++)
            {
                sum += config.礼包列表[i].所占概率;
                if (index < sum)
                {
                    return config.礼包列表[i];
                }
            }

            // 若未找到符合条件的礼包，则返回null
            return null;
        }
    }
}