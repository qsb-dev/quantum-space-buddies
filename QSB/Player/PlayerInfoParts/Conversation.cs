using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.Player;

public partial class PlayerInfo
{
	public int CurrentCharacterDialogueTreeId { get; set; } = -1;
	public GameObject CurrentDialogueBox { get; set; }
}
