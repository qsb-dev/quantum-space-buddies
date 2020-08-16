using System;
using System.Collections;
using UnityEngine;

namespace QSB.Utility
{
    public class UnityHelper : MonoBehaviour
    {
        public static UnityHelper Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void RunWhen(Func<bool> when, Action what)
        {
            StartCoroutine(WaitUntil(when, what));
        }

        private IEnumerator WaitUntil(Func<bool> when, Action what)
        {
            yield return new WaitUntil(when);
            what();
        }

    }
}