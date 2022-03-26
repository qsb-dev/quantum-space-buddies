using QSB.Utility;
using UnityEngine;

namespace QSB.PlayerBodySetup.Remote;

public static class ShaderReplacer
{
	/// <summary>
	/// the materials on the prefabs have the exact same name as the ones in game.
	/// if we just use Shader.Find, we can get the in-game ones instead of the prefab ones,
	/// and replace the prefab ones with the in-game ones.
	/// i am amazed that this works, and i hope it isn't super brittle.
	/// </summary>
	public static void ReplaceShaders(GameObject prefab)
	{
		DebugLog.DebugWrite($"Processing shaders for {prefab.name}...", OWML.Common.MessageType.Info);

		foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
		{
			foreach (var material in renderer.sharedMaterials)
			{
				if (material == null)
				{
					continue;
				}

				if (material.shader == null)
				{
					DebugLog.ToConsole($"Warning - Shader for material {material.name} is null.", OWML.Common.MessageType.Warning);
					continue;
				}

				var replacementShader = Shader.Find(material.shader.name);

				if (replacementShader == null)
				{
					DebugLog.ToConsole($"Warning - Replacement shader found for shader {material.shader.name} was null. Sticking with the original.", OWML.Common.MessageType.Warning);
					continue;
				}

				material.shader = replacementShader;
			}
		}
	}
}