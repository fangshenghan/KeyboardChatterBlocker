using System;
using System.Collections.Generic;
using System.Reflection;
using AdofaiTweaks.Compat.Async;
using AdofaiTweaks.Core;
using AdofaiTweaks.Tweaks.KeyLimiter;
using HarmonyLib;
using SkyHook;
using UnityEngine;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;

namespace KeyboardChatterBlocker
{
    public static class Patch
    {

        public static Dictionary<ushort, long> lastKeyPressAsync = new Dictionary<ushort, long>();
        public static Dictionary<KeyCode, long> lastKeyPress = new Dictionary<KeyCode, long>();

        [HarmonyPatch(typeof(scnEditor), "Play")]
        public static class scnEditor_Play
        {
            public static void Prefix()
            {
                ModEntry tweaks = UnityModManager.FindMod("AdofaiTweaks");
                if (tweaks != null && tweaks.Active)
                {
                    Main.usingTweaks = true;
                }
                else
                {
                    Main.usingTweaks = false;
                }
                if (Main.usingTweaks)
                {
                    Main.kls = Utils.GetKeyLimiterSettings();
                }
            }
        }

        [HarmonyPatch(typeof(scrController), "CountValidKeysPressed")]
        [HarmonyBefore("adofai_tweaks.key_limiter")]
        public static class scrController_CountValidKeysPressed
        {
            public static bool Prefix(ref int __result)
            {
                if (Main.usingTweaks && Main.kls.IsEnabled)
                {
                    __result = Mathf.Min(4, scrController_CountValidKeysPressed_Tweaks());
                }
                else
                {
                    __result = Mathf.Min(4, scrController_CountValidKeysPressed_NoTweaks());
                }
                return false;
            }
        }
        

        private static int scrController_CountValidKeysPressed_NoTweaks()
        {
            int num = 0;
            if (AsyncInputManager.isActive)
            {
                long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                foreach (AnyKeyCode akc in RDInput.GetMainPressKeys())
                {
                    if (akc.value is AsyncKeyCode && AsyncInput.GetKeyDown((AsyncKeyCode)akc.value, false))
                    {
                        ushort keycode = ((AsyncKeyCode)akc.value).key;
                        if (!lastKeyPressAsync.ContainsKey(keycode))
                        {
                            lastKeyPressAsync.Add(keycode, 0L);
                        }
                        if (now - lastKeyPressAsync[keycode] > (long)Main.setting.inputInterval || now - lastKeyPressAsync[keycode] <= 2L)
                        {
                            num++;
                            lastKeyPressAsync[keycode] = now;
                        }
                        else
                        {
                            Main.Logger.Log("Blocked Async Key: " + keycode + " time: " + (now - lastKeyPressAsync[keycode]) + "ms.");
                        }
                    }
                }
            }
            else
            {
                long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                foreach (int k in Enum.GetValues(typeof(KeyCode)))
                {
                    KeyCode keycode = (KeyCode)k;
                    if (Input.GetKeyDown(keycode))
                    {
                        if (!lastKeyPress.ContainsKey(keycode))
                        {
                            lastKeyPress.Add(keycode, 0L);
                        }
                        if (now - lastKeyPress[keycode] > (long)Main.setting.inputInterval || now - lastKeyPress[keycode] <= 2L)
                        {
                            num++;
                            lastKeyPress[keycode] = now;
                        }
                        else
                        {
                            Main.Logger.Log("Blocked Key: " + keycode + " time: " + (now - lastKeyPress[keycode]) + "ms.");
                        }
                    }
                }
            }
            return num;
        }

        private static int scrController_CountValidKeysPressed_Tweaks()
        {
            int num = 0;
            if (AsyncInputManager.isActive)
            {
                long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                foreach (ushort keycode in Main.kls.ActiveAsyncKeys)
                {
                    if (AsyncInput.GetKeyDown(keycode, false))
                    {
                        if (!lastKeyPressAsync.ContainsKey(keycode))
                        {
                            lastKeyPressAsync.Add(keycode, 0L);
                        }
                        if (now - lastKeyPressAsync[keycode] > (long)Main.setting.inputInterval || now - lastKeyPressAsync[keycode] <= 1L)
                        {
                            num++;
                            lastKeyPressAsync[keycode] = now;
                        }
                        else
                        {
                            Main.Logger.Log("Blocked Async Key: " + keycode + " time: " + (now - lastKeyPressAsync[keycode]) + "ms (Using AdofaiTweaks).");
                        }
                    }
                }
                foreach (KeyLabel keycode in Utils.ALWAYS_BOUND_ASYNC_KEYS)
                {
                    if (AsyncInput.GetKeyDown(keycode, false))
                    {
                        if (!lastKeyPressAsync.ContainsKey((ushort)keycode))
                        {
                            lastKeyPressAsync.Add((ushort)keycode, 0L);
                        }
                        if (now - lastKeyPressAsync[(ushort)keycode] > (long)Main.setting.inputInterval || now - lastKeyPressAsync[(ushort)keycode] <= 1L)
                        {
                            num++;
                            lastKeyPressAsync[(ushort)keycode] = now;
                        }
                        else
                        {
                            Main.Logger.Log("Blocked Async Key: " + keycode + " time: " + (now - lastKeyPressAsync[(ushort)keycode]) + "ms (Using AdofaiTweaks).");
                        }
                    }
                }
            }
            else
            {
                long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                foreach (KeyCode keycode in Main.kls.ActiveKeys)
                {
                    if (Input.GetKeyDown(keycode))
                    {
                        if (!lastKeyPress.ContainsKey(keycode))
                        {
                            lastKeyPress.Add(keycode, 0L);
                        }
                        if (now - lastKeyPress[keycode] > (long)Main.setting.inputInterval || now - lastKeyPress[keycode] <= 2L)
                        {
                            num++;
                            lastKeyPress[keycode] = now;
                        }
                        else
                        {
                            Main.Logger.Log("Blocked Key: " + keycode + " time: " + (now - lastKeyPress[keycode]) + "ms (Using AdofaiTweaks).");
                        }
                    }
                }

                foreach (KeyCode keycode in KeyLimiterTweak.ALWAYS_BOUND_KEYS)
                {
                    if (Input.GetKeyDown(keycode))
                    {
                        if (!lastKeyPress.ContainsKey(keycode))
                        {
                            lastKeyPress.Add(keycode, 0L);
                        }
                        if (now - lastKeyPress[keycode] > (long)Main.setting.inputInterval || now - lastKeyPress[keycode] <= 2L)
                        {
                            num++;
                            lastKeyPress[keycode] = now;
                        }
                        else
                        {
                            Main.Logger.Log("Blocked Key: " + keycode + " time: " + (now - lastKeyPress[keycode]) + "ms (Using AdofaiTweaks).");
                        }
                    }
                }
            }
            return num;
        }
    }
}
