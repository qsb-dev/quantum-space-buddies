namespace QSB.WorldSync;

/// <summary>
/// todo make sure this isn't totally broken in weird esoteric cases oh god oh fuck
/// </summary>
internal class QSBOWRigidbody : WorldObject<OWRigidbody>
{
	public override bool ShouldDisplayDebug() => false;
}
