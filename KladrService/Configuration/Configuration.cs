using System;
using System.Configuration;

namespace KladrService.Configuration
{
    static class Configuration
    {
        public static T GetAppSettingsValue<T>(string name, T defaultValue)
        {
            object value = ConfigurationManager.AppSettings[name];
            try
            {
                if (value == null)
                {
                    throw new FormatException("Нет данных");
                }
                else
                {
                    if (typeof(T).IsEnum)
                    {
                        value = (T)(Enum.Parse(typeof(T), value.ToString()));
                    }
                    else
                    {
                        value = (T)Convert.ChangeType(value, typeof(T));
                    }
                }
            }
            catch (FormatException)
            {
                value = defaultValue;
            }
            return (T)value;
        }
    }
}
