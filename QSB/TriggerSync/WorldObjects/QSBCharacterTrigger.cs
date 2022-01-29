using Cysharp.Threading.Tasks;
using System.Threading;

namespace QSB.TriggerSync.WorldObjects
{
	public class QSBCharacterTrigger : QSBTrigger<CharacterAnimController>
	{
		public override async UniTask Init(CancellationToken cancellationToken)
		{
			await base.Init(cancellationToken);
			AttachedObject.OnEntry -= TriggerOwner.OnZoneEntry;
			AttachedObject.OnExit -= TriggerOwner.OnZoneExit;
		}
	}
}
