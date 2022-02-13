using QSB.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.PlayerBodySetup.Remote
{
	public static class FixMaterialsInAllChildren
	{
		private static readonly List<(string MaterialName, Material ReplacementMaterial)> _materialDefinitions = new();
		private static readonly List<string> _materialNames = new()
		{
			"Traveller_HEA_Player_Skin_mat",
			"Traveller_HEA_Player_Clothes_mat",
			"Traveller_HEA_PlayerSuit_mat",
			"Props_HEA_Jetpack_mat",
			"Effects_HEA_MarshmallowFlames_mat",
			"Effects_HEA_Smoke_mat",
			"Props_HEA_RoastingStick_mat",
			"Effects_HEA_ScannerLightVolume_mat",
			"Effects_HEA_ScannerProjector_mat",
			"Props_HEA_Lightbulb_mat",
			"Props_HEA_PlayerTools_mat",
			"Structure_HEA_PlayerShip_Screens_mat",
			"Effects_RecallWhiteHole_mat",
			"Effects_HEA_Vapor_mat",
			"Props_HEA_Lightbulb_OFF_mat",
			"Props_HEA_PlayerProbe_mat",
			"Props_HEA_PlayerProbeLightbulb_mat",
			"Effects_RecallBlackHole_mat"
		};

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
				if (renderer.materials[i].name.Trim() == $"REM_{materialName} (Instance)")
				{
					ReplaceMaterial(renderer, i, replacementMaterial);
				}
			}
		}

		private static void GenerateMaterialDefinitions()
		{
			var allMaterials = (Material[])Resources.FindObjectsOfTypeAll(typeof(Material));

			foreach (var name in _materialNames)
			{
				var matchingMaterial = allMaterials.FirstOrDefault(x => x.name == name);

				if (matchingMaterial == default)
				{
					DebugLog.ToConsole($"Error - could not find material with the name {name}!", OWML.Common.MessageType.Error);
					return;
				}

				_materialDefinitions.Add((name, matchingMaterial));
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
