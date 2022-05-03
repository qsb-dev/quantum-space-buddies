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
			}

			return typeToDrawer.TryGetValue(typeof(T), out var drawer)
				? drawer
				: null;
		}
	}
}