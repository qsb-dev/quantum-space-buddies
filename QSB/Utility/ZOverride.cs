using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QSB.Utility;

[UsedInUnityProject]
public class ZOverride : MonoBehaviour
{
	private const string shaderTestMode = "unity_GUIZTestMode";
	private readonly UnityEngine.Rendering.CompareFunction desiredUIComparison = UnityEngine.Rendering.CompareFunction.Always;
	private Graphic[] uiElementsToApplyTo;
	private readonly Dictionary<Material, Material> materialMappings = new();

	protected virtual void Start()
	{
		uiElementsToApplyTo = gameObject.GetComponentsInChildren<Graphic>();
		foreach (var graphic in uiElementsToApplyTo)
		{
			var material = graphic.materialForRendering;
			if (material == null)
			{
				continue;
			}

			Material materialCopy;
			if (!materialMappings.ContainsKey(material))
			{
				materialCopy = new Material(material);
				materialMappings.Add(material, materialCopy);
			}
			else
			{
				materialCopy = materialMappings[material];
			}

			materialCopy.SetInt(shaderTestMode, (int)desiredUIComparison);
			graphic.material = materialCopy;
		}
	}
}