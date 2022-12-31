using UnityEngine;

namespace QSB.Taunts.ThirdPersonCamera;

internal class CameraController : MonoBehaviour
{
	public GameObject CameraObject { get; set; }

	private float _degreesX;
	private float _degreesY;
	private Quaternion _rotationX;
	private Quaternion _rotationY;

	// How far along the ray to move the camera. Avoids clipping into the walls.
	private const float PERCENT_TO_MOVE = 0.80f;

	// Maximum distance for camera clipping
	private const float RAY_LENGTH = 5f;

	// How fast the camera should slide away from the player
	private const float SLIDE_SPEED = 2f;

	public void FixedUpdate()
	{
		if (CameraManager.Instance.Mode != CameraMode.ThirdPerson)
		{
			return;
		}

		UpdatePosition();
		UpdateInput();
		UpdateRotation();
	}

	private void UpdatePosition()
	{
		var origin = transform.position;
		var localDirection = CameraObject.transform.localPosition.normalized;
		Vector3 localTargetPoint;
		if (Physics.Raycast(origin, transform.TransformDirection(localDirection), out var outRay, RAY_LENGTH, LayerMask.GetMask("Default")))
		{
			// Raycast hit collider, get target from hitpoint.
			localTargetPoint = transform.InverseTransformPoint(outRay.point) * PERCENT_TO_MOVE;
		}
		else
		{
			// Raycast didn't hit collider, get target from camera direction
			localTargetPoint = RAY_LENGTH * PERCENT_TO_MOVE * localDirection;
		}

		// If we need to move the camera closer to the player, do it instantly. If not, slide it out.
		var targetDistance = Vector3.Distance(origin, transform.TransformPoint(localTargetPoint));
		var currentDistance = Vector3.Distance(origin, CameraObject.transform.position);
		var movement = targetDistance < currentDistance
			? localTargetPoint
			: Vector3.MoveTowards(CameraObject.transform.localPosition, localTargetPoint, Time.fixedDeltaTime * SLIDE_SPEED);
		CameraObject.transform.localPosition = movement;
	}

	private void UpdateInput()
	{
		var input = InputLibrary.look.GetAxisValue(false);
		_degreesX += input.x * 180f * Time.fixedDeltaTime;
		_degreesY += input.y * 180f * Time.fixedDeltaTime;
	}

	private void UpdateRotation()
	{
		_degreesX %= 360f;
		_degreesY %= 360f;
		_degreesY = Mathf.Clamp(_degreesY, -80f, 80f);
		_rotationX = Quaternion.AngleAxis(_degreesX, Vector3.up);
		_rotationY = Quaternion.AngleAxis(_degreesY, Vector3.left);
		var localRotation = _rotationX * _rotationY * Quaternion.identity;
		transform.localRotation = localRotation;
	}
}
