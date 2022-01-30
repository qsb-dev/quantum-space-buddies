using Cysharp.Threading.Tasks;
using System.Threading;

namespace QSB.TriggerSync.WorldObjects
{
	public class QSBSolanumTrigger : QSBTrigger<NomaiConversationManager>
	{
		public override async UniTask Init(CancellationToken ct)
		{
			await base.Init(ct);
			AttachedObject.OnEntry -= TriggerOwner.OnEnterWatchVolume;
			AttachedObject.OnExit -= TriggerOwner.OnExitWatchVolume;
		}
	}
}
