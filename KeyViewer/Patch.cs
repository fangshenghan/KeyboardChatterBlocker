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
                object value = anyKeyCode.value;
                if (value is KeyCode)
                {
                    KeyCode keyCode = (KeyCode)value;
                    if (!Main.setting.enableKeyLimiter || Main.setting.allowedKeys.Contains(keyCode) || (States)ins.stateMachine.GetState() != States.PlayerControl)
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

                        ins.keyFrequency[keyCode] = (ins.keyFrequency.ContainsKey(keyCode) ? (ins.keyFrequency[keyCode] + 1) : 0);
                        ins.keyTotal++;
                        if (Main.setting.ignoredKeys.Contains(keyCode))
                        {
                            num++;
                            continue;
                        }
                        if (!lastKeyPress.ContainsKey(keyCode))
                        {
                            lastKeyPress.Add(keyCode, 0L);
                        }
                        if (now - lastKeyPress[keyCode] > (long)Main.setting.inputInterval || now - lastKeyPress[keyCode] <= 2L)
                        {
                            num++;
                            lastKeyPress[keyCode] = now;
                        }
                        else
                        {
                            Main.Logger.Log("Blocked Key: " + keyCode + " time: " + (now - lastKeyPress[keyCode]) + "ms.");
                        }
                    }
                } 
                else if (value is AsyncKeyCode)
                {
                    AsyncKeyCode asyncKeyCode = (AsyncKeyCode)value;
                    if (!Main.setting.enableKeyLimiter || Main.setting.allowedAsyncKeys.Contains(asyncKeyCode.key) || (States)ins.stateMachine.GetState() != States.PlayerControl)
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

                        ins.keyFrequency[asyncKeyCode] = (ins.keyFrequency.ContainsKey(asyncKeyCode) ? (ins.keyFrequency[asyncKeyCode] + 1) : 0);
                        ins.keyTotal++;
                        if (Main.setting.ignoredAsyncKeys.Contains(asyncKeyCode.key))
                        {
                            num++;
                            continue;
                        }
                        if (!lastKeyPressAsync.ContainsKey(asyncKeyCode.key))
                        {
                            lastKeyPressAsync.Add(asyncKeyCode.key, 0L);
                        }
                        if (now - lastKeyPressAsync[asyncKeyCode.key] > (long)Main.setting.inputInterval || now - lastKeyPressAsync[asyncKeyCode.key] <= 2L)
                        {
                            num++;
                            lastKeyPressAsync[asyncKeyCode.key] = now;
                        }
                        else
                        {
                            Main.Logger.Log("Blocked Async Key: " + asyncKeyCode.label + " time: " + (now - lastKeyPressAsync[asyncKeyCode.key]) + "ms.");
                        }
                    }
                }
            }
            ins.maximumUsedKeys = Math.Max(ins.maximumUsedKeys, downKeysDuration.Count);

            return num;
        }
    }
}
