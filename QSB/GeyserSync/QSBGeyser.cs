using QSB.Events;
using QSB.WorldSync;
using UnityEngine.Networking;

namespace QSB.GeyserSync
{
    public class QSBGeyser : WorldObject
    {
        public override string UniqueName => _geyserController.name;

        private GeyserController _geyserController;

        public void Init(GeyserController geyserController)
        {
            WorldRegistry.WorldObjects.Add(this);
            _geyserController = geyserController;

            geyserController.OnGeyserActivateEvent += () => HandleEvent(true);
            geyserController.OnGeyserDeactivateEvent += () => HandleEvent(false);
        }

        private void HandleEvent(bool state)
        {
            if (NetworkServer.active)
            {
                GlobalMessenger<string, bool>.FireEvent(EventNames.QSBGeyserState, UniqueName, state);
            }
        }

        public void SetState(bool state)
        {
            if (state)
            {
                _geyserController.ActivateGeyser();
            }
            else
            {
                _geyserController.DeactivateGeyser();
            }
        }
    }
}
