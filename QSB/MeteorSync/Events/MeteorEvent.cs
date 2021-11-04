using QSB.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using EventType = QSB.Events.EventType;

namespace QSB.MeteorSync.Events {
    /// launch with precalculated launch params
    public class MeteorEvent : QSBEvent<MeteorMessage> {
        public override EventType Type => EventType.Meteor;

        public override void SetupListener() =>
            GlobalMessenger<int>.AddListener(EventNames.QSBMeteorLaunch, Handler);

        public override void CloseListener() =>
            GlobalMessenger<int>.RemoveListener(EventNames.QSBMeteorLaunch, Handler);

        /// server: send precalculated values
        private void Handler(int objectId) {
            var qsbLauncher = QSBWorldSync.GetWorldFromId<QSBMeteorLauncher>(objectId);
            var message = new MeteorMessage {
                AboutId = LocalPlayerId,
                ObjectId = objectId,
                num = qsbLauncher.num,
                launchSpeed = qsbLauncher.launchSpeed,
                damage = qsbLauncher.damage
            };
            DebugLog.DebugWrite($"server sending meteor {message}");
            SendEvent(message);
        }

        /// client: get precalculated values and launch
        public override void OnReceiveRemote(bool isHost, MeteorMessage message) {
            DebugLog.DebugWrite($"client launching meteor with precalculated {message}");

            var qsbLauncher = QSBWorldSync.GetWorldFromId<QSBMeteorLauncher>(message.ObjectId);
            qsbLauncher.num = message.num;
            qsbLauncher.launchSpeed = message.launchSpeed;
            qsbLauncher.damage = message.damage;

            qsbLauncher.AttachedObject.LaunchMeteor();
        }
    }
}
