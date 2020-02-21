using UnityEngine;

namespace QSB.Animation
{
    public class AnimFloatParam
    {
        public float Current { get; private set; }
        public float Target { get; set; }

        private float _velocity;

        public float Smooth(float smoothTime)
        {
            Current = Mathf.SmoothDamp(Current, Target, ref _velocity, smoothTime);
            return Current;
        }

    }
}