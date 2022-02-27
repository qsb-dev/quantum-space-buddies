using Cysharp.Threading.Tasks;
using System.Threading;

namespace QSB.TriggerSync.WorldObjects
{
	public class QSBCharacterTrigger : QSBTrigger<CharacterAnimController>
	{
		public override async UniTask Init(CancellationToken ct)
		{
			await base.Init(ct);
			AttachedObject.OnEntry -= TriggerOwner.OnZoneEntry;
			AttachedObject.OnExit -= TriggerOwner.OnZoneExit;
		}
	}
}