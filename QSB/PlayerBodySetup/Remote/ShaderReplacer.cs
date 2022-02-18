using OWML.Common;
using QSB.Utility;
using UnityEngine;

namespace QSB.PlayerBodySetup.Remote
{
	public static class ShaderReplacer
	{
		public static void ReplaceShaders(GameObject prefab)
		{
			foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
			{
				for (var i = 0; i < renderer.sharedMaterials.Length; i++)
				{
					var material = renderer.sharedMaterials[i];
					if (material == null)
					{
						DebugLog.DebugWrite($"shared material {i} is null\n" +
							$"{renderer} | {prefab}", MessageType.Warning);
						continue;
					}

					if (!material.shader.name.StartsWith("PROXY/"))
					{
						DebugLog.DebugWrite("non-proxy shader found\n" +
							$"{material.shader} | {material} | {renderer} | {prefab}", MessageType.Warning);
						continue;
					}

					var replacementShaderName = material.shader.name.Substring("PROXY/".Length);
					var replacementShader = Shader.Find(replacementShaderName);
					if (replacementShader == null)
					{
						DebugLog.DebugWrite($"could not find replacement shader {replacementShaderName}\n" +
							$"{material.shader} | {material} | {renderer} | {prefab}", MessageType.Error);
						continue;
					}

					material.shader = replacementShader;
				}
			}
		}
	}
}
