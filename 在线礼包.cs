using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace 在线礼包
{
    [ApiVersion(2, 1)]
    public class 在线礼包 : TerrariaPlugin
    {
        public override string Author => "羽学适配 作者：星夜神花 QQ群1103642210 ";
        public override string Description => "在线礼包插件 ";
        public override string Name => "在线礼包";
        public override Version Version => new Version(1, 0, 0, 7);
        public 在线礼包(Main game) : base(game)
        {
        }

        Timer timer;
        Config config;
        public override void Initialize()
        {
            config = Config.LoadConfig;
            Commands.ChatCommands.Add(new Command(GetProbability, "在线礼包"));

            TShockAPI.Hooks.GeneralHooks.ReloadEvent += GeneralHooks_ReloadEvent;
            timer = new Timer(Timer_Elapsed, null, config.触发时间, config.触发时间);
        }

        private void GeneralHooks_ReloadEvent(ReloadEventArgs e)
        {
            config = Config.LoadConfig;
            timer.Change(config.触发时间, config.触发时间);
        }

        readonly Dictionary<string, int> players = new Dictionary<string, int>();
        public static int Day = DateTime.Now.Day;
        private void Timer_Elapsed(object state)
        {
            if (config.启用 == false)
            {
                return;
            }
            if (DateTime.Now.Day != Day)
            {
                Day = DateTime.Now.Day;
                players.Clear();
            }
            for (int i = 0; i < TShock.Players.Length; i++)
            {
                TSPlayer player = TShock.Players[i];
                if (player == null)
                {
                    continue;
                }
                if (player.Active && player.IsLoggedIn)
                {
                    if (player.TPlayer.statLifeMax >= 2000)
                    {
                        continue;
                    }
                    if (!players.ContainsKey(player.Name))
                    {
                        players[player.Name] = 0;
                    }
                    else
                    {
                        players[player.Name] += 1;
                    }
                    if (config.触发序列.ContainsKey(players[player.Name]))
                    {
                        Gift gift = RandGift();
                        if (gift == null)
                        {
                            return;
                        }
                        int 物品数量 = new Random().Next(gift.物品数量[0], gift.物品数量[1]);
                        player.GiveItem(gift.物品ID, 物品数量);
                        player.SendMessage(string.Format(config.触发序列[players[player.Name]] + " [c/55CDFF:服主]送了个在线礼包 [i/s{2}:{1}]", players[player.Name], gift.物品ID, 物品数量), Color.GreenYellow);
                    }
                    else if (players[player.Name] % 10 == 0)
                    {
                        player.SendMessage(string.Format(config.广播消息, players[player.Name]), Color.GreenYellow);
                    }
                }
            }
        }


        private void GetProbability(CommandArgs args)
        {
            Task.Run(() => {
                for (int i = 0; i < config.礼包列表.Count; i += 10)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int j = 0; j < 10 && i + j < config.礼包列表.Count; j++)
                    {
                        Gift gift = config.礼包列表[i + j];
                        double probability = (double)gift.所占概率 / config.denominator;
                        sb.Append("[i/s1:{0}]:{1:0.##}% ".SFormat(gift.物品ID, 100.0 * probability));
                    }
                    args.Player.SendMessage(sb.ToString(), Color.Cornsilk);
                }
            });
        }

        readonly Random rand = new Random();
        public Gift RandGift()
        {
            int index = rand.Next(config.denominator);
            int sum = 0;
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