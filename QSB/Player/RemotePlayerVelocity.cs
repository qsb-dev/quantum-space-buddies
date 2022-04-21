using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.Player;

public class RemotePlayerVelocity : MonoBehaviour
{
	private Vector3 _prevPosition;

	public Vector3 Velocity { get; private set; }

	public void FixedUpdate()
	{
		Velocity = (transform.position - _prevPosition) / Time.fixedDeltaTime;
		_prevPosition = transform.position;
	}
}
