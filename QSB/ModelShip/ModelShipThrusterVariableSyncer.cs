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


	public void Init(GameObject modelShip)
	{
		ThrusterModel = modelShip.GetComponent<ThrusterModel>();
		_thrusterAudio = modelShip.GetComponentInChildren<ThrusterAudio>();

		ModelShipThrusterManager.CreateModelShipVFX(modelShip);
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
				foreach (var item in ModelShipThrusterManager.ThrusterFlameControllers)
				{
					item.OnStopTranslationalThrust();
				}

				_thrusterAudio.OnStopTranslationalThrust();

				ModelShipThrusterManager.ThrusterWashController.OnStopTranslationalThrust();
			}
			else
			{
				foreach (var item in ModelShipThrusterManager.ThrusterFlameControllers)
				{
					item.OnStartTranslationalThrust();
				}

				_thrusterAudio.OnStartTranslationalThrust();

				ModelShipThrusterManager.ThrusterWashController.OnStartTranslationalThrust();
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
