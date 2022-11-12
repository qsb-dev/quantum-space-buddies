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
		foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
		{
			foreach (var material in renderer.sharedMaterials)
			{
				if (material == null)
				{
					continue;
				}

				var replacementShader = Shader.Find(material.shader.name);
				if (replacementShader == null)
				{
					continue;
				}

				// preserve override tag and render queue (for Standard shader)
				// keywords and properties are already preserved
				var renderType = material.GetTag("RenderType", false);
				var renderQueue = material.renderQueue;
				material.shader = replacementShader;
				material.SetOverrideTag("RenderType", renderType);
				material.renderQueue = renderQueue;
			}
		}
	}
}
