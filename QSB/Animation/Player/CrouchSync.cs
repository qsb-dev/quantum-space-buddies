using QuantumUNET;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Animation.Player
{
	public class CrouchSync : QNetworkBehaviour
	{
		public AnimFloatParam CrouchParam { get; } = new AnimFloatParam();

		private const float CrouchSmoothTime = 0.05f;
		private const int CrouchLayerIndex = 1;

		private PlayerCharacterController _playerController;
		private Animator _bodyAnim;

		[SyncVar]
		private float _crouchValue;

		public void Init(PlayerCharacterController playerController, Animator bodyAnim)
		{
			_playerController = playerController;
			_bodyAnim = bodyAnim;
		}

		public void Update()
		{
			if (IsLocalPlayer)
			{
				SyncLocalCrouch();
				return;
			}

			SyncRemoteCrouch();
		}

		private void SyncLocalCrouch()
		{
			if (_playerController == null)
			{
				return;
			}

			var jumpChargeFraction = _playerController.GetJumpCrouchFraction();
			_crouchValue = jumpChargeFraction;
		}

		private void SyncRemoteCrouch()
		{
			if (_bodyAnim == null)
			{
				return;
			}

			CrouchParam.Target = _crouchValue;
			CrouchParam.Smooth(CrouchSmoothTime);
			var jumpChargeFraction = CrouchParam.Current;
			_bodyAnim.SetLayerWeight(CrouchLayerIndex, jumpChargeFraction);
		}
	}
}