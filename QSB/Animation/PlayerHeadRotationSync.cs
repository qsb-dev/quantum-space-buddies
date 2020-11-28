using UnityEngine;

namespace QSB.Animation
{
    public class PlayerHeadRotationSync : MonoBehaviour
    {
        private Animator _attachedAnimator;
        private Transform _lookBase;

        public void Init(Transform lookBase)
        {
            _attachedAnimator = GetComponent<Animator>();
            _lookBase = lookBase;
        }

        void LateUpdate()
        {
            var bone = _attachedAnimator.GetBoneTransform(HumanBodyBones.Head);
            // Get the camera's local rotation with respect to the player body
            var lookLocalRotation = Quaternion.Inverse(_attachedAnimator.transform.rotation) * _lookBase.rotation;
            bone.localRotation = Quaternion.Euler(0f, 0f, lookLocalRotation.eulerAngles.x);
        }
    }
}