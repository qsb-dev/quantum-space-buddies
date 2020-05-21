using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Marshmallow
{
	class PlanetConfig
	{
		[JsonProperty("settings")]
		public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();

		public T GetSettingsValue<T>(string key)
		{
			bool flag = !this.Settings.ContainsKey(key);
			T result;
			if (flag)
			{
				QSB.DebugLog.Console("Error: setting not found: " + key);
				result = default(T);
			}
			else
			{
				object obj = this.Settings[key];
				try
				{
					JObject jobject;
					object value = ((jobject = (obj as JObject)) != null) ? jobject["value"] : obj;

					if (obj.GetType() == typeof(JArray))
					{
						if (typeof(T) == typeof(Vector2))
						{
							JArray array = (JArray)obj;
							float[] items = array.Select(jv => (float)jv).ToArray();
							Vector2 output = new Vector2(items[0], items[1]);

							return (T)Convert.ChangeType(output, typeof(T));
						}

						if (typeof(T) == typeof(Vector3))
						{
							JArray array = (JArray)obj;
							float[] items = array.Select(jv => (float)jv).ToArray();
							Vector3 output = new Vector3(items[0], items[1], items[2]);

							return (T)Convert.ChangeType(output, typeof(T));
						}

						if (typeof(T) == typeof(Vector4) || typeof(T) == typeof(Color))
						{
							JArray array = (JArray)obj;
							float[] items = array.Select(jv => (float)jv).ToArray();
							Vector4 output = new Vector4(items[0], items[1], items[2], items[3]);

							return (T)Convert.ChangeType(output, typeof(T));
						}

						if (typeof(T) == typeof(Color32))
						{
							JArray array = (JArray)obj;
							byte[] items = array.Select(jv => (byte)jv).ToArray();
							Color32 output = new Color32(items[0], items[1], items[2], items[3]);

							return (T)Convert.ChangeType(output, typeof(T));
						}
					}

					result = (T)Convert.ChangeType(value, typeof(T));
				}
				catch (InvalidCastException)
				{
					QSB.DebugLog.Console(string.Format("Error when converting setting {0} of type {1} to type {2}", key, obj.GetType(), typeof(T)));
					result = default(T);
				}
			}
			return result;
		}

	}
}
