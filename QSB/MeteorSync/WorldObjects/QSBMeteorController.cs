using QSB.WorldSync;

namespace QSB.MeteorSync.WorldObjects {
    /// just for holding precalculated damage value
    public class QSBMeteorController : WorldObject<MeteorController> {
        public float damage = float.NaN;

        public override void Init(MeteorController meteorController, int id) {
            ObjectId = id;
            AttachedObject = meteorController;
        }
    }
}
