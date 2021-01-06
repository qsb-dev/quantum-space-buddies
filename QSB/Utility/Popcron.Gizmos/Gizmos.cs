using System;
using UnityEngine;

namespace Popcron
{
	public class Gizmos
	{
		private static string _prefsKey = null;
		private static int? _bufferSize = null;
		private static bool? _enabled = null;
		private static float? _dashGap = null;
		private static bool? _cull = null;
		private static int? _pass = null;
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

		/// <summary>
		/// The size of the total gizmos buffer.
		/// Default is 4096.
		/// </summary>
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

		/// <summary>
		/// The size of the gap when drawing dashed elements.
		/// Default gap size is 0.1
		/// </summary>
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

		/// <summary>
		/// The material being used to render.
		/// </summary>
		public static Material Material
		{
			get => GizmosInstance.Material;
			set => GizmosInstance.Material = value;
		}

		/// <summary>
		/// Global offset for all points. Default is (0, 0, 0).
		/// </summary>
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

		/// <summary>
		/// Draws an element onto the screen.
		/// </summary>
		public static void Draw<T>(Color? color, bool dashed, params object[] args) where T : Drawer
		{
			var drawer = Drawer.Get<T>();
			if (drawer != null)
			{
				var points = drawer.Draw(ref buffer, args);

				//copy from buffer and add to the queue
				var array = new Vector3[points];
				Array.Copy(buffer, array, points);
				GizmosInstance.Submit(array, color, dashed);
			}
		}

		/// <summary>
		/// Draws an array of lines. Useful for things like paths.
		/// </summary>
		public static void Lines(Vector3[] lines, Color? color = null, bool dashed = false)
		{
			GizmosInstance.Submit(lines, color, dashed);
		}

		/// <summary>
		/// Draw line in world space.
		/// </summary>
		public static void Line(Vector3 a, Vector3 b, Color? color = null, bool dashed = false) => Draw<LineDrawer>(color, dashed, a, b);

		/// <summary>
		/// Draw square in world space.
		/// </summary>
		public static void Square(Vector2 position, Vector2 size, Color? color = null, bool dashed = false) => Square(position, Quaternion.identity, size, color, dashed);

		/// <summary>
		/// Draw square in world space with float diameter parameter.
		/// </summary>
		public static void Square(Vector2 position, float diameter, Color? color = null, bool dashed = false) => Square(position, Quaternion.identity, Vector2.one * diameter, color, dashed);

		/// <summary>
		/// Draw square in world space with a rotation parameter.
		/// </summary>
		public static void Square(Vector2 position, Quaternion rotation, Vector2 size, Color? color = null, bool dashed = false) => Draw<SquareDrawer>(color, dashed, position, rotation, size);

		/// <summary>
		/// Draws a cube in world space.
		/// </summary>
		public static void Cube(Vector3 position, Quaternion rotation, Vector3 size, Color? color = null, bool dashed = false) => Draw<CubeDrawer>(color, dashed, position, rotation, size);

		/// <summary>
		/// Draws a rectangle in screen space.
		/// </summary>
		public static void Rect(Rect rect, Camera camera, Color? color = null, bool dashed = false)
		{
			rect.y = Screen.height - rect.y;
			Vector2 corner = camera.ScreenToWorldPoint(new Vector2(rect.x, rect.y - rect.height));
			Draw<SquareDrawer>(color, dashed, corner + rect.size * 0.5f, Quaternion.identity, rect.size);
		}

		/// <summary>
		/// Draws a representation of a bounding box.
		/// </summary>
		public static void Bounds(Bounds bounds, Color? color = null, bool dashed = false) => Draw<CubeDrawer>(color, dashed, bounds.center, Quaternion.identity, bounds.size);

		/// <summary>
		/// Draws a cone similar to the one that spot lights draw.
		/// </summary>
		public static void Cone(Vector3 position, Quaternion rotation, float length, float angle, Color? color = null, bool dashed = false, int pointsCount = 16)
		{
			//draw the end of the cone
			var endAngle = Mathf.Tan(angle * 0.5f * Mathf.Deg2Rad) * length;
			var forward = rotation * Vector3.forward;
			var endPosition = position + forward * length;
			var offset = 0f;
			Draw<PolygonDrawer>(color, dashed, endPosition, pointsCount, endAngle, offset, rotation);

			//draw the 4 lines
			for (var i = 0; i < 4; i++)
			{
				var a = i * 90f * Mathf.Deg2Rad;
				var point = rotation * new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * endAngle;
				Line(position, position + point + forward * length, color, dashed);
			}
		}

		/// <summary>
		/// Draws a sphere at position with specified radius.
		/// </summary>
		public static void Sphere(Vector3 position, float radius, Color? color = null, bool dashed = false, int pointsCount = 16)
		{
			var offset = 0f;
			Draw<PolygonDrawer>(color, dashed, position, pointsCount, radius, offset, Quaternion.Euler(0f, 0f, 0f));
			Draw<PolygonDrawer>(color, dashed, position, pointsCount, radius, offset, Quaternion.Euler(90f, 0f, 0f));
			Draw<PolygonDrawer>(color, dashed, position, pointsCount, radius, offset, Quaternion.Euler(0f, 90f, 90f));
		}

		/// <summary>
		/// Draws a circle in world space and billboards towards the camera.
		/// </summary>
		public static void Circle(Vector3 position, float radius, Camera camera, Color? color = null, bool dashed = false, int pointsCount = 16)
		{
			var offset = 0f;
			var rotation = Quaternion.LookRotation(position - camera.transform.position);
			Draw<PolygonDrawer>(color, dashed, position, pointsCount, radius, offset, rotation);
		}

		/// <summary>
		/// Draws a circle in world space with a specified rotation.
		/// </summary>
		public static void Circle(Vector3 position, float radius, Quaternion rotation, Color? color = null, bool dashed = false, int pointsCount = 16)
		{
			var offset = 0f;
			Draw<PolygonDrawer>(color, dashed, position, pointsCount, radius, offset, rotation);
		}

		public static void Frustum(OWCamera camera, Color? color = null, bool dashed = false) => Draw<FrustumDrawer>(color, dashed, camera);
	}
}
