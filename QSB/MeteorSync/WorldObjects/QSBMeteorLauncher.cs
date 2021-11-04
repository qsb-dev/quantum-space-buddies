using QSB.WorldSync;

namespace QSB.MeteorSync.WorldObjects {
    /// for holding precalculated launch values
    public class QSBMeteorLauncher : WorldObject<MeteorLauncher> {
        public float num = float.NaN, launchSpeed = float.NaN, damage = float.NaN;

        public override void Init(MeteorLauncher meteorLauncher, int id) {
            ObjectId = id;
            AttachedObject = meteorLauncher;
        }
    }
}
