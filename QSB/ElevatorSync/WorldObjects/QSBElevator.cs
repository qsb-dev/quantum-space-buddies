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
		private OWTriggerVolume _elevatorTrigger;

		public override void Init()
		{
			StartDelayedReady();
			QSBCore.UnityEvents.RunWhen(() => AttachedObject._interactVolume != null, InitValues);
		}

		private void InitValues()
		{
			FinishDelayedReady();
			_startLocalPos = AttachedObject._startLocalPos;
			_endLocalPos = AttachedObject._endLocalPos;
			_interactVolume = AttachedObject._interactVolume;
			_owAudioSourceOneShot = AttachedObject._owAudioSourceOneShot;
			_owAudioSourceLP = AttachedObject._owAudioSourceLP;

			var boxShape = AttachedObject.gameObject.AddComponent<BoxShape>();
			boxShape.center = new Vector3(0, 1.75f, 0.25f);
			boxShape.size = new Vector3(3, 3.5f, 3);

			_elevatorTrigger = AttachedObject.gameObject.GetAddComponent<OWTriggerVolume>();
		}

		public void RemoteCall(bool isGoingUp)
		{
			if (_elevatorTrigger.IsTrackingObject(Locator.GetPlayerDetector()))
			{
				SetDirection(isGoingUp);

				AttachedObject._attachPoint.AttachPlayer();

				if (Locator.GetPlayerSuit().IsWearingSuit() && Locator.GetPlayerSuit().IsTrainingSuit())
				{
					Locator.GetPlayerSuit().RemoveSuit();
				}

				RemoteStartLift();
			}
			else
			{
				SetDirection(isGoingUp);
				RemoteStartLift();
			}
		}

		private void SetDirection(bool isGoingUp)
		{
			_interactVolume.transform.Rotate(0f, 180f, 0f);
			var targetPos = isGoingUp ? _endLocalPos : _startLocalPos;
			AttachedObject._targetLocalPos = targetPos;
			AttachedObject._goingToTheEnd = isGoingUp;
		}

		private void RemoteStartLift()
		{
			AttachedObject.enabled = true;
			AttachedObject._initLocalPos = AttachedObject.transform.localPosition;
			AttachedObject._initLiftTime = Time.time;
			_owAudioSourceOneShot.PlayOneShot(AudioType.TH_LiftActivate);
			_owAudioSourceLP.FadeIn(0.5f);
			_interactVolume.DisableInteraction();
		}
	}
}