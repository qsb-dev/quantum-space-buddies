using QSB.ConversationSync.WorldObjects;
using UnityEngine;

namespace QSB.Player;

public partial class PlayerInfo
{
	public QSBCharacterDialogueTree CurrentCharacterDialogueTree { get; set; }
	public GameObject CurrentDialogueBox { get; set; }
}
