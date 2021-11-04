using QSB.AnglerFish.WorldObjects;
using QSB.WorldSync;

namespace QSB.AnglerFish {
    public class AnglerManager : WorldObjectManager {
        protected override void RebuildWorldObjects(OWScene scene)
            => QSBWorldSync.Init<QSBAngler, AnglerfishController>();
    }
}
