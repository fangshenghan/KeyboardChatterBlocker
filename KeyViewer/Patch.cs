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
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            scrController ins = scrController.instance;
            ins.keyLimiterOverCounter = 0;

            Dictionary<AnyKeyCode, float> downKeysDuration = (Dictionary<AnyKeyCode, float>)AccessTools.Field(ins.GetType(), "downKeysDuration").GetValue(ins);
            if ((States)ins.stateMachine.GetState() != States.PlayerControl)
            {
                downKeysDuration.Clear();
                ins.maximumUsedKeys = 0;
            }

            int num2 = ADOBase.controller.unlockKeyLimiter ? 16 : 10;
            for (int j = downKeysDuration.Count - 1; j >= 0; j--)
            {
                KeyValuePair<AnyKeyCode, float> keyValuePair = downKeysDuration.ElementAt(j);
                if (Time.time - keyValuePair.Value >= 0.5f)
                {
                    downKeysDuration.Remove(keyValuePair.Key);
                }
            }
            foreach (AnyKeyCode anyKeyCode in RDInput.GetMainPressKeys())
            {
                bool flag = false;
                if (!downKeysDuration.ContainsKey(anyKeyCode) && downKeysDuration.Count >= num2)
                {
                    ins.keyLimiterOverCounter++;
                    flag = true;
                }
                if (!flag)
                {
                    downKeysDuration[anyKeyCode] = Time.time;
                }
                object value = anyKeyCode.value;
                if (value is KeyCode)
                {
                    KeyCode keycode = (KeyCode)value;
                    ins.keyFrequency[keycode] = (ins.keyFrequency.ContainsKey(keycode) ? (ins.keyFrequency[keycode] + 1) : 0);
                    ins.keyTotal++;

                    if (!Main.setting.enableKeyLimiter || Main.setting.allowedKeys.Contains(keycode))
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

                if (value is AsyncKeyCode)
                {
                    AsyncKeyCode k = (AsyncKeyCode)value;
                    ins.keyFrequency[k] = (ins.keyFrequency.ContainsKey(k) ? (ins.keyFrequency[k] + 1) : 0);
                    ins.keyTotal++;

                    if (!Main.setting.enableKeyLimiter || Main.setting.allowedAsyncKeys.Contains(k.key))
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
            }
            ins.maximumUsedKeys = Math.Max(ins.maximumUsedKeys, downKeysDuration.Count);

            return num;
        }
    }
}
