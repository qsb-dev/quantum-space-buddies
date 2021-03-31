using OWML.Utils;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.ElevatorSync.WorldObjects
{
	public class QSBElevator : WorldObject<Elevator>
	{
		private Vector3 _startLocalPos;
		private Vector3 _endLocalPos;

		private SingleInteractionVolume _interactVolume;
		private OWAudioSource _owAudioSourceOneShot;
		private OWAudioSource _owAudioSourceLP;

		public override void Init(Elevator elevator, int id)
		{
			AttachedObject = elevator;
			ObjectId = id;
			QSBCore.UnityEvents.RunWhen(() => AttachedObject.GetValue<SingleInteractionVolume>("_interactVolume") != null, InitValues);
		}

		private void InitValues()
		{
			_startLocalPos = AttachedObject.GetValue<Vector3>("_startLocalPos");
			_endLocalPos = AttachedObject.GetValue<Vector3>("_endLocalPos");
			_interactVolume = AttachedObject.GetValue<SingleInteractionVolume>("_interactVolume");
			_owAudioSourceOneShot = AttachedObject.GetValue<OWAudioSource>("_owAudioSourceOneShot");
			_owAudioSourceLP = AttachedObject.GetValue<OWAudioSource>("_owAudioSourceLP");
		}

		public void RemoteCall(bool isGoingUp)
		{
			SetDirection(isGoingUp);
			RemoteStartLift();
		}

		private void SetDirection(bool isGoingUp)
		{
			var targetPos = isGoingUp ? _endLocalPos : _startLocalPos;
			AttachedObject.SetValue("_goingToTheEnd", isGoingUp);
			AttachedObject.SetValue("_targetLocalPos", targetPos);
			_interactVolume.transform.Rotate(0f, 180f, 0f);
		}

		private void RemoteStartLift()
		{
			AttachedObject.enabled = true;
			AttachedObject.SetValue("_initLocalPos", AttachedObject.transform.localPosition);
			AttachedObject.SetValue("_initLiftTime", Time.time);
			_owAudioSourceOneShot.PlayOneShot(AudioType.TH_LiftActivate);
			_owAudioSourceLP.FadeIn(0.5f);
			_interactVolume.DisableInteraction();
		}
	}
}