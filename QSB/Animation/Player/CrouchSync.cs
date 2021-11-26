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
		private VariableReference<float> _crouchValueReference;
		private FloatVariableSyncer _variableSyncer;

		public float CrouchValue;

		public void Init(PlayerCharacterController playerController, Animator bodyAnim)
		{
			_playerController = playerController;
			_bodyAnim = bodyAnim;

			DebugLog.DebugWrite($"create reference");
			_crouchValueReference = new VariableReference<float>(() => CrouchValue, val => CrouchValue = val);
			DebugLog.DebugWrite($"add syncer");
			_variableSyncer = gameObject.AddComponent<FloatVariableSyncer>();
			_variableSyncer.FloatToSync = _crouchValueReference;
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
			DebugLog.DebugWrite($"update reference value");
			_crouchValueReference.Value = jumpChargeFraction;
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