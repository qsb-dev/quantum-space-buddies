using System.Collections.Generic;
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
			var localPlayerAnimController = Locator.GetPlayerBody().GetComponentInChildren<PlayerAnimController>(true);

			var playerClothesMat = localPlayerAnimController._unsuitedGroup.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material;
			var playerSkinMat = localPlayerAnimController._unsuitedGroup.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material;
			var playerSuitMat = localPlayerAnimController._suitedGroup.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material;
			var playerJetpackMat = localPlayerAnimController._suitedGroup.transform.GetChild(4).GetComponent<SkinnedMeshRenderer>().material;

			var roastingSystem = Locator.GetPlayerCamera().transform.Find("RoastingSystem");
			var stickRoot = roastingSystem.GetChild(0);
			var stickPivot = stickRoot.GetChild(0);
			var stickTip = stickPivot.GetChild(0);

			var localMallowRoot = stickTip.Find("Mallow_Root");
			var localMallowFlames = localMallowRoot.Find("Effects_HEA_MarshmallowFlames");
			var mallowFlamesMat = localMallowFlames.GetComponent<MeshRenderer>().material;

			var localStick = stickTip.Find("Props_HEA_RoastingSick").GetChild(0);
			var roastingStickMat = localStick.GetComponent<MeshRenderer>().material;

			_materialDefinitions.Add((playerSkinMat.name, playerSkinMat));
			_materialDefinitions.Add((playerClothesMat.name, playerClothesMat));
			_materialDefinitions.Add((playerSuitMat.name, playerSuitMat));
			_materialDefinitions.Add((playerJetpackMat.name, playerJetpackMat));
			_materialDefinitions.Add((mallowFlamesMat.name, mallowFlamesMat));
			_materialDefinitions.Add((roastingStickMat.name, roastingStickMat));
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
