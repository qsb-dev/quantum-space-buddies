namespace QSB.Patches
{
	public abstract class QSBPatch
	{
		public abstract PatchType Type { get; }
		public abstract void DoPatches();
		public abstract void DoUnpatches();
	}
}