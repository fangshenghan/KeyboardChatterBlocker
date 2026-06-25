using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Xml.Serialization;

namespace KeyboardChatterBlocker;

public class Setting
{
	public int inputInterval = 100;

	public string lang = "English";

	public List<ushort> ignoredAsyncKeys = new List<ushort>();

	public List<KeyCode> ignoredKeys = new List<KeyCode>();

	public bool enableKeyLimiter;

	public List<KeyLimiterProfile> keyLimiterProfiles = new List<KeyLimiterProfile>();

    public void Save()
    {
        var filepath = Path.Combine(Main.ModDirectoryPath, typeof(Setting).Name + ".json");
        try
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };
            var json = JsonConvert.SerializeObject(this, settings);
            File.WriteAllText(filepath, json);
        }
        catch (Exception e)
        {
            Main.Logger.Error("Can't save " + Main.ModDirectoryPath + ".");
            Main.Logger.LogException(e);
        }
    }

    public static Setting Load()
    {
        var jsonPath = Path.Combine(Main.ModDirectoryPath, typeof(Setting).Name + ".json");
        var xmlPath = Path.Combine(Main.ModDirectoryPath, typeof(Setting).Name + ".xml");

        if (File.Exists(jsonPath))
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                };

                var json = File.ReadAllText(jsonPath);
                return JsonConvert.DeserializeObject<Setting>(json, settings) ?? new Setting();
            }
            catch (Exception e)
            {
                Main.Logger.Error("Failed to load JSON settings: " + e);
                return new Setting();
            }
        }

        if (File.Exists(xmlPath))
        {
            try
            {
                Main.Logger.Log("Found old XML settings. Migrating to JSON...");

                Setting migratedSettings;
                using (StreamReader textReader = new StreamReader(xmlPath))
                {
                    var serializer = new XmlSerializer(typeof(Setting));
                    migratedSettings = (Setting)serializer.Deserialize(textReader);
                }

                migratedSettings.Save();
                try
                {
                    File.Delete(xmlPath);
                }
                catch (Exception ex)
                {
                    Main.Logger.Error("Could not delete old XML file: " + ex.Message);
                }

                return migratedSettings;
            }
            catch (Exception e)
            {
                Main.Logger.Error("Failed to migrate old XML settings: " + e);
                return new Setting();
            }
        }
        return new Setting();
    }
}
