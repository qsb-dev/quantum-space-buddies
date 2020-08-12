using OWML.ModHelper.Events;
using UnityEngine;

namespace QSB.ElevatorSync
{
    public class ElevatorController : MonoBehaviour
    {
        public static ElevatorController Instance { get; private set; }

        private Elevator _elevator;
        private Vector3 _startLocalPos;
        private Vector3 _endLocalPos;

        private SingleInteractionVolume _interactVolume;
        private OWAudioSource _owAudioSourceOneShot;
        private OWAudioSource _owAudioSourceLP;

        private void Awake()
        {
            Instance = this;

            QSB.Helper.Events.Subscribe<Elevator>(OWML.Common.Events.AfterAwake);
            QSB.Helper.Events.Event += OnEvent;

            QSB.Helper.HarmonyHelper.AddPostfix<Elevator>("StartLift", typeof(ElevatorPatches), nameof(ElevatorPatches.StartLift));
        }

        private void OnEvent(MonoBehaviour behaviour, OWML.Common.Events ev)
        {
            if (behaviour is Elevator elevator && ev == OWML.Common.Events.AfterAwake)
            {
                _elevator = elevator;
                _startLocalPos = _elevator.GetValue<Vector3>("_startLocalPos");
                _endLocalPos = _elevator.GetValue<Vector3>("_endLocalPos");
                _interactVolume = _elevator.GetValue<SingleInteractionVolume>("_interactVolume");
                _owAudioSourceOneShot = _elevator.GetValue<OWAudioSource>("_owAudioSourceOneShot");
                _owAudioSourceLP = _elevator.GetValue<OWAudioSource>("_owAudioSourceLP");
            }
        }

        private void Update()
        {
            if (_elevator == null)
            {
                return;
            }
            if (Input.GetKeyDown(KeyCode.PageUp))
            {
                LocalCall(ElevatorDirection.Up);
            }
            if (Input.GetKeyDown(KeyCode.PageDown))
            {
                LocalCall(ElevatorDirection.Down);
            }
        }

        public void RemoteCall(ElevatorDirection direction)
        {
            PrepareForMoving(direction);
            RemoteStartLift();
        }

        private void LocalCall(ElevatorDirection direction)
        {
            PrepareForMoving(direction);
            _elevator.Invoke("StartLift");
        }

        private void PrepareForMoving(ElevatorDirection direction)
        {
            var isGoingUp = direction == ElevatorDirection.Up;
            var targetPos = isGoingUp ? _endLocalPos : _startLocalPos;
            _elevator.SetValue("_goingToTheEnd", isGoingUp);
            _elevator.SetValue("_targetLocalPos", targetPos);
            _interactVolume.transform.Rotate(0f, 180f, 0f);
        }

        private void RemoteStartLift()
        {
            _elevator.enabled = true;
            _elevator.SetValue("_initLocalPos", _elevator.transform.localPosition);
            _elevator.SetValue("_initLiftTime", Time.time);
            _owAudioSourceOneShot.PlayOneShot(AudioType.TH_LiftActivate);
            _owAudioSourceLP.FadeIn(0.5f);
            _interactVolume.DisableInteraction();
        }
    }
}
