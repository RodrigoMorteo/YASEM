using System;

namespace YASEM.Core.Utilities
{
    public class EnumValidator{
    public static T ValidateEnumValue<T>(string value) where T : struct, Enum
        {
            if (Enum.TryParse<T>(value, out T enumValue) && Enum.IsDefined(typeof(T), enumValue))
            {
                return enumValue;
            }
            
            throw new EnumValidationException(value, typeof(T));
        }
    }
    

    public class EnumValidationException : Exception
    {
        public EnumValidationException(string value, Type enumType) :
            base($"'{value}' is not a valid value of the {enumType.Name} enum.")
        {}
    }

    public class StringUtils {
        public static string FirstCharToUpperString(string input) //see acknowledgements.md [1]my
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            return string.Create(input.Length, input, static (Span<char> chars, string str) =>
            {
                chars[0] = char.ToUpperInvariant(str[0]);
                str.AsSpan(1).CopyTo(chars[1..]);
            });
        }
    }
}