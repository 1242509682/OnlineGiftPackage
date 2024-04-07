using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TShockAPI;

namespace 在线礼包
{
    public class GiftConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanConvert(Type objectType)
        {
            return typeof(Gift) == objectType;
        }

        // 优化 ReadJson 方法，加入异常处理和状态验证
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                JObject giftObject = JObject.Load(reader);
                Gift gift = new();

                gift.物品名称 = (string)giftObject["物品名称"];
                gift.物品ID = (int)giftObject["物品ID"];

                JArray? 数量Array = giftObject["物品数量"] as JArray;
                gift.物品数量 = new int[2];
                gift.物品数量[0] = (int)数量Array[0];
                gift.物品数量[1] = (int)数量Array[1];

                gift.所占概率 = (int)giftObject["所占概率"];

                return gift;
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException($"无法反序列化Gift: {ex.Message}", ex);
            }
        }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Gift gift = (Gift)value;

            writer.WriteStartObject();
            writer.WritePropertyName("物品名称");
            writer.WriteValue(gift.物品名称);
            writer.WritePropertyName("物品ID");
            writer.WriteValue(gift.物品ID);
            writer.WritePropertyName("物品数量");
            writer.WriteStartArray();
            writer.WriteValue(gift.物品数量[0]);
            writer.WriteValue(gift.物品数量[1]);
            writer.WriteEndArray();
            writer.WritePropertyName("所占概率");
            writer.WriteValue(gift.所占概率);
            writer.WriteEndObject();
        }
    }

    [JsonConverter(typeof(GiftConverter))]
    public class Gift
    {
        [JsonProperty("物品名称")]
        public string 物品名称 { get; set; }
        [JsonProperty("物品ID")]
        public int 物品ID { get; set; }
        [JsonProperty("物品数量")]
        public int[] 物品数量 { get; set; }
        [JsonProperty("所占概率")]
        public int 所占概率 { get; set; }
    }

    public class Config
    {
        public bool 启用 { get; set; } = true;
        [JsonProperty("总概率")]
        public int 总概率 = 100;
        public TimeSpan 触发时间 { get; set; } = TimeSpan.FromMinutes(30);
        // 新增广播间隔时间属性（单位：分钟）
        [JsonProperty("广播间隔时间/分钟")]
        public int 广播间隔时间 { get; set; } = 30; // 默认间隔30分钟
        public string 广播消息 { get; set; } = "你已连续在线[c/55CDFF:{0}]分钟";
        public Dictionary<int, string> 触发序列 { get; set; } = new Dictionary<int, string>();
        public List<Gift> 礼包列表 { get; set; } = new List<Gift>();

        public static string path = Path.Combine(TShock.SavePath, "在线礼包.json");

        // 保存配置文件的方法
        public static void SaveConfig(Config config)
        {
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(Config.path, json);
        }

        private Config()
        {
            this.触发时间 = TimeSpan.FromMinutes(30);
        }
        public static Config LoadConfig
        {
            get
            {
                if (!File.Exists(path))
                {
                    Config config = new Config();
                    Gift item1 = new Gift()
                    {
                        物品名称 = "铂金币",
                        物品ID = 74,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 3,
                    };
                    config.礼包列表.Add(item1);
                    Gift item2 = new Gift()
                    {
                        物品名称 = "蠕虫罐头",
                        物品ID = 4345,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 3,
                    };
                    config.礼包列表.Add(item2);
                    Gift item3 = new Gift()
                    {
                        物品名称 = "草药袋",
                        物品ID = 3093,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 3,
                    };
                    config.礼包列表.Add(item3);
                    for (int i = 1; i <= 50; i++)
                    {
                        config.触发序列.Add(i * 1, $"你已获得{i}个礼包");
                    }
                    File.WriteAllText(path, JsonConvert.SerializeObject(config));
                    return config;
                }
                else
                {
                    Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
                    config.总概率 = 100;
                    foreach (Gift gift in config.礼包列表)
                    {
                        config.总概率 += gift.所占概率;
                    }
                    if (config == null)
                    {
                        throw new FormatException("配置文件'在线礼包.json'读取出错！");
                    }
                    return config;
                }

            }

        }
    }
}
