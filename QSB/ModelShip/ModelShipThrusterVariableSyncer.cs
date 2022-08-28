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

	public void Init(GameObject modelShip)
	{
		DebugLog.ToConsole("init model ship");

		ThrusterModel = modelShip.GetComponent<ThrusterModel>();
		_thrusterAudio = modelShip.GetComponentInChildren<ThrusterAudio>();

		ThrusterFlameControllers.Clear();
		foreach (var item in modelShip.GetComponentsInChildren<ThrusterFlameController>())
		{
			DebugLog.ToConsole("adding thruster");
			ThrusterFlameControllers.Add(item);
		}
	}

	public void Update()
	{
		if (QSBPlayerManager.LocalPlayer.FlyingModelShip)
		{
			DebugLog.ToConsole($"{QSBPlayerManager.LocalPlayerId} is flying the model ship");
			GetFromShip();
			return;
		}

		if (AccelerationSyncer.public_HasChanged())
		{
			DebugLog.ToConsole("value changed");

			if (AccelerationSyncer.Value == Vector3.zero)
			{
				DebugLog.ToConsole("not flying");
				foreach (var item in ThrusterFlameControllers)
				{
					item.OnStopTranslationalThrust();
				}

				_thrusterAudio.OnStopTranslationalThrust();


			}
			else
			{
				DebugLog.ToConsole("flying");
				foreach (var item in ThrusterFlameControllers)
				{
					item.OnStartTranslationalThrust();
				}

				_thrusterAudio.OnStartTranslationalThrust();


			}
		}
	}

	private void GetFromShip()
	{
		DebugLog.ToConsole("Getting from ship");
		if (ThrusterModel)
		{
			DebugLog.ToConsole("Update local acc");
			AccelerationSyncer.Value = ThrusterModel.GetLocalAcceleration();
		}
	}
}
