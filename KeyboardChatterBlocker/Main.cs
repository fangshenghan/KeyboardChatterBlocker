using HarmonyLib;
using KeyboardChatterBlocker.Languages;
using SkyHook;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using static UnityModManagerNet.UnityModManager;
using static UnityModManagerNet.UnityModManager.ModEntry;

namespace KeyboardChatterBlocker;

public static class Main
{
    public static string ModDirectoryPath;

    public static ModLogger Logger;

	public static ModEntry ModEntry;

	public static Harmony harmony;

	public static Setting setting;

	public static Dictionary<string, Language> langs = new Dictionary<string, Language>();

	public static bool isListeningKeysIgnore = false;

	public static bool isListeningKeysLimiter = false;

	public static KeyLimiterProfile listeningKeyLimiterProfile = new KeyLimiterProfile("");

	public static KeyLimiterProfile selectedKeyLimiterProfile = new KeyLimiterProfile("");

	public static Dictionary<ushort, string> keyLabels = new Dictionary<ushort, string>();

	public static void Setup(ModEntry modEntry)
	{
		LoadLanguages();
		InitializeKeyLabelDict();
		ModEntry = modEntry;
		Logger = modEntry.Logger;
        ModDirectoryPath = string.IsNullOrEmpty(modEntry.Path)
                ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                : modEntry.Path;
		setting = Setting.Load();
		modEntry.OnToggle = OnToggle;
		modEntry.OnGUI = OnGUI;
		modEntry.OnUpdate = OnUpdate;
		modEntry.OnSaveGUI = OnSaveGUI;
	}

	private static bool OnToggle(ModEntry modEntry, bool value)
	{
		if (value)
		{
			harmony = new Harmony(modEntry.Info.Id);
			harmony.PatchAll(Assembly.GetExecutingAssembly());
			UpdateSelectedKeyLimiterProfile();
		}
		else
		{
			harmony.UnpatchAll(modEntry.Info.Id);
		}
		return true;
	}

    private static void OnGUI(ModEntry modEntry)
    {
        if ((isListeningKeysIgnore || isListeningKeysLimiter) && GUIUtility.keyboardControl != 0)
        {
            GUIUtility.keyboardControl = 0;
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label("Language");
        GUIStyle gUIStyle = new GUIStyle(GUI.skin.button);
        foreach (string key in langs.Keys)
        {
            if (setting.lang.Equals(key))
            {
                gUIStyle.fontStyle = FontStyle.Bold;
            }
            if (GUILayout.Button(key, gUIStyle, GUILayout.Width(200f)))
            {
                setting.lang = key;
            }
            gUIStyle.fontStyle = FontStyle.Normal;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label(langs[setting.lang].chatter_threshold_label);
        string input = GUILayout.TextField(setting.inputInterval.ToString() ?? "", GUILayout.Width(150f));
        try
        {
            if (!isListeningKeysIgnore && !isListeningKeysLimiter)
            {
                setting.inputInterval = int.Parse(Regex.Replace(input, "\\D", ""));
            }
        }
        catch
        {
            setting.inputInterval = 100;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(10f);
        GUILayout.BeginHorizontal();
        if (AsyncInputManager.isActive)
        {
            GUILayout.Label(langs[setting.lang].ignored_async_keys + ": ");
            foreach (ushort ignoredAsyncKey in setting.ignoredAsyncKeys)
            {
                GUILayout.Label(ignoredAsyncKey + "(" + (keyLabels.ContainsKey(ignoredAsyncKey) ? keyLabels[ignoredAsyncKey] : "Unknown") + ")");
            }
        }
        else
        {
            GUILayout.Label(langs[setting.lang].ignored_keys + ": ");
            foreach (KeyCode ignoredKey in setting.ignoredKeys)
            {
                GUILayout.Label(ignoredKey.ToString());
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (isListeningKeysIgnore)
        {
            if (GUILayout.Button(langs[setting.lang].listening_keys))
            {
                isListeningKeysIgnore = false;
            }
        }
        else if (GUILayout.Button(langs[setting.lang].change_keys))
        {
            isListeningKeysIgnore = true;
        }
        if (GUILayout.Button(langs[setting.lang].clear_keys))
        {
            if (AsyncInputManager.isActive)
            {
                setting.ignoredAsyncKeys.Clear();
            }
            else
            {
                setting.ignoredKeys.Clear();
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(10f);
        GUILayout.BeginHorizontal();
        setting.enableKeyLimiter = GUILayout.Toggle(setting.enableKeyLimiter, langs[setting.lang].enable_key_limiter);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if (setting.enableKeyLimiter)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(langs[setting.lang].create_key_limiter_profile))
            {
                setting.keyLimiterProfiles.Add(new KeyLimiterProfile("Profile " + (setting.keyLimiterProfiles.Count + 1)));
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            List<KeyLimiterProfile> list = new List<KeyLimiterProfile>(setting.keyLimiterProfiles);
            foreach (KeyLimiterProfile item in list)
            {
                GUILayout.BeginHorizontal();
                item.isSelected = GUILayout.Toggle(item.isSelected, item.name);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                if (!item.isSelected)
                {
                    continue;
                }
                foreach (KeyLimiterProfile item2 in list)
                {
                    if (!item2.Equals(item))
                    {
                        item2.isSelected = false;
                    }
                }
                GUILayout.BeginHorizontal();
                GUILayout.Space(20f);
                GUILayout.Label(langs[setting.lang].key_limiter_profile_name);
                item.name = GUILayout.TextField(item.name);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(20f);
                if (AsyncInputManager.isActive)
                {
                    GUILayout.Label(langs[setting.lang].allowed_async_keys + ": ");
                    string text = "";
                    foreach (ushort allowedAsyncKey in item.allowedAsyncKeys)
                    {
                        text = text + allowedAsyncKey + "(" + (keyLabels.ContainsKey(allowedAsyncKey) ? keyLabels[allowedAsyncKey] : "Unknown") + ")  ";
                    }
                    GUILayout.Label(text);
                }
                else
                {
                    GUILayout.Label(langs[setting.lang].allowed_keys + ": ");
                    string text2 = "";
                    foreach (KeyCode allowedKey in item.allowedKeys)
                    {
                        text2 = text2 + allowedKey.ToString() + "  ";
                    }
                    GUILayout.Label(text2);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(20f);
                if (isListeningKeysLimiter)
                {
                    if (GUILayout.Button(langs[setting.lang].listening_keys))
                    {
                        isListeningKeysLimiter = false;
                    }
                }
                else if (GUILayout.Button(langs[setting.lang].change_keys))
                {
                    isListeningKeysLimiter = true;
                    listeningKeyLimiterProfile = item;
                }
                if (GUILayout.Button(langs[setting.lang].clear_keys))
                {
                    if (AsyncInputManager.isActive)
                    {
                        item.allowedAsyncKeys.Clear();
                    }
                    else
                    {
                        item.allowedKeys.Clear();
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(20f);
                if (GUILayout.Button(langs[setting.lang].delete_key_limiter_profile))
                {
                    setting.keyLimiterProfiles.Remove(item);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }
        GUILayout.BeginHorizontal();
        GUILayout.EndHorizontal();
    }

    private unsafe static void OnUpdate(ModEntry modEntry, float value)
    {
        // Moved Input Tracking into Patching
        if (!AsyncInputManager.isActive && (isListeningKeysIgnore || isListeningKeysLimiter))
        {
            NativeInputListener.StartListening();
        }
        else
        {
            NativeInputListener.StopListening();
        }

        while (NativeInputListener.CapturedKeys.TryDequeue(out KeyCode clickedKey))
        {
            if (isListeningKeysIgnore)
            {
                if (setting.ignoredKeys.Contains(clickedKey))
                    setting.ignoredKeys.Remove(clickedKey);
                else
                    setting.ignoredKeys.Add(clickedKey);
            }
            else if (isListeningKeysLimiter && listeningKeyLimiterProfile != null)
            {
                if (listeningKeyLimiterProfile.allowedKeys.Contains(clickedKey))
                    listeningKeyLimiterProfile.allowedKeys.Remove(clickedKey);
                else
                    listeningKeyLimiterProfile.allowedKeys.Add(clickedKey);
            }
        }
        UpdateSelectedKeyLimiterProfile();
    }

    private static void OnSaveGUI(ModEntry modEntry)
	{
		setting.Save();
	}

	private static void LoadLanguages()
	{
		langs.Add("한국어", new Korean());
		langs.Add("简体中文", new Chinese());
		langs.Add("English", new English());
		langs.Add("Tiếng Việt", new Vietnamese());
	}

	private static void InitializeKeyLabelDict()
	{
		foreach (KeyValuePair<ushort, KeyCode> item in Utils.SkyUnityKeyCodeDictionary)
		{
			keyLabels[item.Key] = SkyHookKeyMapper.UnityKeyToSkyHookKey(item.Value).ToString();
		}
	}

	public static void UpdateSelectedKeyLimiterProfile()
	{
		foreach (KeyLimiterProfile keyLimiterProfile in setting.keyLimiterProfiles)
		{
			if (keyLimiterProfile.isSelected)
			{
				selectedKeyLimiterProfile = keyLimiterProfile;
				break;
			}
		}
	}
}
