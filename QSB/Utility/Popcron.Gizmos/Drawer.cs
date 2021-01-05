using System;
using System.Collections.Generic;
using UnityEngine;

namespace Popcron
{
	public abstract class Drawer
	{
		private static Dictionary<Type, Drawer> typeToDrawer = null;

		public abstract int Draw(ref Vector3[] buffer, params object[] args);

		public Drawer()
		{

		}

		public static Drawer Get<T>() where T : class
		{
			//find all drawers
			if (typeToDrawer == null)
			{
				typeToDrawer = new Dictionary<Type, Drawer>
				{

					//add defaults
					{ typeof(CubeDrawer), new CubeDrawer() },
					{ typeof(LineDrawer), new LineDrawer() },
					{ typeof(PolygonDrawer), new PolygonDrawer() },
					{ typeof(SquareDrawer), new SquareDrawer() },
					{ typeof(FrustumDrawer), new FrustumDrawer() }
				};

				//find extras
				var assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (var assembly in assemblies)
				{
					var types = assembly.GetTypes();
					foreach (var type in types)
					{
						if (type.IsAbstract)
						{
							continue;
						}

						if (type.IsSubclassOf(typeof(Drawer)) && !typeToDrawer.ContainsKey(type))
						{
							try
							{
								var value = (Drawer)Activator.CreateInstance(type);
								typeToDrawer[type] = value;
							}
							catch (Exception e)
							{
								Debug.LogError($"couldnt register drawer of type {type} because {e.Message}");
							}
						}
					}
				}
			}

			if (typeToDrawer.TryGetValue(typeof(T), out var drawer))
			{
				return drawer;
			}
			else
			{
				return null;
			}
		}
	}
}
