using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.PlayerBodySetup.Remote;

public class DreamWorldSpawnAnimator : MonoBehaviour
{
	[SerializeField]
	private Transform _bodyRoot;

	private float _progression;
	private OWRenderer[] _renderers;
	private Material _spawnEffectMaterial;

	public const float DREAMWORLD_SPAWN_TIME = 2f;

	private void Awake()
	{
		_renderers = GetComponentsInChildren<Renderer>(true)
			.Select(x => x.gameObject.GetAddComponent<OWRenderer>())
			.ToArray();
		enabled = false;

		foreach (var renderer in _renderers)
		{
			if (renderer is null)
			{
				DebugLog.ToConsole($"Error - A renderer found on {gameObject.name} is null!", OWML.Common.MessageType.Error);
				continue;
			}

			foreach (var material in renderer.sharedMaterials)
			{
				if (material is null)
				{
					DebugLog.ToConsole($"Error - A material on renderer {renderer.name} is null!", OWML.Common.MessageType.Error);
					continue;
				}

				if (material.shader is null)
				{
					DebugLog.ToConsole($"Error - The shader on material {material.name}, attached to renderer {renderer.name}, is null!", OWML.Common.MessageType.Error);
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

		_progression = Mathf.MoveTowards(_progression, 4, (4 * Time.deltaTime) / DREAMWORLD_SPAWN_TIME);

		if (OWMath.ApproxEquals(_progression, 4))
		{
			_progression = 4;
			_spawnEffectMaterial.SetFloat("_Enabled", 0f);
			enabled = false;
		}

		_spawnEffectMaterial.SetFloat("_Progression", _progression);
	}
}
