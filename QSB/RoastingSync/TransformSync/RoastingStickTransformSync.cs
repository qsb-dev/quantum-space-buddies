using OWML.Utils;
using QSB.Player;
using QSB.TransformSync;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.RoastingSync.TransformSync
{
	internal class RoastingStickTransformSync : SectoredTransformSync
	{
		private Transform _stickTip;
		private Transform _networkStickTip => gameObject.transform.GetChild(0);
		private const float SmoothTime = 0.1f;
		private Vector3 _positionSmoothVelocity;
		private Quaternion _rotationSmoothVelocity;

		private Transform GetPivot()
			=> Resources.FindObjectsOfTypeAll<RoastingStickController>().First().transform.Find("Stick_Root/Stick_Pivot");

		protected override GameObject InitLocalTransform()
		{
			var pivot = GetPivot();
			Player.RoastingStick = pivot.gameObject;
			_stickTip = pivot.Find("Stick_Tip");
			return pivot.gameObject;
		}

		protected override GameObject InitRemoteTransform()
		{
			var newPivot = Instantiate(GetPivot());
			newPivot.parent = null;
			newPivot.gameObject.SetActive(false);
			Destroy(newPivot.Find("Stick_Tip/Props_HEA_RoastingStick/RoastingStick_Arm").gameObject);
			Destroy(newPivot.Find("Stick_Tip/Props_HEA_RoastingStick/RoastingStick_Arm_NoSuit").gameObject);
			var mallowRoot = newPivot.Find("Stick_Tip/Mallow_Root");
			mallowRoot.gameObject.SetActive(false);
			var oldMarshmallow = mallowRoot.GetComponent<Marshmallow>();

			// Recreate particle system
			Destroy(mallowRoot.Find("MallowSmoke").GetComponent<RelativisticParticleSystem>());
			var newSystem = mallowRoot.Find("MallowSmoke").gameObject.AddComponent<CustomRelativisticParticleSystem>();
			newSystem.Init(Player);

			// Create new marshmallow
			var newMarshmallow = mallowRoot.gameObject.AddComponent<QSBMarshmallow>();
			newMarshmallow._fireRenderer = oldMarshmallow.GetValue<MeshRenderer>("_fireRenderer");
			newMarshmallow._smokeParticles = oldMarshmallow.GetValue<ParticleSystem>("_smokeParticles");
			newMarshmallow._mallowRenderer = oldMarshmallow.GetValue<MeshRenderer>("_mallowRenderer");
			newMarshmallow._rawColor = oldMarshmallow.GetValue<Color>("_rawColor");
			newMarshmallow._toastedColor = oldMarshmallow.GetValue<Color>("_toastedColor");
			newMarshmallow._burntColor = oldMarshmallow.GetValue<Color>("_burntColor");
			Destroy(oldMarshmallow);

			Player.RoastingStick = newPivot.gameObject;
			Player.Marshmallow = newMarshmallow;
			mallowRoot.gameObject.SetActive(true);
			_stickTip = newPivot.Find("Stick_Tip");
			return newPivot.gameObject;
		}

		protected override void UpdateTransform()
		{
			base.UpdateTransform();
			if (_stickTip == null)
			{
				DebugLog.ToConsole($"Warning - _stickTip is null for player {PlayerId}", OWML.Common.MessageType.Warning);
				return;
			}

			if (HasAuthority)
			{
				_networkStickTip.localPosition = _stickTip.localPosition;
				_networkStickTip.localRotation = _stickTip.localRotation;
				return;
			}

			_stickTip.localPosition = Vector3.SmoothDamp(_stickTip.localPosition, _networkStickTip.localPosition, ref _positionSmoothVelocity, SmoothTime);
			_stickTip.localRotation = QuaternionHelper.SmoothDamp(_stickTip.localRotation, _networkStickTip.localRotation, ref _rotationSmoothVelocity, SmoothTime);
		}

		public override bool IsReady => Locator.GetPlayerTransform() != null
			&& Player != null
			&& QSBPlayerManager.PlayerExists(Player.PlayerId)
			&& Player.PlayerStates.IsReady
			&& NetId.Value != uint.MaxValue
			&& NetId.Value != 0U;
	}
}