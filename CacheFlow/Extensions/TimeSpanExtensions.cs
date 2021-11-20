using System;

namespace FloxDc.CacheFlow.Extensions;

public static class TimeSpanExtensions
{
    public static TimeSpan BeforeMinuteEnds() 
        => GetNextFrame(1);

        
    public static TimeSpan BeforeTwoMinutesEnd() 
        => GetNextFrame(2);

        
    public static TimeSpan BeforeFiveMinutesEnd() 
        => GetNextFrame(5);

        
    public static TimeSpan BeforeTenMinutesEnd() 
        => GetNextFrame(10);

        
    public static TimeSpan BeforeQuarterHourEnds() 
        => GetNextFrame(15);

        
    public static TimeSpan BeforeHalfHourEnds() 
        => GetNextFrame(30);


    public static TimeSpan BeforeHourEnds()
    {
        var now = DateTime.UtcNow;
        var nextHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc).AddHours(1);

        return nextHour - now;
    }


    public static TimeSpan BeforeDayEnds()
    {
        var now = DateTime.UtcNow;
        var nextDay = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(1);

        return nextDay - now;
    }

        
    private static TimeSpan GetNextFrame(int frameInMinutes)
    {
        var now = DateTime.UtcNow;
        var frame = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Utc).AddMinutes(frameInMinutes);

        return frame - now;
    }
}