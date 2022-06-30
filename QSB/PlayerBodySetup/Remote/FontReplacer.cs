using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.PlayerBodySetup.Remote;

public static class FontReplacer
{
	public static void ReplaceFonts(GameObject prefab)
	{
		var everyBehaviour = prefab.GetComponentsInChildren<MonoBehaviour>();

		var allFonts = Resources.LoadAll("fonts/english - latin", typeof(Font));

		foreach (var item in everyBehaviour)
		{
			var publicFields = item.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

			var privateSerializedFields = item
				.GetType()
				.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(x => x.IsDefined(typeof(SerializeField), false))
				.ToArray();

			// concatenate the two arrays
			var final = new FieldInfo[publicFields.Length + privateSerializedFields.Length];
			publicFields.CopyTo(final, 0);
			privateSerializedFields.CopyTo(final, publicFields.Length);

			foreach (var field in final)
			{
				if (field.FieldType == typeof(Font))
				{
					var existingFont = (Font)field.GetValue(item);

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

					field.SetValue(item, replacementFont);
				}
			}
		}
	}
}
