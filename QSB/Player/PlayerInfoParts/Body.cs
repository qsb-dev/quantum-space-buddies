using OWML.Common;
using QSB.Utility;
using UnityEngine;

namespace QSB.Player;

public partial class PlayerInfo
{
	public OWCamera Camera
	{
		get => _camera;
		set
		{
			if (value == null)
			{
				DebugLog.ToConsole($"Warning - Setting {PlayerId}.Camera to null.", MessageType.Warning);
			}

			_camera = value;
		}
	}
	private OWCamera _camera;

	public GameObject CameraBody { get; set; }

	public GameObject Body
	{
		get
		{
			if (_body == null && IsReady)
			{
				DebugLog.ToConsole($"Warning - {PlayerId}.Body is null!", MessageType.Warning);
			}

			return _body;
		}
		set
		{
			if (value == null)
			{
				DebugLog.ToConsole($"Warning - Setting {PlayerId}.Body to null.", MessageType.Warning);
			}

			_body = value;
		}
	}
	private GameObject _body;

	/// <summary>
	/// remote light sensor is disabled.
	/// it only acts as a storage of data and is always synced with the local light sensor.
	/// </summary>
	public LightSensor LightSensor
	{
		get
		{
			if (IsLocalPlayer)
			{
				return Locator.GetPlayerLightSensor();
			}

			if (CameraBody == null)
			{
				DebugLog.ToConsole($"Error - Can't get LightSensor for {PlayerId}, because CameraBody is null.", MessageType.Error);
				return null;
			}

			return CameraBody.transform.Find("REMOTE_CameraDetector").GetComponent<LightSensor>();
		}
	}

	public Vector3 Velocity
	{
		get
		{
			if (IsLocalPlayer)
			{
				return Locator.GetPlayerBody().GetVelocity();
			}

			if (Body == null)
			{
				DebugLog.ToConsole($"Error - Can't get velocity for {PlayerId}, because Body is null.", MessageType.Error);
				return Vector3.zero;
			}

			return Body.GetComponent<RemotePlayerVelocity>().Velocity;
		}
	}
}
