using TShockAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace 在线礼包
{
    class GiftConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanConvert(Type objectType)
        {
            return typeof(Gift) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Gift gift = new Gift();
            reader.Read(); // StartArray
            gift.物品名称 = reader.ReadAsString();
            gift.物品ID = reader.ReadAsInt32().Value;
            reader.Read(); // StartArray
            gift.物品数量 = new int[2];
            gift.物品数量[0] = reader.ReadAsInt32().Value;
            gift.物品数量[1] = reader.ReadAsInt32().Value;
            reader.Read(); // EndArray
            gift.所占概率 = reader.ReadAsInt32().Value;
            reader.Read(); // EndArray
            return gift;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Gift gift = value as Gift;
            writer.WriteStartArray();
            writer.WriteValue(gift.物品名称);
            writer.WriteValue(gift.物品ID);
            writer.WriteStartArray();
            writer.WriteValue(gift.物品数量[0]);
            writer.WriteValue(gift.物品数量[1]);
            writer.WriteEndArray();
            writer.WriteValue(gift.所占概率);
            writer.WriteEndArray();
        }
    }

    [JsonConverter(typeof(GiftConverter))]
    public class Gift
    {
        public string 物品名称 { get; set; }
        public int 物品ID { get; set; }
        public int[] 物品数量 { get; set; }
        public int 所占概率 { get; set; }
    }

    public class Config
    {
        public bool 启用 { get; set; } = true;
        public TimeSpan 触发时间 { get; set; } = TimeSpan.FromMinutes(1);
        public Dictionary<int, string> 触发序列 { get; set; } = new Dictionary<int, string>();
        public string 广播消息 { get; set; } = "你已连续在线[c/55CDFF:{0}]分钟";
        public List<Gift> 礼包列表 { get; set; } = new List<Gift>();

        [JsonIgnore]
        public int denominator = 0;

        public static string path = Path.Combine(TShock.SavePath, "在线礼包.json");

        private Config()
        {
            this.触发时间 = TimeSpan.FromMinutes(1);
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
                    Gift item4 = new Gift()
                    {
                        物品名称 = "培根",
                        物品ID = 3532,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 3,
                    };
                    config.礼包列表.Add(item4);
                    Gift item5 = new Gift()
                    {
                        物品名称 = "木匣",
                        物品ID = 2334,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 3,
                    };
                    config.礼包列表.Add(item5);
                    Gift item6 = new Gift()
                    {
                        物品名称 = "铁匣",
                        物品ID = 2335,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 3,
                    };
                    config.礼包列表.Add(item6);
                    Gift item7 = new Gift()
                    {
                        物品名称 = "金匣",
                        物品ID = 2336,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 3,
                    };
                    config.礼包列表.Add(item7);
                    Gift item8 = new Gift()
                    {
                        物品名称 = "地牢匣",
                        物品ID = 3205,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 3,
                    };
                    config.礼包列表.Add(item8);
                    Gift item9 = new Gift()
                    {
                        物品名称 = "天空匣",
                        物品ID = 3206,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 3,
                    };
                    config.礼包列表.Add(item9);
                    Gift item10 = new Gift()
                    {
                        物品名称 = "冰冻匣",
                        物品ID = 4405,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 3,
                    };
                    config.礼包列表.Add(item10);
                    Gift item11 = new Gift()
                    {
                        物品名称 = "绿洲匣",
                        物品ID = 4407,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 3,
                    };
                    config.礼包列表.Add(item11);
                    Gift item12 = new Gift()
                    {
                        物品名称 = "黑曜石匣",
                        物品ID = 4877,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 3,
                    };
                    config.礼包列表.Add(item12);
                    Gift item13 = new Gift()
                    {
                        物品名称 = "海洋匣",
                        物品ID = 5002,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 3,
                    };
                    config.礼包列表.Add(item13);
                    Gift item14 = new Gift()
                    {
                        物品名称 = "暴怒药水",
                        物品ID = 2347,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 3,
                    };
                    config.礼包列表.Add(item14);
                    Gift item15 = new Gift()
                    {
                        物品名称 = "怒气药水",
                        物品ID = 2349,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 3,
                    };
                    config.礼包列表.Add(item15);
                    Gift item16 = new Gift()
                    {
                        物品名称 = "传送药水",
                        物品ID = 2351,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 2,
                    };
                    config.礼包列表.Add(item16);
                    Gift item17 = new Gift()
                    {
                        物品名称 = "生命力药水",
                        物品ID = 2345,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 1,
                    };
                    config.礼包列表.Add(item17);
                    Gift item18 = new Gift()
                    {
                        物品名称 = "耐力药水",
                        物品ID = 2346,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 3,
                    };
                    config.礼包列表.Add(item18);
                    Gift item19 = new Gift()
                    {
                        物品名称 = "强效幸运药水",
                        物品ID = 4479,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 1,
                    };
                    config.礼包列表.Add(item19);
                    Gift item20 = new Gift()
                    {
                        物品名称 = "黑曜石皮药水",
                        物品ID = 288,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 3,
                    };
                    config.礼包列表.Add(item20);
                    Gift item21 = new Gift()
                    {
                        物品名称 = "羽落药水",
                        物品ID = 295,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 3,
                    };
                    config.礼包列表.Add(item21);
                    Gift item22 = new Gift()
                    {
                        物品名称 = "洞穴探险药水",
                        物品ID = 296,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 3,
                    };
                    config.礼包列表.Add(item22);
                    Gift item23 = new Gift()
                    {
                        物品名称 = "战斗药水",
                        物品ID = 300,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 3,
                    };
                    config.礼包列表.Add(item23);
                    Gift item24 = new Gift()
                    {
                        物品名称 = "挖矿药水",
                        物品ID = 2322,
                        物品数量 = new int[] { 3, 8 },
                        所占概率 = 3,
                    };
                    config.礼包列表.Add(item24);
                    for (int i = 1; i <= 200; i++)
                    {
                        config.触发序列.Add(i * 1, $"你已获得{i}个礼包");
                    }

                    File.WriteAllText(path, JsonConvert.SerializeObject(config));
                    return config;
                }
                else
                {
                    Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
                    config.denominator = 0;
                    foreach (Gift gift in config.礼包列表)
                    {
                        config.denominator += gift.所占概率;
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