using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using TShockAPI;

namespace OnlineGift;

public class PlayerStats
{
    public string PlayerName { get; set; } = string.Empty;
    public string UUID { get; set; } = string.Empty;
    public long TotalOnlineSeconds { get; set; } = 0;
    public int TotalGiftsReceived { get; set; } = 0;
    public int TodayGiftsReceived { get; set; } = 0;
    public int ConsecutiveDays { get; set; } = 0;
    public DateTime LastGiftTime { get; set; } = DateTime.MinValue;
    public DateTime LastLoginTime { get; set; } = DateTime.Now;
    public DateTime LastResetDate { get; set; } = DateTime.Now.Date;
    public DateTime LastGiftSendTime { get; set; } = DateTime.Now; // 用于剩余时间计算
}

public static class StatsManager
{
    private static readonly string StatsFolderPath = Path.Combine(Configuration.FolderPath, "统计数据");
    private static Dictionary<string, PlayerStats> playerStats = new Dictionary<string, PlayerStats>();

    #region GZip 压缩辅助方法
    private static Stream GZipWrite(string filePath)
    {
        var fileStream = new FileStream(filePath, FileMode.Create);
        return new GZipStream(fileStream, CompressionLevel.Optimal);
    }

    private static Stream GZipRead(string filePath)
    {
        var fileStream = new FileStream(filePath, FileMode.Open);
        return new GZipStream(fileStream, CompressionMode.Decompress);
    }
    #endregion

    #region 加载所有玩家统计数据方法
    public static void LoadAllStats()
    {
        try
        {
            Directory.CreateDirectory(StatsFolderPath);
            playerStats.Clear();

            var files = Directory.GetFiles(StatsFolderPath, "*.dat");
            foreach (var file in files)
            {
                try
                {
                    using var stream = GZipRead(file);
                    using var reader = new BinaryReader(stream);

                    var stats = new PlayerStats
                    {
                        PlayerName = reader.ReadString(),
                        UUID = reader.ReadString(),
                        TotalOnlineSeconds = reader.ReadInt64(),
                        TotalGiftsReceived = reader.ReadInt32(),
                        TodayGiftsReceived = reader.ReadInt32(),
                        ConsecutiveDays = reader.ReadInt32(),
                        LastGiftTime = DateTime.FromBinary(reader.ReadInt64()),
                        LastLoginTime = DateTime.FromBinary(reader.ReadInt64()),
                        LastResetDate = DateTime.FromBinary(reader.ReadInt64()),
                        LastGiftSendTime = DateTime.FromBinary(reader.ReadInt64())
                    };

                    playerStats[stats.PlayerName] = stats;
                }
                catch (Exception ex)
                {
                    TShock.Log.ConsoleError($"[在线礼包] 加载玩家统计文件失败 {file}: {ex.Message}");
                }
            }

            TShock.Log.ConsoleInfo($"[在线礼包] 已加载 {playerStats.Count} 个玩家的统计数据");
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleError($"[在线礼包] 加载统计文件失败: {ex.Message}");
        }
    }
    #endregion

    #region 保存所有玩家统计数据方法
    public static void SaveAllStats()
    {
        try
        {
            Directory.CreateDirectory(StatsFolderPath);

            foreach (var kvp in playerStats)
            {
                var stats = kvp.Value;
                string filePath = Path.Combine(StatsFolderPath, $"{kvp.Key}.dat");

                try
                {
                    using var stream = GZipWrite(filePath);
                    using var writer = new BinaryWriter(stream);

                    writer.Write(stats.PlayerName);
                    writer.Write(stats.UUID);
                    writer.Write(stats.TotalOnlineSeconds);
                    writer.Write(stats.TotalGiftsReceived);
                    writer.Write(stats.TodayGiftsReceived);
                    writer.Write(stats.ConsecutiveDays);
                    writer.Write(stats.LastGiftTime.ToBinary());
                    writer.Write(stats.LastLoginTime.ToBinary());
                    writer.Write(stats.LastResetDate.ToBinary());
                    writer.Write(stats.LastGiftSendTime.ToBinary());
                }
                catch (Exception ex)
                {
                    TShock.Log.ConsoleError($"[在线礼包] 保存玩家统计文件失败 {filePath}: {ex.Message}");
                    TShock.Log.ConsoleError($"[在线礼包] 堆栈跟踪: {ex.StackTrace}");
                }
            }
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleError($"[在线礼包] 保存统计文件失败: {ex.Message}");
        }
    }
    #endregion

    #region 保存指定玩家数据方法
    public static void SaveStats(TSPlayer plr)
    {
        if (plr?.Name == null) return;

        if (playerStats.TryGetValue(plr.Name, out var stats))
        {
            string filePath = Path.Combine(StatsFolderPath, $"{plr.Name}.dat");

            try
            {
                using var stream = GZipWrite(filePath);
                using var writer = new BinaryWriter(stream);

                writer.Write(stats.PlayerName);
                writer.Write(stats.UUID);
                writer.Write(stats.TotalOnlineSeconds);
                writer.Write(stats.TotalGiftsReceived);
                writer.Write(stats.TodayGiftsReceived);
                writer.Write(stats.ConsecutiveDays);
                writer.Write(stats.LastGiftTime.ToBinary());
                writer.Write(stats.LastLoginTime.ToBinary());
                writer.Write(stats.LastResetDate.ToBinary());
                writer.Write(stats.LastGiftSendTime.ToBinary());
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError($"[在线礼包] 保存玩家统计文件失败 {filePath}: {ex.Message}");
            }
        }
    }
    #endregion

    #region 获取指定玩家数据方法
    public static PlayerStats GetStats(TSPlayer plr)
    {
        if (plr?.Name == null) return new PlayerStats();

        if (!playerStats.TryGetValue(plr.Name, out var stats))
        {
            stats = new PlayerStats
            {
                PlayerName = plr.Name,
                UUID = plr.UUID,
                LastLoginTime = DateTime.Now,
                LastGiftSendTime = DateTime.Now // 初始化礼包发送时间
            };
            playerStats[plr.Name] = stats;
        }

        // 检查是否需要重置每日统计
        if (stats.LastResetDate < DateTime.Today)
        {
            stats.TodayGiftsReceived = 0;
            stats.LastResetDate = DateTime.Today;

            // 检查连续天数
            if ((DateTime.Today - stats.LastGiftTime.Date).TotalDays <= 1)
            {
                stats.ConsecutiveDays++;
            }
            else
            {
                stats.ConsecutiveDays = 1;
            }
        }

        return stats;
    }
    #endregion

    #region 更新指定玩家数据方法
    public static void UpdateStats(TSPlayer plr, int giftsReceived = 0)
    {
        var stats = GetStats(plr);

        if (giftsReceived > 0)
        {
            stats.TotalGiftsReceived += giftsReceived;
            stats.TodayGiftsReceived += giftsReceived;
            stats.LastGiftTime = DateTime.Now;
            stats.LastGiftSendTime = DateTime.Now; // 更新礼包发送时间
        }

        // 检查连续天数
        if (stats.LastGiftTime.Date == DateTime.Today.AddDays(-1).Date)
        {
            // 昨天领取过，连续天数+1
            stats.ConsecutiveDays++;
        }
        // 如果是今天第一次领取，不改变连续天数（已经在每日重置时处理）
        else if (stats.LastGiftTime.Date < DateTime.Today.AddDays(-1).Date)
        {
            // 超过一天没领取，重置连续天数
            stats.ConsecutiveDays = 1;
        }
       
        // 更新在线时间（粗略计算）
        var onlineTime = DateTime.Now - stats.LastLoginTime;
        stats.TotalOnlineSeconds += (long)onlineTime.TotalSeconds;
        stats.LastLoginTime = DateTime.Now;
        SaveStats(plr);
    }
    #endregion

    #region 获取排行榜方法
    public static List<PlayerStats> GetLeaderboard(string type = "gifts", int count = 10)
    {
        var statsList = playerStats.Values.ToList();

        return type.ToLower() switch
        {
            "time" => statsList.OrderByDescending(s => s.TotalOnlineSeconds).Take(count).ToList(),
            "days" => statsList.OrderByDescending(s => s.ConsecutiveDays).Take(count).ToList(),
            _ => statsList.OrderByDescending(s => s.TotalGiftsReceived).Take(count).ToList()
        };
    } 
    #endregion

    #region 删除统计文件方法
    public static void DeleteStatsFiles()
    {
        try
        {
            if (!Directory.Exists(StatsFolderPath))
                return;

            var files = Directory.GetFiles(StatsFolderPath, "*.dat");
            int deletedCount = 0;

            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                    deletedCount++;
                }
                catch (Exception ex)
                {
                    TShock.Log.ConsoleError($"[在线礼包] 删除统计文件失败 {file}: {ex.Message}");
                }
            }

            // 清空内存中的统计数据
            playerStats.Clear();

            TShock.Log.ConsoleInfo($"[在线礼包] 已删除 {deletedCount} 个统计文件");
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleError($"[在线礼包] 删除统计文件失败: {ex.Message}");
        }
    }
    #endregion
}