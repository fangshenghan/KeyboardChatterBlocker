using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public static Dictionary<ushort, long> countedAsync = new Dictionary<ushort, long>();
        public static Dictionary<KeyCode, long> counted = new Dictionary<KeyCode, long>();

        [HarmonyPatch(typeof(scrController), "CountValidKeysPressed")]
        [HarmonyBefore("adofai_tweaks.key_limiter")]
        public static class scrController_CountValidKeysPressed
        {
            public static bool Prefix(ref int __result)
            {
                __result = scrController_CountValidKeysPressed_NoTweaks();
                return false;
            }
        }

        private static int scrController_CountValidKeysPressed_NoTweaks()
        {
            int num = 0;
            if (AsyncInputManager.isActive)
            {
                long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                foreach (AsyncKeyCode k in AsyncInputManager.keyDownMask)
                {
                    if (Main.setting.ignoredAsyncKeys.Contains(k.key))
                    {
                        num++;
                        continue;
                    }
                    if (!lastKeyPressAsync.ContainsKey(k.key))
                    {
                        lastKeyPressAsync.Add(k.key, 0L);
                    }
                    if (now - lastKeyPressAsync[k.key] > (long)Main.setting.inputInterval || now - lastKeyPressAsync[k.key] <= 2L)
                    {
                        num++;
                        lastKeyPressAsync[k.key] = now;
                    }
                    else
                    {
                        Main.Logger.Log("Blocked Async Key: " + k.label + " time: " + (now - lastKeyPressAsync[k.key]) + "ms.");
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
                        if (Main.setting.ignoredKeys.Contains(keycode))
                        {
                            num++;
                            continue;
                        }
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
    }
}
