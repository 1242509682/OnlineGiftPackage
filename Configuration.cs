using Newtonsoft.Json;
using TShockAPI;

namespace OnlineGift;

public class Configuration
{
    [JsonProperty("启用插件", Order = 0)]
    public bool Enabled { get; set; } = true;
    [JsonProperty("管理权限", Order = 1)]
    public string IsAdamin { get; set; } = "gift.admin";
    [JsonProperty("总概率(自动更新)", Order = 2)]
    public int Total { get; set; }
    [JsonProperty("发放秒数", Order = 3)]
    public int SendTimer { get; set; } = 1800;
    [JsonProperty("跳过生命上限", Order = 4)]
    public int SkipStatLifeMax { get; set; } = 500;
    [JsonProperty("发放时的信息", Order = 5)]
    public string Text { get; set; } = $"[c/55CDFF:服务器]送了你1个在线礼包";
    [JsonProperty("礼包列表", Order = 6)]
    public List<GiftData> GiftList { get; set; } = new List<GiftData>();

    #region 读取与创建配置文件方法
    public static readonly string FilePath = Path.Combine(TShock.SavePath, "在线礼包.json");
    public void Write()
    {
        Total = TotalRate();
        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(FilePath, json);
    }
    public static Configuration Read()
    {
        if (!File.Exists(FilePath))
        {
            var NewConfig = new Configuration();
            NewConfig.SetDefault();
            new Configuration().Write();
            return NewConfig;
        }
        else
        {
            var jsonContent = File.ReadAllText(FilePath);
            return JsonConvert.DeserializeObject<Configuration>(jsonContent)!;
        }
    }
    #endregion

    #region 计算总概率方法
    public int TotalRate()
    {
        if (GiftList != null)
        {
            return GiftList.Sum(gift => gift.Rate);
        }
        else
        {
            TShock.Log.ConsoleInfo("[在线礼包] 无法计算总概率，因为礼包列表为空。");
            return 0;
        }
    }

    public void UpdateTotalRate()
    {
        Total = Read().TotalRate();  // 设置总概率
        Write(); // 将更新后的总概率写回配置文件
    }
    #endregion

    #region 预设参数方法
    public void SetDefault()
    {
        Enabled = true;
        SendTimer = 1800;
        Text = $"[c/55CDFF:服务器]送了你1个在线礼包";
        GiftList = new List<GiftData>()
        {
            new GiftData("铂金币",74,1,new int[] { 2, 5 }),
            new GiftData("蠕虫罐头",4345,1,new int[] { 2, 5 }),
            new GiftData("草药袋",3093,1,new int[] { 2, 5 }),
            new GiftData("培根",3532,1,new int[] { 2, 5 }),
            new GiftData("木匣",2334,1,new int[] { 2, 5 }),
            new GiftData("铁匣",2335,1,new int[] { 2, 5 }),
            new GiftData("金匣",2336,1,new int[] { 2, 5 }),
            new GiftData("地牢匣",3205,1,new int[] { 2, 5 }),
            new GiftData("天空匣",3206,1,new int[] { 2, 5 }),
            new GiftData("冰冻匣",4405,1,new int[] { 2, 5 }),
            new GiftData("绿洲匣",4407,1,new int[] { 2, 5 }),
            new GiftData("黑曜石匣",4877,1,new int[] { 2, 5 }),
            new GiftData("海洋匣",5002,1,new int[] { 2, 5 }),
            new GiftData("暴怒药水",2347,1,new int[] { 2, 5 }),
            new GiftData("怒气药水",2349,1,new int[] { 2, 5 }),
            new GiftData("传送药水",2351,1,new int[] { 2, 5 }),
            new GiftData("生命力药水",2345,1,new int[] { 2, 5 }),
            new GiftData("耐力药水",2346,1,new int[] { 2, 5 }),
            new GiftData("强效幸运药水",4479,1,new int[] { 2, 5 }),
            new GiftData("黑曜石皮药水",288,1,new int[] { 2, 5 }),
            new GiftData("羽落药水",295,1,new int[] { 2, 5 }),
            new GiftData("洞穴探险药水",296,1,new int[] { 2, 5 }),
            new GiftData("战斗药水",300,1,new int[] { 2, 5 }),
            new GiftData("挖矿药水",2322,1,new int[] { 2, 5 }),
            new GiftData("生命水晶",29,1,new int[] { 2, 5 }),
            new GiftData("魔镜",50,1,new int[] { 1, 1 }),
            new GiftData("飞虫剑",5129,1,new int[] { 1, 1 }),
            new GiftData("血泪",4271,1,new int[] { 1, 2 }),
            new GiftData("幸运马掌",158,1,new int[] { 1, 1 }),
            new GiftData("超亮头盔",4008,1,new int[] { 1, 1 }),
            new GiftData("非负重石",5391,1,new int[] { 1, 1 }),
            new GiftData("水蜡烛",148,1,new int[] { 3, 5 }),
            new GiftData("暗影蜡烛",5322,1,new int[] { 3, 5 }),
            new GiftData("肥料",602,1,new int[] { 3, 5 }),
            new GiftData("魔法灯笼",3043,1,new int[] { 1,1 }),
            new GiftData("挖矿衣",410,1,new int[] { 1,1 }),
            new GiftData("挖矿裤",411,1,new int[] { 1,1 }),
            new GiftData("熔线钓钩",2422,1,new int[] { 1,1 }),
            new GiftData("湿炸弹",4824,1,new int[] { 5,10 }),
            new GiftData("恶魔海螺",4819,1,new int[] { 1,1 }),
            new GiftData("魔法海螺",4263,1,new int[] { 1,1 }),
            new GiftData("鱼饵桶",4608,1,new int[] { 10,30 }),
            new GiftData("花园侏儒",4609,1,new int[] { 3,5 }),
            new GiftData("掘墓者铲子",4711,1,new int[] { 1,1 }),
            new GiftData("月亮领主腿",5001,1,new int[] { 1,1 }),
            new GiftData("火把神的恩宠",5043,1,new int[] { 1,1 }),
            new GiftData("工匠面包",5326,1,new int[] { 1,1 }),
            new GiftData("闭合的虚空袋",5325,1,new int[] { 1,1 }),
            new GiftData("先进战斗技术",4382,1,new int[] { 1,1 }),
            new GiftData("先进战斗技术：卷二",5336,1,new int[] { 1,1 }),
            new GiftData("闪电胡萝卜",4777,1,new int[] { 1,1 }),
            new GiftData("熔火护身符",4038,1,new int[] { 1,1 }),
            new GiftData("泰拉魔刃",4144,1,new int[] { 1,1 }),
            new GiftData("真空刃",3368,1,new int[] { 1,1 }),
        };
    }
    #endregion

}