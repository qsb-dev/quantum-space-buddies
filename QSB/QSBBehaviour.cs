using UnityEngine;
using UnityEngine.SceneManagement;

namespace QSB {
    abstract class QSBBehaviour: MonoBehaviour {
        protected bool isPlayerAwake;

        protected virtual void Awake () {
            GlobalMessenger.AddListener("WakeUp", PlayerWokeUp);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded (Scene scene, LoadSceneMode mode) {
            if (scene.name == "SolarSystem") {
                StartSolarSystem();
            } else if (scene.name == "EyeOfTheUniverse") {
                StartEyeOfUniverse();
            }
        }

        protected virtual void PlayerWokeUp () {
            isPlayerAwake = true;
        }

        protected virtual void StartSolarSystem () { }

        protected virtual void StartEyeOfUniverse () { }
    }
}
