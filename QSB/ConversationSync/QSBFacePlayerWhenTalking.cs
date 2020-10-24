using QSB.Utility;
using UnityEngine;

namespace QSB.ConversationSync
{
    class QSBFacePlayerWhenTalking : MonoBehaviour
    {
        private CharacterDialogueTree _dialogueTree;
        private Quaternion _origLocalRotation;
        private Quaternion _targetLocalRotation;

        private void Awake()
        {
            _dialogueTree = GetComponentInChildren<CharacterDialogueTree>();
            DebugLog.DebugWrite("Awake of QSBFacePlayer. Attached to " + _dialogueTree.name);
            if (_dialogueTree != null)
            {
                _dialogueTree.OnStartConversation += () => StartConversation(Locator.GetPlayerTransform().position);
                _dialogueTree.OnEndConversation += EndConversation;
            }
            _origLocalRotation = base.transform.localRotation;
        }

        private void OnDestroy()
        {
            if (_dialogueTree != null)
            {
                _dialogueTree.OnStartConversation -= () => StartConversation(Locator.GetPlayerTransform().position);
                _dialogueTree.OnEndConversation -= EndConversation;
            }
        }

        private void Start()
        {
            enabled = false;
        }

        public void StartConversation(Vector3 playerPosition)
        {
            Vector3 vector = playerPosition - transform.position;
            Vector3 vector2 = vector - Vector3.Project(vector, transform.up);
            float angle = Vector3.Angle(transform.forward, vector2) * Mathf.Sign(Vector3.Dot(vector2, transform.right));
            Vector3 axis = transform.parent.InverseTransformDirection(transform.up);
            Quaternion lhs = Quaternion.AngleAxis(angle, axis);
            this.FaceLocalRotation(lhs * transform.localRotation);
        }

        public void EndConversation()
        {
            FaceLocalRotation(_origLocalRotation);
        }

        private void FaceLocalRotation(Quaternion targetLocalRotation)
        {
            enabled = true;
            _targetLocalRotation = targetLocalRotation;
        }

        private void Update()
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, _targetLocalRotation, 0.1f);
            if (Mathf.Abs(Quaternion.Angle(transform.localRotation, _targetLocalRotation)) < 1f)
            {
                enabled = false;
            }
        }
    }
}
