using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace MusicSorter2
{
    public class RegistrySettings : UserSettings
    {
        string SubKey;
        public RegistrySettings(string sub_key)
        {
            this.SubKey = sub_key;
            Registry.CurrentUser.CreateSubKey(sub_key);
        }

        public override object GetValue(string key,object default_val= null)
        {
            object result = null;
            RegistryReadOperation(delegate (RegistryKey registry_sub_key)
            {
                result = registry_sub_key.GetValue(key, default_val);
            });

            return result;
        }

        public override void SetValue(string key, object value)
        {
            RegistryWriteOperation(rsk => rsk.SetValue(key, value));
        }


        void RegistryOperation(bool write, Action<RegistryKey> action)
        {
            RegistryKey current_user = Registry.CurrentUser;
            RegistryKey registry_sub_key =
                write ? current_user.CreateSubKey(SubKey) : current_user.OpenSubKey(SubKey);

            action.Invoke(registry_sub_key);

            registry_sub_key.Close();
            current_user.Close();
        }

        void RegistryReadOperation(Action<RegistryKey> action)
        {
            RegistryOperation(false, action);
        }

        void RegistryWriteOperation(Action<RegistryKey> action)
        {
            RegistryOperation(true, action);
        }
    }
}
