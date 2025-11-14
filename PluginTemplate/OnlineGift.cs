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
    public override Version Version => new Version(1, 1, 2);
    #endregion

    #region 注册与卸载方法
    public OnlineGift(Main game) : base(game){}
    public override void Initialize()
    {
        LoadConfig();
        GeneralHooks.ReloadEvent += ReloadConfig;
        ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
        ServerApi.Hooks.NetGreetPlayer.Register(this, this.OnGreetPlayer);
        ServerApi.Hooks.ServerLeave.Register(this, this.OnServerLeave);
        TShockAPI.Commands.ChatCommands.Add(new Command(new List<string> { "在线礼包", "gift" }, Commands.GiftCMD, "在线礼包", "gift"));
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            GeneralHooks.ReloadEvent -= ReloadConfig;
            ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
            ServerApi.Hooks.NetGreetPlayer.Deregister(this, this.OnGreetPlayer);
            ServerApi.Hooks.ServerLeave.Deregister(this, this.OnServerLeave);
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
        string nextTime = FormatTime(Config!.SendTimer * 60);
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
    public static readonly Dictionary<int, int> players = new Dictionary<int, int>();
    private void OnGreetPlayer(GreetPlayerEventArgs args)
    {
        var plr = TShock.Players[args.Who];
        if (plr is null || !plr.Active || !plr.IsLoggedIn) return;

        if (!players.ContainsKey(plr.Index))
        {
            players[plr.Index] = 0;
        }
    }
    private void OnServerLeave(LeaveEventArgs args)
    {
        var plr = TShock.Players[args.Who];

        if (players.ContainsKey(plr.Index))
        {
            players.Remove(plr.Index);
        }
    }
    #endregion

    #region 游戏更新事件
    public static Color color = new Color(240, 250, 150);
    private void OnGameUpdate(EventArgs args)
    {
        if (Config is null || !Config.Enabled) return;

        var Players = new int[players.Count];
        players.Keys.CopyTo(Players, 0);

        foreach (var pIndex in Players)
        {
            var plr = TShock.Players[pIndex];
            if (plr is null || !plr.Active || !plr.IsLoggedIn ||
                plr.TPlayer.statLifeMax < Config.SkipStatLifeMax) continue;

            players[pIndex]++;
            if (players[pIndex] < Config.SendTimer * 60) continue;

            GiftData gift = RandGift();
            if (gift is null) continue;

            int stack = Main.rand.Next(gift.Stack[0], gift.Stack[1]);
            plr.GiveItem(gift.ItemType, stack);
            string item = string.Format(" [i/s{0}:{1}] ", stack, gift.ItemType);

            // 重置玩家计时器
            players[pIndex] = 0;

            // 计算剩余时间（帧数）并格式化显示
            string nextTime = FormatTime(Config.SendTimer * 60);
            plr.SendMessage($"{Config.Text} {item} 下次发放将在[c/F38152:{nextTime}]后", color);
        }
    }
    #endregion

    #region 格式化时间显示
    public static string FormatTime(int frames)
    {
        // 将帧数转换为秒数（60帧=1秒）
        int s = frames / 60;

        if (s < 60)
            return $"{s}秒";
        else if (s < 3600)
            return $"{s / 60}分钟";
        else if (s < 86400)
            return $"{s / 3600}小时";
        else
            return $"{s / 86400}天";
    }
    #endregion

    #region 随机选取礼包的方法
    public GiftData RandGift()
    {
        if (Config is null || !Config.Enabled) return null;

        int index = Main.rand.Next(Config.Total);
        int sum = 0;

        for (int i = 0; i < Config.GiftList.Count; i++)
        {
            sum += Config.GiftList[i].Rate;
            if (index < sum)
            {
                return Config.GiftList[i];
            }
        }
        return null;
    } 
    #endregion
}