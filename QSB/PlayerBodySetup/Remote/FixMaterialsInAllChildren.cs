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
		private static readonly List<(string MaterialName, Material ReplacementMaterial)> _materialDefinitions = new();

		private static void ReplaceMaterial(Renderer renderer, int index, Material mat)
		{
			var mats = renderer.materials;
			mats[index] = mat;
			renderer.materials = mats;
		}

		private static void ReplaceMaterials(Renderer renderer, string materialName, Material replacementMaterial)
		{
			for (var i = 0; i < renderer.materials.Length; i++)
			{
				if (renderer.materials[i].name.Trim() == $"{materialName} (Instance)")
				{
					ReplaceMaterial(renderer, i, replacementMaterial);
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
				DebugLog.DebugWrite(name);

				var matchingMaterial = allMaterials.Where(x => x.name == name).ToArray();

				foreach (var item in matchingMaterial)
				{
					DebugLog.DebugWrite($"- {item.name}");
				}

				if (matchingMaterial == default)
				{
					DebugLog.ToConsole($"Error - could not find material with the name {name}!", OWML.Common.MessageType.Error);
					return;
				}

				_materialDefinitions.Add(new(name, matchingMaterial[0]));
			}
		}

		public static void ReplaceMaterials(Transform rootObject)
		{
			if (_materialDefinitions.Count == 0)
			{
				GenerateMaterialDefinitions();
			}

			foreach (var renderer in rootObject.GetComponentsInChildren<Renderer>(true))
			{
				foreach (var (materialName, replacementMaterial) in _materialDefinitions)
				{
					ReplaceMaterials(renderer, materialName, replacementMaterial);
				}
			}
		}
	}
}
