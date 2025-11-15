using Microsoft.Xna.Framework;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace OnlineGift;

[ApiVersion(2, 1)]
public class OnlineGift : TerrariaPlugin
{
    #region 插件信息
    public override string Name => "在线礼包";
    public override string Author => "星夜神花 羽学重构";
    public override string Description => "在线礼包插件 ";
    public override Version Version => new Version(1, 1, 3);
    #endregion

    #region 注册与卸载方法
    public OnlineGift(Main game) : base(game) { }
    public override void Initialize()
    {
        LoadConfig();
        StatsManager.LoadAllStats(); // 加载所有玩家统计数据
        GeneralHooks.ReloadEvent += ReloadConfig;
        GetDataHandlers.PlayerUpdate += OnPlayerUpdate!;
        ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreetPlayer);
        ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
        TShockAPI.Commands.ChatCommands.Add(new Command(new List<string> { "在线礼包", "gift" }, Commands.GiftCMD, "在线礼包", "gift"));
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            StatsManager.SaveAllStats(); // 保存所有玩家统计数据
            GeneralHooks.ReloadEvent -= ReloadConfig;
            GetDataHandlers.PlayerUpdate -= OnPlayerUpdate!;
            ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreetPlayer);
            ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
            TShockAPI.Commands.ChatCommands.RemoveAll(x => x.CommandDelegate == Commands.GiftCMD);
        }
        base.Dispose(disposing);
    }
    #endregion

    #region 配置重载读取与写入方法
    public static Configuration? Config = new();
    private static void ReloadConfig(ReloadEventArgs args = null!)
    {
        LoadConfig();
        Config?.UpdateTotalRate();
        args.Player.SendInfoMessage("[在线礼包]重新加载配置完毕。");
        string nextTime = FormatTime(Config!.SendTimer);
        TShock.Utils.Broadcast($"当前发放礼包时间为:{nextTime}", color);
        TShock.Utils.Broadcast($"{Config.GiftList.Count}个礼包的总概率为:{Config.TotalRate()}", color);
    }

    private static void LoadConfig()
    {
        Config = Configuration.Read();
        WriteName();
        Config.Write();
    }
    #endregion

    #region 获取物品中文名
    public static void WriteName()
    {
        foreach (var item in Config!.GiftList)
        {
            if (string.IsNullOrEmpty(item.ItemName))
            {
                item.ItemName = Lang.GetItemNameValue(item.ItemType);
            }
        }
    }
    #endregion

    #region 初始化玩家数据方法
    private void OnGreetPlayer(GreetPlayerEventArgs args)
    {
        var plr = TShock.Players[args.Who];
        if (plr is null || !plr.Active || !plr.IsLoggedIn) return;

        // 初始化玩家统计
        StatsManager.UpdateStats(plr);
    }

    private void OnServerLeave(LeaveEventArgs args)
    {
        var plr = TShock.Players[args.Who];
        if (plr != null)
        {
            // 保存玩家统计
            StatsManager.SaveStats(plr);
        }
    }
    #endregion

    #region 玩家更新事件
    public static Color color = new Color(240, 250, 150);
    private void OnPlayerUpdate(object sender, GetDataHandlers.PlayerUpdateEventArgs args)
    {
        if (Config is null || !Config.Enabled) return;

        var plr = args.Player;
        if (plr is null || !plr.Active || !plr.IsLoggedIn) return;

        if (Config.SkipStatLifeMax > 0 &&
            !plr.HasPermission(Config.IsAdamin) &&
            plr.TPlayer.statLifeMax >= Config.SkipStatLifeMax)
        {
            return;
        }

        var stats = StatsManager.GetStats(plr);
        var elapsed = (DateTime.Now - stats.LastGiftSendTime).TotalSeconds;

        if (elapsed >= Config.SendTimer)
        {
            GiftData gift = RandGift(plr);
            if (gift is null) return;

            int stack = Main.rand.Next(gift.Stack[0], gift.Stack[1]);
            plr.GiveItem(gift.ItemType, stack);
            string item = string.Format(" [i/s{0}:{1}] ", stack, gift.ItemType);

            // 更新玩家统计（获得礼包）
            StatsManager.UpdateStats(plr, 1);

            // 计算剩余时间并格式化显示
            string nextTime = FormatTime(Config.SendTimer);
            plr.SendMessage($"{Config.Text} {item} 下次发放将在[c/F38152:{nextTime}]后", color);
        }
    }
    #endregion

    #region 格式化时间显示
    public static string FormatTime(int seconds)
    {
        if (seconds < 60)
            return $"{seconds}秒";
        else if (seconds < 3600)
            return $"{seconds / 60}分钟";
        else if (seconds < 86400)
            return $"{seconds / 3600}小时";
        else
            return $"{seconds / 86400}天";
    }
    #endregion

    #region 随机选取礼包的方法（添加条件检查）
    public static GiftData RandGift(TSPlayer player)
    {
        if (Config is null || !Config.Enabled) return null;

        // 首先筛选出满足条件的礼包
        var AvailGifts = new List<GiftData>();
        var AvailRates = new List<int>();
        int TotalRate = 0;

        for (int i = 0; i < Config.GiftList.Count; i++)
        {
            var gift = Config.GiftList[i];

            // 检查条件
            if (gift.Conditions == null || gift.Conditions.Count == 0 || Condition.CheckGroup(player.TPlayer, gift.Conditions))
            {
                AvailGifts.Add(gift);
                AvailRates.Add(gift.Rate);
                TotalRate += gift.Rate;
            }
        }

        // 如果没有可用的礼包，返回null
        if (AvailGifts.Count == 0 || TotalRate == 0)
        {
            return null;
        }

        // 在可用的礼包中随机选择
        int index = Main.rand.Next(TotalRate);
        int sum = 0;

        for (int i = 0; i < AvailGifts.Count; i++)
        {
            sum += AvailRates[i];
            if (index < sum)
            {
                return AvailGifts[i];
            }
        }
        return null;
    }
    #endregion

}