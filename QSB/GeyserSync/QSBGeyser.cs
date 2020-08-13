using QSB.Events;
using QSB.WorldSync;
using UnityEngine.Networking;

namespace QSB.GeyserSync
{
    public class QSBGeyser : WorldObject
    {
        private GeyserController _geyserController;

        public void Init(GeyserController geyserController, int id)
        {
            Id = id;
            WorldRegistry.WorldObjects.Add(this);
            _geyserController = geyserController;

            geyserController.OnGeyserActivateEvent += () => HandleEvent(true);
            geyserController.OnGeyserDeactivateEvent += () => HandleEvent(false);
        }

        private void HandleEvent(bool state)
        {
            if (NetworkServer.active)
            {
                GlobalMessenger<int, bool>.FireEvent(EventNames.QSBGeyserState, Id, state);
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
