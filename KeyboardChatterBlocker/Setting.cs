using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;

namespace KeyboardChatterBlocker;

public class Setting : ModSettings
{
	public int inputInterval = 100;

	public string lang = "English";

	public List<ushort> ignoredAsyncKeys = new List<ushort>();

	public List<KeyCode> ignoredKeys = new List<KeyCode>();

	public bool enableKeyLimiter;

	public List<KeyLimiterProfile> keyLimiterProfiles = new List<KeyLimiterProfile>();

	public override void Save(ModEntry modEntry)
	{
		string path = GetPath(modEntry);
		try
		{
			using StreamWriter textWriter = new StreamWriter(path);
			new XmlSerializer(GetType()).Serialize(textWriter, this);
		}
		catch (Exception ex)
		{
			modEntry.Logger.Error("Can't save " + path + ".");
			modEntry.Logger.LogException(ex);
		}
	}

	public override string GetPath(ModEntry modEntry)
	{
		return Path.Combine(modEntry.Path, GetType().Name + ".xml");
	}
}
