using System;
using System.Collections.Generic;
using UnityEngine;

namespace KeyboardChatterBlocker;

[Serializable]
public class KeyLimiterProfile
{
	public string name = "";

	public bool isSelected;

	public List<ushort> allowedAsyncKeys = new List<ushort>();

	public List<KeyCode> allowedKeys = new List<KeyCode>();

	public KeyLimiterProfile(string name)
	{
		name = name;
	}

	public KeyLimiterProfile()
	{
		name = "";
	}
}
