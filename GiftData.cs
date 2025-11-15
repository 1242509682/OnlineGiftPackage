using Newtonsoft.Json;

namespace OnlineGift;

public class GiftData
{
    [JsonProperty("物品名称")]
    public string ItemName { get; set; } = "未命名物品";
    [JsonProperty("物品ID")]
    public int ItemType { get; set; } = 0;
    [JsonProperty("所占概率")]
    public int Rate { get; set; } = 1;
    [JsonProperty("物品数量")]
    public int[] Stack { get; set; } = new int[2] { 1, 1 };

    // 新增条件字段
    [JsonProperty("进度条件")]
    public List<string> Conditions { get; set; } = new List<string>();

    public GiftData(string name, int type, int rate, int[] stack, List<string> conditions = null)
    {
        ItemName = name;
        ItemType = type;
        Rate = rate;
        Stack = stack;
        Conditions = conditions;
    }
}