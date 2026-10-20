
using System;

namespace KOOP.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converts a <see cref="DateTime"/> object into a version of itself with
        /// no identifier specifying whether the time is in local time or in UTC.
        /// </summary>
        public static DateTime Sanitize(this DateTime dateTime)
        {
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
        }
    }
}