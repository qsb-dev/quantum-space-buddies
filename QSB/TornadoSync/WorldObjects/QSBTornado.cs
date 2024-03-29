﻿using QSB.Messaging;
using QSB.TornadoSync.Messages;
using QSB.WorldSync;

namespace QSB.TornadoSync.WorldObjects;

public class QSBTornado : WorldObject<TornadoController>
{
	public override void SendInitialState(uint to) =>
		this.SendMessage(new TornadoFormStateMessage(FormState) { To = to });

	public bool FormState
	{
		get => AttachedObject._tornadoRoot.activeSelf // forming or formed or collapsing
		       && !AttachedObject._tornadoCollapsing; // and not collapsing
		set
		{
			if (FormState == value)
			{
				return;
			}

			if (value)
			{
				AttachedObject._tornadoCollapsing = false;
				AttachedObject.StartFormation();
			}
			else
			{
				AttachedObject._secondsUntilFormation = 0;
				AttachedObject.StartCollapse();
			}
		}
	}
}