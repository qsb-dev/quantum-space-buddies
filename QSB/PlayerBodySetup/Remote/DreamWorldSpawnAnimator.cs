using UnityEngine;

namespace QSB.PlayerBodySetup.Remote;

public class DreamWorldSpawnAnimator : MonoBehaviour
{
	[SerializeField]
	private Transform _bodyRoot;

	private float _progression;
	private Renderer[] _renderers;
	private Material _spawnEffectMaterial;

	public const float DREAMWORLD_SPAWN_TIME = 2f;

	private void Awake()
	{
		_renderers = GetComponentsInChildren<Renderer>(true);
		enabled = false;

		foreach (var renderer in _renderers)
		{
			foreach (var material in renderer.sharedMaterials)
			{
				if (material == null)
				{
					continue;
				}

				if (material.shader.name == "DreamWorldSpawnEffect")
				{
					_spawnEffectMaterial = material;
					return;
				}
			}
		}

		_spawnEffectMaterial.SetFloat("_Enabled", 0f);
		_spawnEffectMaterial.SetFloat("_Progression", 0f);
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
