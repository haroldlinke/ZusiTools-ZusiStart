using log4net;
using System;
using System.Globalization;

namespace ZusiKlassenLib2.Common
{
    public static class ZusiDate
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ZusiDate));

        private static readonly string[] _formats = new string[]
        {
            LongDateFormat,
            ShortDateFormat,
            "yyyy-MM-dd HH:m:ss"
        };

        private static readonly DateTimeStyles _defaultStyle =
            DateTimeStyles.AllowInnerWhite |
            DateTimeStyles.AllowLeadingWhite |
            DateTimeStyles.AllowTrailingWhite |
            DateTimeStyles.AllowWhiteSpaces |
            DateTimeStyles.AssumeLocal;

        public const string LongDateFormat = "yyyy-MM-dd HH:mm:ss";
        public const string ShortDateFormat = "yyyy-MM-dd";

        //---------------------------------------------------------------------
        public static DateTime? Parse(string dateString)
        {
            if (!string.IsNullOrEmpty(dateString))
            {
                if (DateTime.TryParseExact(dateString.Trim(), _formats, CultureInfo.InvariantCulture, _defaultStyle, out DateTime value))
                {
                    return value;
                }

                Log.ErrorFormat("Could not recognise '{0}' as a valid date.", dateString);
            }

            return null;
        }
    }
}
