using QSB.MeteorSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.MeteorSync {
    public class MeteorManager : WorldObjectManager {
        protected override void RebuildWorldObjects(OWScene scene) {
            QSBWorldSync.Init<QSBMeteorLauncher, MeteorLauncher>();
            QSBWorldSync.Init<QSBMeteorController, MeteorController>();
        }
    }
}
