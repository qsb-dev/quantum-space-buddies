using QSB.Player;

namespace QSB.TriggerSync.WorldObjects
{
	public class QSBShrineTrigger : QSBTrigger<QuantumShrine>
	{
		protected override void OnEnter(PlayerInfo player) => player.IsInShrine = true;

		protected override void OnExit(PlayerInfo player) => player.IsInShrine = false;
	}
}