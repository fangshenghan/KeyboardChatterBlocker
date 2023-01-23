using AdofaiTweaks.Core;
using AdofaiTweaks.Tweaks.KeyLimiter;
using SkyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardChatterBlocker
{
    public static class Utils
    {

        public static readonly ISet<KeyLabel> ALWAYS_BOUND_ASYNC_KEYS = new HashSet<KeyLabel> {
            KeyLabel.MouseLeft,
            KeyLabel.MouseMiddle,
            KeyLabel.MouseRight,
            KeyLabel.MouseX1,
            KeyLabel.MouseX2,
        };

        public static KeyLimiterSettings GetKeyLimiterSettings()
        {
            object sync = typeof(AdofaiTweaks.AdofaiTweaks).GetField("synchronizer", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            Dictionary<Type, TweakSettings> dict = (Dictionary<Type, TweakSettings>)sync.GetType().GetField("tweakSettingsDictionary", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sync);

            foreach (Type t in dict.Keys)
            {
                if (t.Name.Equals("KeyLimiterSettings"))
                {
                    return (KeyLimiterSettings)dict[t];
                }
            }

            return null;
        }

    }
}
