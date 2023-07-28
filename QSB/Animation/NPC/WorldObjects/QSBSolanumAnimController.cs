using QSB.TriggerSync.WorldObjects;
using QSB.WorldSync;
using System.Linq;

namespace QSB.Animation.NPC.WorldObjects;

/// <summary>
/// only used to get QSBSolanumTrigger from SolanumAnimController
/// </summary>
public class QSBSolanumAnimController : WorldObject<SolanumAnimController>
{
	private QSBSolanumTrigger _trigger;
	public QSBSolanumTrigger Trigger => _trigger ??= QSBWorldSync.GetWorldObjects<QSBSolanumTrigger>().First();
}