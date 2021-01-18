using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Popcron
{
	public class GizmosInstance : MonoBehaviour
	{
		private const int DefaultQueueSize = 4096;

		private static GizmosInstance instance;
		private static Material defaultMaterial;

		private Material overrideMaterial;
		private int queueIndex = 0;
		private int lastFrame;
		private Element[] queue = new Element[DefaultQueueSize];

		/// <summary>
		/// The material being used to render
		/// </summary>
		public static Material Material
		{
			get
			{
				var inst = GetOrCreate();
				if (inst.overrideMaterial)
				{
					return inst.overrideMaterial;
				}

				return DefaultMaterial;
			}
			set
			{
				var inst = GetOrCreate();
				inst.overrideMaterial = value;
			}
		}

		/// <summary>
		/// The default line renderer material
		/// </summary>
		public static Material DefaultMaterial
		{
			get
			{
				if (!defaultMaterial)
				{
					// Unity has a built-in shader that is useful for drawing
					// simple colored things.
					var shader = Shader.Find("UI/Default");
					defaultMaterial = new Material(shader)
					{
						hideFlags = HideFlags.HideAndDontSave
					};

					// Turn on alpha blending
					defaultMaterial.SetInt("unity_GUIZTestMode", (int)CompareFunction.Always);
				}

				return defaultMaterial;
			}
		}

		internal static GizmosInstance GetOrCreate()
		{
			if (!instance)
			{
				var gizmosInstances = FindObjectsOfType<GizmosInstance>();
				for (var i = 0; i < gizmosInstances.Length; i++)
				{
					instance = gizmosInstances[i];

					//destroy any extra gizmo instances
					if (i > 0)
					{
						Destroy(gizmosInstances[i]);
					}
				}

				//none were found, create a new one
				if (!instance)
				{
					instance = new GameObject(typeof(GizmosInstance).FullName).AddComponent<GizmosInstance>();
					instance.gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
				}
			}

			return instance;
		}

		private float CurrentTime => Time.time;

		/// <summary>
		/// Submits an array of points to draw into the queue.
		/// </summary>
		internal static void Submit(Vector3[] points, Color? color, bool dashed)
		{
			var inst = GetOrCreate();

			//if new frame, reset index
			if (inst.lastFrame != Time.frameCount)
			{
				inst.lastFrame = Time.frameCount;
				inst.queueIndex = 0;
			}

			//excedeed the length, so make it even bigger
			if (inst.queueIndex >= inst.queue.Length)
			{
				var bigger = new Element[inst.queue.Length + DefaultQueueSize];
				for (var i = inst.queue.Length; i < bigger.Length; i++)
				{
					bigger[i] = new Element();
				}

				Array.Copy(inst.queue, 0, bigger, 0, inst.queue.Length);
				inst.queue = bigger;
			}

			inst.queue[inst.queueIndex].color = color ?? Color.white;
			inst.queue[inst.queueIndex].points = points;
			inst.queue[inst.queueIndex].dashed = dashed;

			inst.queueIndex++;
		}

		private void OnEnable()
		{
			//populate queue with empty elements
			queue = new Element[DefaultQueueSize];
			for (var i = 0; i < DefaultQueueSize; i++)
			{
				queue[i] = new Element();
			}

			Camera.onPostRender += OnRendered;
		}

		private void OnDisable() => Camera.onPostRender -= OnRendered;

		private void Update()
		{
			//always render something
			Gizmos.Line(default, default);
		}

		private void OnRendered(Camera camera)
		{
			Material.SetPass(0);

			var offset = Gizmos.Offset;

			GL.PushMatrix();
			GL.MultMatrix(Matrix4x4.identity);
			GL.Begin(GL.LINES);

			var alt = CurrentTime % 1 > 0.5f;
			var dashGap = Mathf.Clamp(Gizmos.DashGap, 0.01f, 32f);
			var points = new List<Vector3>();

			//draw le elements
			for (var e = 0; e < queueIndex; e++)
			{
				//just in case
				if (queue.Length <= e)
				{
					break;
				}

				var element = queue[e];

				points.Clear();
				if (element.dashed)
				{
					//subdivide
					for (var i = 0; i < element.points.Length - 1; i++)
					{
						var pointA = element.points[i];
						var pointB = element.points[i + 1];
						var direction = pointB - pointA;
						if (direction.sqrMagnitude > dashGap * dashGap * 2f)
						{
							var magnitude = direction.magnitude;
							var amount = Mathf.RoundToInt(magnitude / dashGap);
							direction /= magnitude;

							for (var p = 0; p < amount - 1; p++)
							{
								if (p % 2 == (alt ? 1 : 0))
								{
									var startLerp = p / (amount - 1f);
									var endLerp = (p + 1) / (amount - 1f);
									var start = Vector3.Lerp(pointA, pointB, startLerp);
									var end = Vector3.Lerp(pointA, pointB, endLerp);
									points.Add(start);
									points.Add(end);
								}
							}
						}
						else
						{
							points.Add(pointA);
							points.Add(pointB);
						}
					}
				}
				else
				{
					points.AddRange(element.points);
				}

				GL.Color(element.color);
				for (var i = 0; i < points.Count; i++)
				{
					GL.Vertex(points[i] + offset);
				}
			}

			GL.End();
			GL.PopMatrix();
		}
	}
}
