using System;
using UnityEngine;

namespace Popcron
{
	public class Gizmos
	{
		private static string _prefsKey = null;
		private static int? _bufferSize = null;
		private static float? _dashGap = null;
		private static Vector3? _offset = null;

		private static Vector3[] buffer = new Vector3[BufferSize];

		private static string PrefsKey
		{
			get
			{
				if (string.IsNullOrEmpty(_prefsKey))
				{
					_prefsKey = $"{SystemInfo.deviceUniqueIdentifier}.{Application.companyName}.{Application.productName}.{Constants.UniqueIdentifier}";
				}

				return _prefsKey;
			}
		}

		public static int BufferSize
		{
			get
			{
				if (_bufferSize == null)
				{
					_bufferSize = PlayerPrefs.GetInt($"{PrefsKey}.BufferSize", 4096);
				}

				return _bufferSize.Value;
			}
			set
			{
				value = Mathf.Clamp(value, 0, int.MaxValue);
				if (_bufferSize != value)
				{
					_bufferSize = value;
					PlayerPrefs.SetInt($"{PrefsKey}.BufferSize", value);

					//buffer size changed, so recreate the buffer array too
					buffer = new Vector3[value];
				}
			}
		}

		public static float DashGap
		{
			get
			{
				if (_dashGap == null)
				{
					_dashGap = PlayerPrefs.GetFloat($"{PrefsKey}.DashGap", 0.1f);
				}

				return _dashGap.Value;
			}
			set
			{
				if (_dashGap != value)
				{
					_dashGap = value;
					PlayerPrefs.SetFloat($"{PrefsKey}.DashGap", value);
				}
			}
		}

		public static Material Material
		{
			get => GizmosInstance.Material;
			set => GizmosInstance.Material = value;
		}

		public static Vector3 Offset
		{
			get
			{
				const string Delim = ",";
				if (_offset == null)
				{
					var data = PlayerPrefs.GetString($"{PrefsKey}.Offset", 0 + Delim + 0 + Delim + 0);
					var indexOf = data.IndexOf(Delim);
					var lastIndexOf = data.LastIndexOf(Delim);
					if (indexOf + lastIndexOf > 0)
					{
						var arr = data.Split(Delim[0]);
						_offset = new Vector3(float.Parse(arr[0]), float.Parse(arr[1]), float.Parse(arr[2]));
					}
					else
					{
						return Vector3.zero;
					}
				}

				return _offset.Value;
			}
			set
			{
				const string Delim = ",";
				if (_offset != value)
				{
					_offset = value;
					PlayerPrefs.SetString($"{PrefsKey}.Offset", value.x + Delim + value.y + Delim + value.y);
				}
			}
		}

		public static void Draw<T>(Color? color, params object[] args) where T : Drawer
		{
			var drawer = Drawer.Get<T>();
			if (drawer != null)
			{
				var points = drawer.Draw(ref buffer, args);

				//copy from buffer and add to the queue
				var array = new Vector3[points];
				Array.Copy(buffer, array, points);
				GizmosInstance.Submit(array, color);
			}
		}

		public static void Lines(Vector3[] lines, Color? color = null)
			=> GizmosInstance.Submit(lines, color);

		public static void Line(Vector3 a, Vector3 b, Color? color = null)
			=> Draw<LineDrawer>(color, a, b);

		public static void Square(Vector2 position, Vector2 size, Color? color = null)
			=> Square(position, Quaternion.identity, size, color);

		public static void Square(Vector2 position, float diameter, Color? color = null)
			=> Square(position, Quaternion.identity, Vector2.one * diameter, color);

		public static void Square(Vector2 position, Quaternion rotation, Vector2 size, Color? color = null)
			=> Draw<SquareDrawer>(color, position, rotation, size);

		public static void Cube(Vector3 position, Quaternion rotation, Vector3 size, Color? color = null)
			=> Draw<CubeDrawer>(color, position, rotation, size);

		public static void Rect(Rect rect, Camera camera, Color? color = null)
		{
			rect.y = Screen.height - rect.y;
			Vector2 corner = camera.ScreenToWorldPoint(new Vector2(rect.x, rect.y - rect.height));
			Draw<SquareDrawer>(color, corner + (rect.size * 0.5f), Quaternion.identity, rect.size);
		}

		public static void Bounds(Bounds bounds, Color? color = null)
			=> Draw<CubeDrawer>(color, bounds.center, Quaternion.identity, bounds.size);

		public static void Cone(Vector3 position, Quaternion rotation, float length, float angle, Color? color = null, int pointsCount = 16)
		{
			//draw the end of the cone
			var endAngle = Mathf.Tan(angle * 0.5f * Mathf.Deg2Rad) * length;
			var forward = rotation * Vector3.forward;
			var endPosition = position + (forward * length);
			var offset = 0f;
			Draw<PolygonDrawer>(color, endPosition, pointsCount, endAngle, offset, rotation);

			//draw the 4 lines
			for (var i = 0; i < 4; i++)
			{
				var a = i * 90f * Mathf.Deg2Rad;
				var point = rotation * new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * endAngle;
				Line(position, position + point + (forward * length), color);
			}
		}

		public static void Sphere(Vector3 position, float radius, Color? color = null, int pointsCount = 16)
		{
			var offset = 0f;
			Draw<PolygonDrawer>(color, position, pointsCount, radius, offset, Quaternion.Euler(0f, 0f, 0f));
			Draw<PolygonDrawer>(color, position, pointsCount, radius, offset, Quaternion.Euler(90f, 0f, 0f));
			Draw<PolygonDrawer>(color, position, pointsCount, radius, offset, Quaternion.Euler(0f, 90f, 90f));
		}

		public static void Circle(Vector3 position, float radius, Camera camera, Color? color = null, int pointsCount = 16)
		{
			var offset = 0f;
			var rotation = Quaternion.LookRotation(position - camera.transform.position);
			Draw<PolygonDrawer>(color, position, pointsCount, radius, offset, rotation);
		}

		public static void Circle(Vector3 position, float radius, Quaternion rotation, Color? color = null, int pointsCount = 16)
		{
			var offset = 0f;
			Draw<PolygonDrawer>(color, position, pointsCount, radius, offset, rotation);
		}

		public static void Frustum(OWCamera camera, Color? color = null)
			=> Draw<FrustumDrawer>(color, camera);
	}
}