using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection;
using UnityModManagerNet;
using UnityEngine;
using KeyboardChatterBlocker.Languages;

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

        public static bool listeningKeys = false;

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
            GUILayout.Label(langs[setting.lang].chatter_threshold_label);
            string s = GUILayout.TextField(setting.inputInterval + "", GUILayout.Width(150));
            if (!s.Equals(""))
            {
                setting.inputInterval = int.Parse(s);
            }
            GUILayout.FlexibleSpace();
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
            if (listeningKeys)
            {
                if (GUILayout.Button(langs[setting.lang].listening_keys))
                {
                    listeningKeys = false;
                }
            }
            else
            {
                if (GUILayout.Button(langs[setting.lang].change_keys))
                {
                    listeningKeys = true;
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
        }

        public static Dictionary<ushort, String> keyLabels = new Dictionary<ushort, String>();

        private static void OnUpdate(UnityModManager.ModEntry modEntry, float value)
        {
            if (Main.listeningKeys)
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
