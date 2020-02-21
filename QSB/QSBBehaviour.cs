using UnityEngine;
using UnityEngine.SceneManagement;

namespace QSB
{
    public abstract class QSBBehaviour : MonoBehaviour
    {
        protected bool IsPlayerAwake;

        protected virtual void Awake()
        {
            GlobalMessenger.AddListener("WakeUp", PlayerWokeUp);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "SolarSystem")
            {
                StartSolarSystem();
            }
            else if (scene.name == "EyeOfTheUniverse")
            {
                StartEyeOfUniverse();
            }
        }

        protected virtual void PlayerWokeUp()
        {
            IsPlayerAwake = true;
        }

        protected virtual void StartSolarSystem() { }

        protected virtual void StartEyeOfUniverse() { }
    }
}
