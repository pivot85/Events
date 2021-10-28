using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Events.Bot
{
    public static class EnumExtensions
    {
        public static IEnumerable<TEnum> GetFlags<TEnum>(this TEnum e) where TEnum : Enum
        {
            return Enum.GetValues(e.GetType()).Cast<Enum>().Where(e.HasFlag).Cast<TEnum>();
        }

        public static string ToPascalCase(this Enum value)
        {
            return Regex.Replace(value.ToString(), "(\\B[A-Z])", " $1");
        }
    }
}
