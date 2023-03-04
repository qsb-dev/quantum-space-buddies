using QSB.Utility;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace QSB.PlayerBodySetup.Remote;

public static class FontReplacer
{
	public static void ReplaceFonts(GameObject prefab)
	{
		var allFonts = Resources.LoadAll<Font>("fonts/english - latin");

		foreach (var monoBehaviour in prefab.GetComponentsInChildren<MonoBehaviour>(true))
		{
			if (monoBehaviour == null)
			{
				DebugLog.ToConsole($"Null monobehaviour found on {prefab.name}!", OWML.Common.MessageType.Warning);
				continue;
			}

			var publicFields = monoBehaviour
				.GetType()
				.GetFields(BindingFlags.Public | BindingFlags.Instance);
			var privateSerializedFields = monoBehaviour
				.GetType()
				.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(x => x.IsDefined(typeof(SerializeField)));

			foreach (var field in publicFields.Concat(privateSerializedFields))
			{
				if (field.FieldType != typeof(Font))
				{
					continue;
				}

				var existingFont = (Font)field.GetValue(monoBehaviour);
				if (existingFont == null)
				{
					continue;
				}

				var fontName = existingFont.name;
				var replacementFont = allFonts.FirstOrDefault(x => x.name == fontName);
				if (replacementFont == null)
				{
					DebugLog.ToConsole($"Warning - Couldn't find replacement font for {fontName}.", OWML.Common.MessageType.Warning);
					continue;
				}

				field.SetValue(monoBehaviour, replacementFont);
			}
		}
	}
}
