using Mirror;
using QSB.Player;
using QSB.Utility.VariableSync;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.ModelShip;

public class ModelShipThrusterVariableSyncer : NetworkBehaviour
{
	public Vector3VariableSyncer AccelerationSyncer;

	private ThrusterModel _thrusterModel;
	private ThrusterAudio _thrusterAudio;
	public List<ThrusterFlameController> ThrusterFlameControllers = new();

	public static ModelShipThrusterVariableSyncer LocalInstance;

	public void Start()
	{
		LocalInstance = this;
	}

	public void Init()
	{
		_thrusterModel = gameObject.GetComponent<ThrusterModel>();
		_thrusterAudio = gameObject.GetComponentInChildren<ThrusterAudio>();

		ThrusterFlameControllers.Clear();
		foreach (var item in gameObject.GetComponentsInChildren<ThrusterFlameController>())
		{
			ThrusterFlameControllers.Add(item);
		}
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


			}
			else
			{
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
		if (_thrusterModel)
		{
			AccelerationSyncer.Value = _thrusterModel.GetLocalAcceleration();
		}
	}
}
