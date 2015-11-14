using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pola.Data
{
    public static class Settings
    {
        private const string DeviceIdKey = "DeviceId";

        public static string DeviceId
        {
            get
            {
                string deviceId = GetValueOrDefault<string>(DeviceIdKey, null);
                if (deviceId == null)
                {
                    deviceId = Guid.NewGuid().ToString();
                    SetValue(DeviceIdKey, deviceId);
                }
                return deviceId;
            }

            set
            {
                SetValue(DeviceIdKey, value);
            }
        }

        private static void SetValue(string key, object value)
        {
            Windows.Storage.ApplicationData.Current.LocalSettings.Values[key] = value;
        }

        private static T GetValueOrDefault<T>(string key, T defaultValue)
        {
            if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                return (T)Windows.Storage.ApplicationData.Current.LocalSettings.Values[key];
            else
                return defaultValue;
        }
    }
}
