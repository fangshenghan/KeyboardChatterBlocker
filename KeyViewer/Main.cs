using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection;
using UnityModManagerNet;
using UnityEngine;
using GDMiniJSON;
using KeyboardChatterBlocker.Languages;
using static UnityModManagerNet.UnityModManager;
using AdofaiTweaks.Tweaks.KeyLimiter;

namespace KeyboardChatterBlocker
{
    public static class Main
    {
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static UnityModManager.ModEntry modEntry;

        public static Harmony harmony;
        public static Setting setting;

        public static Dictionary<string, Language> langs = new Dictionary<string, Language>();

        public static bool usingTweaks = false;
        public static KeyLimiterSettings kls;

        public static void Setup(UnityModManager.ModEntry modEntry)
        {
            Main.modEntry = modEntry;
            Main.Logger = modEntry.Logger;
            Main.LoadLanguages();
            Main.setting = new Setting();
            Main.setting = UnityModManager.ModSettings.Load<Setting>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if(value)
            {
                harmony = new Harmony(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                ModEntry tweaks = UnityModManager.FindMod("AdofaiTweaks");
                if (tweaks != null && tweaks.Active)
                {
                    usingTweaks = true;
                }
                else
                {
                    usingTweaks = false;
                }

                if (usingTweaks)
                {
                    kls = Utils.GetKeyLimiterSettings();
                }
            } 
            else
            {
                harmony.UnpatchAll(modEntry.Info.Id);
            }
            return true;
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            /*foreach(MethodBase original in Harmony.GetAllPatchedMethods())
            { 
                if (original.HasMethodBody())
                {
                    Patches patchInfo = Harmony.GetPatchInfo(original);
                    foreach (HarmonyLib.Patch p in patchInfo.Prefixes)
                    {
                        if (p.PatchMethod.Module.Name.Contains("AdofaiTweaks"))
                        {
                            Main.Logger.Log(p.owner);
                        }
                    }
                }
            }*/

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
