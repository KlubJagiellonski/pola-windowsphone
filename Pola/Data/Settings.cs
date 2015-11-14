using System;

namespace Pola.Data
{
    /// <summary>
    /// Provides application settings as static properties. 
    /// </summary>
    public static class Settings
    {
        private const string DeviceIdKey = "DeviceId";

        /// <summary>
        /// Gets device ID generated from GUID. It stays the same as long as the app is isntalled on the phone.
        /// </summary>
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
