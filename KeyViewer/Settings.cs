using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityModManagerNet;
using UnityEngine;

namespace KeyboardChatterBlocker
{
    public class Setting : UnityModManager.ModSettings
    {
        public int inputInterval = 100;
        public string lang = "English";

        public bool enableKeyLimiter = false;

        public List<ushort> ignoredAsyncKeys = new List<ushort>();
        public List<KeyCode> ignoredKeys = new List<KeyCode>();

        public List<ushort> allowedAsyncKeys = new List<ushort>();
        public List<KeyCode> allowedKeys = new List<KeyCode>();

        public override void Save(UnityModManager.ModEntry modEntry) {
            var filepath = GetPath(modEntry);
            try {
                using (var writer = new StreamWriter(filepath)) {
                    var serializer = new XmlSerializer(GetType());
                    serializer.Serialize(writer, this);
                }
            } catch (Exception e) {
                modEntry.Logger.Error($"Can't save {filepath}.");
                modEntry.Logger.LogException(e);
            }
        }
        
        public override string GetPath(UnityModManager.ModEntry modEntry) {
            return Path.Combine(modEntry.Path, GetType().Name + ".xml");
        }

    }
}
