using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection;
using UnityModManagerNet;
using UnityEngine;
using KeyboardChatterBlocker.Languages;
using System.Text.RegularExpressions;

namespace KeyboardChatterBlocker
{
    public static class Main
    {
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static UnityModManager.ModEntry modEntry;

        public static Harmony harmony;
        public static Setting setting;

        public static Dictionary<string, Language> langs = new Dictionary<string, Language>();

        public static void Setup(UnityModManager.ModEntry modEntry)
        {
            Main.modEntry = modEntry;
            Main.Logger = modEntry.Logger;
            Main.LoadLanguages();
            Main.setting = new Setting();
            Main.setting = UnityModManager.ModSettings.Load<Setting>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnUpdate = OnUpdate;
            modEntry.OnSaveGUI = OnSaveGUI;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if(value)
            {
                harmony = new Harmony(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                harmony.UnpatchAll(modEntry.Info.Id);
            }
            return true;
        }

        public static bool listeningKeysIgnore = false, listeningKeysLimiter = false;

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Language");
            GUIStyle gs = new GUIStyle(GUI.skin.button);
            foreach (string lang in langs.Keys)
            {
                if (setting.lang.Equals(lang)) gs.fontStyle = FontStyle.Bold;
                if (GUILayout.Button(lang, gs, GUILayout.Width(200))) setting.lang = lang;
                gs.fontStyle = FontStyle.Normal;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(langs[setting.lang].chatter_threshold_label);
            string s = GUILayout.TextField(setting.inputInterval + "", GUILayout.Width(150));
            try
            {
                if (!listeningKeysIgnore && !listeningKeysLimiter)
                {
                    setting.inputInterval = int.Parse(Regex.Replace(s, @"\D", ""));
                }
            }
            catch
            {
                setting.inputInterval = 100;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (AsyncInputManager.isActive)
            {
                GUILayout.Label(langs[setting.lang].ignored_async_keys + ": ");
                foreach (ushort k in setting.ignoredAsyncKeys)
                {
                    GUILayout.Label(k + "(" + (keyLabels.ContainsKey(k) ? keyLabels[k] : "Unknown") + ")");
                }
            }
            else
            {
                GUILayout.Label(langs[setting.lang].ignored_keys + ": ");
                foreach (KeyCode k in setting.ignoredKeys)
                {
                    GUILayout.Label(k.ToString());
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (listeningKeysIgnore)
            {
                if (GUILayout.Button(langs[setting.lang].listening_keys))
                {
                    listeningKeysIgnore = false;
                }
            }
            else
            {
                if (GUILayout.Button(langs[setting.lang].change_keys))
                {
                    listeningKeysIgnore = true;
                }
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

            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            setting.enableKeyLimiter = GUILayout.Toggle(setting.enableKeyLimiter, langs[setting.lang].enable_key_limiter);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (setting.enableKeyLimiter)
            {
                GUILayout.BeginHorizontal();
                if (AsyncInputManager.isActive)
                {
                    GUILayout.Label(langs[setting.lang].allowed_async_keys + ": ");
                    foreach (ushort k in setting.allowedAsyncKeys)
                    {
                        GUILayout.Label(k + "(" + (keyLabels.ContainsKey(k) ? keyLabels[k] : "Unknown") + ")");
                    }
                }
                else
                {
                    GUILayout.Label(langs[setting.lang].allowed_keys + ": ");
                    foreach (KeyCode k in setting.allowedKeys)
                    {
                        GUILayout.Label(k.ToString());
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (listeningKeysLimiter)
                {
                    if (GUILayout.Button(langs[setting.lang].listening_keys))
                    {
                        listeningKeysLimiter = false;
                    }
                }
                else
                {
                    if (GUILayout.Button(langs[setting.lang].change_keys))
                    {
                        listeningKeysLimiter = true;
                    }
                }
                if (GUILayout.Button(langs[setting.lang].clear_keys))
                {
                    if (AsyncInputManager.isActive)
                    {
                        setting.allowedAsyncKeys.Clear();
                    }
                    else
                    {
                        setting.allowedKeys.Clear();
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();
        }

        public static Dictionary<ushort, String> keyLabels = new Dictionary<ushort, String>();

        private static void OnUpdate(UnityModManager.ModEntry modEntry, float value)
        {
            if (Main.listeningKeysIgnore)
            {
                if (AsyncInputManager.isActive)
                {
                    foreach (AsyncKeyCode k in Utils.GetKeysDownThisFrame())
                    {
                        keyLabels[k.key] = k.label.ToString();
                        if (setting.ignoredAsyncKeys.Contains(k.key))
                        {
                            Main.setting.ignoredAsyncKeys.Remove(k.key);
                        }
                        else
                        {
                            Main.setting.ignoredAsyncKeys.Add(k.key);
                        }
                    }
                }
                else
                {
                    foreach (int i in Enum.GetValues(typeof(KeyCode)))
                    {
                        KeyCode keyCode = (KeyCode)i;
                        if (Input.GetKeyDown(keyCode) && !keyCode.ToString().Contains("Mouse"))
                        {
                            if (setting.ignoredKeys.Contains(keyCode))
                            {
                                setting.ignoredKeys.Remove(keyCode);
                            }
                            else
                            {
                                setting.ignoredKeys.Add(keyCode);
                            }
                        }
                    }
                }
            }else if (Main.listeningKeysLimiter)
            {
                if (AsyncInputManager.isActive)
                {
                    foreach (AsyncKeyCode k in Utils.GetKeysDownThisFrame())
                    {
                        keyLabels[k.key] = k.label.ToString();
                        if (setting.allowedAsyncKeys.Contains(k.key))
                        {
                            Main.setting.allowedAsyncKeys.Remove(k.key);
                        }
                        else
                        {
                            Main.setting.allowedAsyncKeys.Add(k.key);
                        }
                    }
                }
                else
                {
                    foreach (int i in Enum.GetValues(typeof(KeyCode)))
                    {
                        KeyCode keyCode = (KeyCode)i;
                        if (Input.GetKeyDown(keyCode) && !keyCode.ToString().Contains("Mouse"))
                        {
                            if (setting.allowedKeys.Contains(keyCode))
                            {
                                setting.allowedKeys.Remove(keyCode);
                            }
                            else
                            {
                                setting.allowedKeys.Add(keyCode);
                            }
                        }
                    }
                }
            }
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            setting.Save(modEntry);
        }

        private static void LoadLanguages()
        {
            langs.Add("한국어", new Korean());
            langs.Add("简体中文", new Chinese());
            langs.Add("English", new English());
        }
    }
}
