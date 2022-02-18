using QSB.Utility;
using UnityEngine;

namespace QSB.PlayerBodySetup.Remote
{
	public static class ShaderReplacer
	{
		public static void ReplaceShaders(GameObject prefab)
		{
			DebugLog.DebugWrite($"TODO: replace shaders for prefab {prefab}");

			foreach (var renderer in prefab.GetComponentsInChildren<Renderer>())
			{
				DebugLog.DebugWrite($"found renderer {renderer}");
				foreach (var material in renderer.sharedMaterials)
				{
					DebugLog.DebugWrite($"found shared material {material}");
					DebugLog.DebugWrite($"shader = {material.shader}");
				}
			}
		}
	}
}
