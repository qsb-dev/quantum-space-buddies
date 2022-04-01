using OWML.Common;
using QSB.Utility;
using UnityEngine;

namespace QSB.Player;

public partial class PlayerInfo
{
	public OWCamera Camera
	{
		get
		{
			if (_camera == null && IsReady)
			{
				DebugLog.ToConsole($"Warning - {PlayerId}.Camera is null!", MessageType.Warning);
			}

			return _camera;
		}
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
}
