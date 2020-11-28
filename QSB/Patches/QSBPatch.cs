namespace QSB
{
    public abstract class QSBPatch
    {
        public abstract QSBPatchTypes Type { get; }

        public abstract void DoPatches();
    }
}
