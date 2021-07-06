using System;

namespace Utils
{
	public class TimeUtils
	{
		public const long TicksInSecond = 10_000_000;
		
		public static string SecondsToMS(long seconds)
		{
			var timeSpan = TimeSpan.FromSeconds(seconds);
			if (timeSpan.TotalMinutes >= 1)
				return timeSpan.ToString("mm\\:ss");
			return timeSpan.ToString("ss");
		}

		public static string SecondsToHMS(long seconds)
		{
			var timeSpan = TimeSpan.FromSeconds(seconds);
			if (timeSpan.TotalHours >= 1)
				return timeSpan.ToString("hh\\:mm\\:ss");
			if (timeSpan.TotalMinutes >= 1)
				return timeSpan.ToString("mm\\:ss");
			return timeSpan.ToString("ss");
		}

		public static string TicksToHMS(long ticks)
		{
			var timeSpan = TimeSpan.FromTicks(ticks);
			return timeSpan.ToString("hh\\:mm\\:ss");
		}
	}
}