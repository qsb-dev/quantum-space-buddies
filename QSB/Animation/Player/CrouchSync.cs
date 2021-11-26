using QSB.Utility;
using QSB.Utility.VariableSync;
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

		public FloatVariableSyncer CrouchVariableSyncer;
		public float CrouchValue = 0f;

		public void Init(PlayerCharacterController playerController, Animator bodyAnim)
		{
			_playerController = playerController;
			_bodyAnim = bodyAnim;

			CrouchVariableSyncer.Init(() => CrouchValue, val => CrouchValue = val);
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
			CrouchVariableSyncer.ValueToSync.Value = jumpChargeFraction;
		}

		private void SyncRemoteCrouch()
		{
			if (_bodyAnim == null)
			{
				return;
			}

			CrouchParam.Target = CrouchValue;
			CrouchParam.Smooth(CrouchSmoothTime);
			var jumpChargeFraction = CrouchParam.Current;
			_bodyAnim.SetLayerWeight(CrouchLayerIndex, jumpChargeFraction);
		}
	}
}