using UnityEngine;

namespace QSB.PlayerBodySetup.Remote
{
	public struct MaterialDefinition
	{
		public MaterialDefinition(string materialName, Material replacementMaterial)
		{
			MaterialName = materialName;
			ReplacementMaterial = replacementMaterial;
		}

		public string MaterialName { get; private set; }
		public Material ReplacementMaterial { get; private set; }
	}
}
