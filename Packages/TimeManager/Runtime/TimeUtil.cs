using System;
using System.Globalization;
using UnityEngine;

namespace GSDev.Time
{
    public static class TimeUtil
    {
        public static readonly DateTime ZeroUTC = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static readonly DateTime ZeroLocal = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);

        public const string TimeFormatFull = "yyyyMMddHHmmss";
        public const string TimeFormatDate = "yyyyMMdd";

        public static DateTime GetDateTimeByString(
            string timeInfo,
            string timeType,
            bool isLocal)
        {
            if (DateTime.TryParseExact(
                timeInfo,
                timeType,
                CultureInfo.InvariantCulture,
                isLocal ? DateTimeStyles.AssumeLocal : DateTimeStyles.AdjustToUniversal,
                out var dateTime))
                return dateTime;
            
            Debug.LogError("时间配置错误");
            return DateTime.Now;
        }

        #region DateTime Extend

        public static DateTime ParseBy(this DateTime self, string timeInfo, string timeType, bool isLocal)
        {
            if (string.IsNullOrEmpty(timeInfo)) return self;
            if (DateTime.TryParseExact(timeInfo, timeType, CultureInfo.InvariantCulture,
                isLocal ? DateTimeStyles.AssumeLocal : DateTimeStyles.AdjustToUniversal, out var dateTime))
                return dateTime;
            Debug.LogError("时间配置错误");
            return self;
        }

        public static DateTime ParseFull(this DateTime self, string timeInfo, bool isLocal = true)
        {
            return self.ParseBy(timeInfo, TimeFormatFull, isLocal);
        }

        public static DateTime ParseDate(this DateTime self, string timeInfo, bool isLocal = true)
        {
            return self.ParseBy(timeInfo, TimeFormatDate, isLocal);
        }

        public static DateTime ParseDataNone(this DateTime self, string timeInfo, bool isLocal = true)
        {
            return self.ParseBy(timeInfo, "yyyy.M.d", isLocal);
        }

        public static string ToStringDate(this DateTime dateTime)
        {
            return dateTime.ToString(TimeFormatDate, CultureInfo.InvariantCulture);
        }

        public static string ToStringFull(this DateTime dateTime)
        {
            return dateTime.ToString(TimeFormatFull, CultureInfo.InvariantCulture);
        }
        
        public static long TotalSeconds(this DateTime dateTime)
        {
            return dateTime.Kind == DateTimeKind.Local
                ? (long) (dateTime - ZeroLocal).TotalSeconds
                : (long) (dateTime - ZeroUTC).TotalSeconds;
        }
        

        #endregion

        #region 时间转换

        private const int SecondsPerMinute = 60;
        private const int SecondsPerHour = 60 * SecondsPerMinute;
        public const int SecondsPerDay = 24 * SecondsPerHour;
        
        public static void SecondsToDetailTime(
            long totalSeconds,
            out int days,
            out int hours,
            out int minutes,
            out int seconds)
        {
            days = (int) (totalSeconds / SecondsPerDay);
            var leftSeconds = totalSeconds % SecondsPerDay;
            hours = (int) (leftSeconds / SecondsPerHour);
            leftSeconds %= SecondsPerHour;
            minutes = (int) (leftSeconds / SecondsPerMinute);
            seconds = (int) (leftSeconds % SecondsPerMinute);
        }
        
        public static void SecondsToDetailTime(
            int totalSeconds,
            out int days,
            out int hours,
            out int minutes,
            out int seconds)
        {
            days = totalSeconds / SecondsPerDay;
            var leftSeconds = totalSeconds % SecondsPerDay;
            hours = leftSeconds / SecondsPerHour;
            leftSeconds %= SecondsPerHour;
            minutes = leftSeconds / SecondsPerMinute;
            seconds = leftSeconds % SecondsPerMinute;
        }
        
        public static void SecondsToDetailTime(
            long totalSeconds,
            out int hours,
            out int minutes,
            out int seconds)
        {
            hours = (int) (totalSeconds / SecondsPerHour);
            var leftSeconds = totalSeconds % SecondsPerHour;
            minutes = (int) (leftSeconds / SecondsPerMinute);
            seconds = (int) (leftSeconds % SecondsPerMinute);
        }
        
        public static void SecondsToDetailTime(
            int totalSeconds,
            out int hours,
            out int minutes,
            out int seconds)
        {
            hours = totalSeconds / SecondsPerHour;
            var leftSeconds = totalSeconds % SecondsPerHour;
            minutes = leftSeconds / SecondsPerMinute;
            seconds = leftSeconds % SecondsPerMinute;
        }

        #endregion
        
        #region DateTime转换为Unix时间戳

        //DateTime转换为Unix 10位时间戳 UtcNow 和 Now 返回是一样的
        public static long GetTimeStamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        //13位时间戳
        public static long GetTimeStamp13()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        
        //Unix10位时间戳转换为C# DateTime
        public static DateTime TimeStampToDateTime(long seconds)
        {
            var dto = DateTimeOffset.FromUnixTimeSeconds(seconds);
            return dto.LocalDateTime;
        }
        public static DateTime TimeStampToDateTimeUTC(long seconds)
        {
            var dto = DateTimeOffset.FromUnixTimeSeconds(seconds);
            return dto.DateTime;
        }
        //Unix13位时间戳转换为C# DateTime
        public static DateTime TimeStamp13ToDateTime(long milliseconds)
        {
            var dto = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
            return dto.LocalDateTime;
        }
        public static DateTime TimeStamp13ToDateTimeUTC(long milliseconds)
        {
            var dto = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
            return dto.DateTime;
        }

        #endregion
    }
}