using System;

namespace qualityassurance.tools
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
}