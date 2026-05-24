using System;
using System.Collections.Generic;
using HarmonyLib;
using SkyHook;
using UnityEngine;
using static UnityModManagerNet.UnityModManager.ModEntry;

namespace KeyboardChatterBlocker;

public static class Patch
{
	private static scrPlayer PlayerOne
	{
		get
		{
			return CachedInstance.playerOne;
		}
	}

    private static readonly WeakReference<scrController> _cachedInstanceRef = new WeakReference<scrController>(null);
    private static scrController CachedInstance
    {
        get
        {
            if (_cachedInstanceRef.TryGetTarget(out var target) && target != null)
            {
                return target;
            }
            return null;
        }
        set
        {
            _cachedInstanceRef.SetTarget(value);
        }
    }

    [HarmonyPatch(typeof(scrPlayer), "CountValidKeysPressed")]
	public static class scrController_CountValidKeysPressed_Patch
	{
		private static int validKeys;

		public static void Prefix()
		{
			validKeys = CountValidKeysPressed();
		}

		public static void Postfix(ref int __result)
		{
			__result = validKeys;
		}
	}

	[HarmonyPatch(typeof(SkyHookManager), "HookCallback")]
	public static class SkyHookManager_HookCallback_Patch
	{
		public static bool Prefix(SkyHookEvent ev)
		{
			if (CachedInstance is null)
			{
				return true;
			}
			if (ev.Type == SkyHook.EventType.KeyReleased || ev.Key == 27)
			{
				return true;
			}
			if (Main.setting.enableKeyLimiter && !Main.selectedKeyLimiterProfile.allowedAsyncKeys.Contains(ev.Key) && !CachedInstance.paused && CachedInstance.gameworld)
			{
				Enum state = CachedInstance.stateMachine.GetState();
				if (state is States && (States)(object)state == States.PlayerControl)
				{
					return false;
				}
			}
			if (Main.setting.ignoredAsyncKeys.Contains(ev.Key))
			{
				return true;
			}
			if (!lastKeyPressAsync.ContainsKey(ev.Key))
			{
				lastKeyPressAsync.Add(ev.Key, 0L);
			}
			long num = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			long num2 = num - lastKeyPressAsync[ev.Key];
			if (num2 > Main.setting.inputInterval)
			{
				lastKeyPressAsync[ev.Key] = num;
				return true;
			}
			ModLogger logger = Main.Logger;
			string[] obj = ["Blocked Async Key: ", null, null, null, null];
			KeyLabel label = ev.Label;
			obj[1] = label.ToString();
			obj[2] = " time: ";
			obj[3] = num2.ToString();
			obj[4] = "ms.";
			logger.Log(string.Concat(obj));
			return false;
		}
	}

    [HarmonyPatch(typeof(scrController), "Awake")]
    private static class scrController_Awake_Patch
    {
        public static void Postfix(scrController __instance)
        {
            CachedInstance = __instance;
        }
    }

    public static Dictionary<ushort, long> lastKeyPressAsync = new Dictionary<ushort, long>();

	public static Dictionary<KeyCode, long> lastKeyPress = new Dictionary<KeyCode, long>();

	public static Dictionary<ushort, long> countedAsync = new Dictionary<ushort, long>();

	public static Dictionary<KeyCode, long> counted = new Dictionary<KeyCode, long>();

	private static int CountValidKeysPressed()
	{
		if (!CachedInstance) return 0;
		int num = 0;
		long num2 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		PlayerOne.keyLimiterOverCounter = 0;
		foreach (AnyKeyCode mainPressKey in RDInput.GetMainPressKeys())
		{
			object value = mainPressKey.value;
			if (value is KeyCode keyCode)
			{
				if (Main.setting.enableKeyLimiter && !Main.selectedKeyLimiterProfile.allowedKeys.Contains(keyCode) && !CachedInstance.paused && CachedInstance.gameworld)
				{
					Enum state = CachedInstance.stateMachine.GetState();
					if (state is States && (States)(object)state == States.PlayerControl)
					{
						continue;
					}
				}
                PlayerOne.keyFrequency[keyCode] = PlayerOne.keyFrequency.ContainsKey(keyCode) ? (PlayerOne.keyFrequency[keyCode] + 1) : 0;
                PlayerOne.keyTotal++;
				if (Main.setting.ignoredKeys.Contains(keyCode))
				{
					num++;
					continue;
				}
				if (!lastKeyPress.ContainsKey(keyCode))
				{
					lastKeyPress.Add(keyCode, 0L);
				}
				if (num2 - lastKeyPress[keyCode] > Main.setting.inputInterval || num2 - lastKeyPress[keyCode] <= 5)
				{
					num++;
					lastKeyPress[keyCode] = num2;
					continue;
				}
				Main.Logger.Log("Blocked Key: " + keyCode.ToString() + " time: " + (num2 - lastKeyPress[keyCode]) + "ms.");
			}
			else if (value is AsyncKeyCode asyncKeyCode)
			{
				PlayerOne.keyFrequency[asyncKeyCode] = PlayerOne.keyFrequency.ContainsKey(asyncKeyCode) ? (PlayerOne.keyFrequency[asyncKeyCode] + 1) : 0;
				PlayerOne.keyTotal++;
				num++;
			}
		}
		return num;
	}
}
