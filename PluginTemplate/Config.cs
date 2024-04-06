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
        [JsonProperty("所占概率")]
        public int 所占概率 { get; set; }
        [JsonProperty("物品数量")]
        public int[] 物品数量 { get; set; }

    }


    public class Config
    {
        public bool 启用 { get; set; } = true;
        [JsonProperty("总概率")]
        public int 总概率 = 100;
        public TimeSpan 触发时间 { get; set; } = TimeSpan.FromSeconds(1800);
        public Dictionary<int, string> 触发序列 { get; set; } = new Dictionary<int, string>();
        public List<Gift> 礼包列表 { get; set; } = new List<Gift>();
        // 添加一个计算总概率的方法
        public int CalculateTotalProbability() => 礼包列表.Sum(gift => gift.所占概率);
        public static string path = Path.Combine(TShock.SavePath, "在线礼包.json");
        public Config()
        {
            foreach (Gift gift in 礼包列表)
            {
                总概率 += gift.所占概率;
            }
        }

        // 保存配置文件的方法
        public static void SaveConfig(Config config)
        {
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(Config.path, json);
        }

        public static Config LoadOrCreateConfig()
        {
            Config config = new Config
            {
                礼包列表 = new List<Gift>
        {
            new Gift
            {
                物品名称 = "铂金币",
                物品ID = 74,
                所占概率 = 1,
                物品数量 = new int[] { 2, 5 },
            },
            new Gift
            {
                物品名称 = "蠕虫罐头",
                物品ID = 4345,
                所占概率 = 1,
                物品数量 = new int[] { 2, 5 },

            },
            new Gift
            {
                物品名称 = "草药袋",
                物品ID = 3093,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "培根",
                物品ID = 3532,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "木匣",
                物品ID = 2334,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "铁匣",
                物品ID = 2335,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },

            },
            new Gift
            {
                物品名称 = "金匣",
                物品ID = 2336,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "地牢匣",
                物品ID = 3205,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "天空匣",
                物品ID = 3206,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "冰冻匣",
                物品ID = 4405,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "绿洲匣",
                物品ID = 4407,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "黑曜石匣",
                物品ID = 4877,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "海洋匣",
                物品ID = 5002,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "暴怒药水",
                物品ID = 2347,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "怒气药水",
                物品ID = 2349,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "传送药水",
                物品ID = 2351,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "生命力药水",
                物品ID = 2345,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "耐力药水",
                物品ID = 2346,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "强效幸运药水",
                物品ID = 4479,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "黑曜石皮药水",
                物品ID = 288,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "羽落药水",
                物品ID = 295,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "洞穴探险药水",
                物品ID = 296,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "战斗药水",
                物品ID = 300,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "挖矿药水",
                物品ID = 2322,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "生命水晶",
                物品ID = 29,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "魔镜",
                物品ID = 50,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "飞虫剑",
                物品ID = 5129,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "草药袋",
                物品ID = 3093,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "血泪",
                物品ID = 4271,
                所占概率 = 1,
                物品数量 = new int[] {1, 2 },
            },
            new Gift
            {
                物品名称 = "幸运马掌",
                物品ID = 158,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "超亮头盔",
                物品ID = 4008,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "非负重石",
                物品ID = 5391,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "水蜡烛",
                物品ID = 148,
                所占概率 = 1,
                物品数量 = new int[] {3, 5 },
            },
            new Gift
            {
                物品名称 = "暗影蜡烛",
                物品ID = 5322,
                所占概率 = 1,
                物品数量 = new int[] {3, 5 },
            },
            new Gift
            {
                物品名称 = "肥料",
                物品ID = 602,
                所占概率 = 1,
                物品数量 = new int[] {3, 5 },
            },
            new Gift
            {
                物品名称 = "魔法灯笼",
                物品ID = 3043,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "挖矿衣",
                物品ID = 410,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "挖矿裤",
                物品ID = 411,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "熔线钓钩",
                物品ID = 2422,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "无底微光桶",
                物品ID = 5364,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "无底蜂蜜桶",
                物品ID = 5302,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "无底熔岩桶",
                物品ID = 4820,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "无底水桶",
                物品ID = 3031,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "湿炸弹",
                物品ID = 4824,
                所占概率 = 1,
                物品数量 = new int[] {3, 5 },
            },
            new Gift
            {
                物品名称 = "恶魔海螺",
                物品ID = 4819,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "魔法海螺",
                物品ID = 4263,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "鱼饵桶",
                物品ID = 4608,
                所占概率 = 1,
                物品数量 = new int[] {5, 9 },
            },
            new Gift
            {
                物品名称 = "花园侏儒",
                物品ID = 4609,
                所占概率 = 1,
                物品数量 = new int[] {2, 5 },
            },
            new Gift
            {
                物品名称 = "掘墓者铲子",
                物品ID = 4711,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "万能晶塔",
                物品ID = 4951,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "月亮领主腿",
                物品ID = 5001,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "火把神的恩宠",
                物品ID = 5043,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "工匠面包",
                物品ID = 5326,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "闭合的虚空袋",
                物品ID = 5325,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "先进战斗技术：卷二",
                物品ID = 5336,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "先进战斗技术",
                物品ID = 4382,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "闪电胡萝卜",
                物品ID = 4777,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "熔火护身符",
                物品ID = 4038,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "泰拉魔刃",
                物品ID = 4144,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
            new Gift
            {
                物品名称 = "真空刃",
                物品ID = 3368,
                所占概率 = 1,
                物品数量 = new int[] {1, 1 },
            },
        },
            };

            // 如果需要将总概率限制为100，可以在加载完所有Gift后，根据实际情况调整所占概率
            foreach (Gift gift in config.礼包列表)
            {
                //强制所有Gift的所占概率之和为100，则可以做如下操作：
                 gift.所占概率 *= 100 / config.礼包列表.Sum(g => g.所占概率);
            }

            // 初始化触发序列，这里只有一条记录，可直接插入无需循环
            for (int i = 1; i <= 5; i++)
            {
                config.触发序列.Add(i * 1, $"你已获得{i}个礼包");
            }


            if (!File.Exists(path))
            {
                int totalProbability = config.CalculateTotalProbability();
                Console.WriteLine($"所有礼包的总概率为：{totalProbability}");
                File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented, new GiftConverter()));
            }


            // 序列化并写入文件时，使用触发时间的字符串形式
            File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented, new GiftConverter()));
            return config;
        }


        public static Gift SelectRandomGift(Config config)
        {
            Random random = new Random();
            int randomNum = random.Next(0, config.CalculateTotalProbability());

            Gift selectedGift = default!;
            foreach (Gift gift in config.礼包列表)
            {
                randomNum -= gift.所占概率;
                if (randomNum < 0)
                {
                    selectedGift = gift;
                    break;
                }
            }

            // 确保找到一个礼包（即使概率设置不正确），在这种情况下，至少返回列表中的第一个礼包
            return selectedGift ?? config.礼包列表[0];
        }


        public static void Main(string[] args)
        {
            Config config = LoadOrCreateConfig();

            // 序列化配置并同时写入文件
            SaveConfig(config);

            //发放一个随机礼包
            Gift randomGift = SelectRandomGift(config);
            Console.WriteLine($"发放的礼包为：{randomGift.物品名称}");

        }
    }
}