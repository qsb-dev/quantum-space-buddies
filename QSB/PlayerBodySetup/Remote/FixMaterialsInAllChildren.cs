using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.PlayerBodySetup.Remote
{
	public static class FixMaterialsInAllChildren
	{
		private static List<MaterialDefinition> _materialDefinitions = new();

		static void ReplaceMaterial(Renderer renderer, int index, Material mat)
		{
			var mats = renderer.materials;
			mats[index] = mat;
			renderer.materials = mats;
		}

		static void CheckReplaceMaterials(Renderer renderer, string materialName, Material replacementMaterial)
		{
			for (var i = 0; i < renderer.materials.Length; i++)
			{
				if (renderer.materials[i].name.Trim() == $"{materialName} (Instance)")
				{
					ReplaceMaterial(renderer, i, replacementMaterial);
					continue;
				}
			}
		}

		public static void ReplaceMaterials(Transform rootObject)
		{
			DebugLog.DebugWrite($"Replace materials on children of {rootObject.name}");

			if (_materialDefinitions.Count == 0)
			{
				try
				{
					GenerateMaterialDefinitions();
				}
				catch (Exception ex)
				{
					DebugLog.ToConsole($"Exception when generating material definitions. {ex}", OWML.Common.MessageType.Error);
					return;
				}
			}

			foreach (var renderer in rootObject.GetComponentsInChildren<Renderer>(true))
			{
				foreach (var def in _materialDefinitions)
				{
					CheckReplaceMaterials(renderer, def.MaterialName, def.ReplacementMaterial);
				}
			}
		}

		private static void GenerateMaterialDefinitions()
		{
			var matNameList = new List<string>()
			{
				"Traveller_HEA_Player_Skin_mat",
				"Traveller_HEA_Player_Clothes_mat",
				"Traveller_HEA_PlayerSuit_mat",
				"Props_HEA_Jetpack_mat"
			};

			var allMaterials = (Material[])Resources.FindObjectsOfTypeAll(typeof(Material));

			foreach (var name in matNameList)
			{
				var matchingMaterial = allMaterials.FirstOrDefault(x => x.name == name);

				if (matchingMaterial == default)
				{
					DebugLog.ToConsole($"Error - could not find material with the name {name}!", OWML.Common.MessageType.Error);
					return;
				}

				_materialDefinitions.Add(new(name, matchingMaterial));
			}
		}
	}
}
