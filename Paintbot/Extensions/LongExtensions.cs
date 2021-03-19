namespace PaintBot.Extensions
{
    using System;

    public static class LongExtensions
    {
        public static DateTime ToDateTime(this long timestamp)
        {
            try
            {
                return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
            }
            catch (ArgumentOutOfRangeException)
            {
                return DateTime.UtcNow;
            }
        }
    }
}