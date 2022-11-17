using QSB.Utility;
using UnityEngine;

namespace QSB.PlayerBodySetup.Remote;

[UsedInUnityProject]
public class DreamWorldSpawnAnimator : MonoBehaviour
{
	[SerializeField]
	private Transform _bodyRoot;

	private float _progression;
	private Material _spawnEffectMaterial;

	public const float DREAMWORLD_SPAWN_TIME = 2f;

	private void Awake()
	{
		enabled = false;

		_spawnEffectMaterial = GetSpawnMaterial();
		_spawnEffectMaterial.SetFloat("_Enabled", 0f);
		_spawnEffectMaterial.SetFloat("_Progression", 0f);
	}

	private Material GetSpawnMaterial()
	{
		foreach (var renderer in GetComponentsInChildren<Renderer>(true))
		{
			foreach (var material in renderer.sharedMaterials)
			{
				if (material == null)
				{
					continue;
				}

				if (material.shader.name == "DreamWorldSpawnEffect")
				{
					return material;
				}
			}
		}

		return null;
	}

	public void StartSpawnEffect()
	{
		_progression = 0;
		_spawnEffectMaterial.SetFloat("_Enabled", 1f);
		enabled = true;
	}

	private void Update()
	{
		_spawnEffectMaterial.SetVector("_BodyPosition", _bodyRoot.position);

		_progression = Mathf.MoveTowards(_progression, 4, 4 * Time.deltaTime / DREAMWORLD_SPAWN_TIME);

		if (OWMath.ApproxEquals(_progression, 4))
		{
			_progression = 4;
			_spawnEffectMaterial.SetFloat("_Enabled", 0f);
			enabled = false;
		}

		_spawnEffectMaterial.SetFloat("_Progression", _progression);
	}
}
