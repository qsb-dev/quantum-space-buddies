using Mirror;
using QSB.Player;
using QSB.Utility;
using QSB.Utility.VariableSync;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.ModelShip;

public class ModelShipThrusterVariableSyncer : MonoBehaviour
{
	public Vector3VariableSyncer AccelerationSyncer;

	public ThrusterModel ThrusterModel { get; private set; }
	private ThrusterAudio _thrusterAudio;
	public List<ThrusterFlameController> ThrusterFlameControllers = new();
	public ThrusterWashController ThrusterWashController { get; private set; }

	public void Init(GameObject modelShip)
	{
		ThrusterModel = modelShip.GetComponent<ThrusterModel>();
		_thrusterAudio = modelShip.GetComponentInChildren<ThrusterAudio>();

		ThrusterFlameControllers.Clear();
		foreach (var item in modelShip.GetComponentsInChildren<ThrusterFlameController>())
		{
			ThrusterFlameControllers.Add(item);
		}

		ThrusterWashController = modelShip.GetComponentInChildren<ThrusterWashController>();
	}

	public void Update()
	{
		if (QSBPlayerManager.LocalPlayer.FlyingModelShip)
		{
			GetFromShip();
			return;
		}

		if (AccelerationSyncer.public_HasChanged())
		{
			if (AccelerationSyncer.Value == Vector3.zero)
			{
				foreach (var item in ThrusterFlameControllers)
				{
					item.OnStopTranslationalThrust();
				}

				_thrusterAudio.OnStopTranslationalThrust();

				ThrusterWashController.OnStopTranslationalThrust();
			}
			else
			{
				foreach (var item in ThrusterFlameControllers)
				{
					item.OnStartTranslationalThrust();
				}

				_thrusterAudio.OnStartTranslationalThrust();

				ThrusterWashController.OnStartTranslationalThrust();
			}
		}
	}

	private void GetFromShip()
	{
		if (ThrusterModel)
		{
			AccelerationSyncer.Value = ThrusterModel.GetLocalAcceleration();
		}
	}
}
