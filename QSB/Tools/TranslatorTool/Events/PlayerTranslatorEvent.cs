﻿using QSB.Events;
using QSB.Messaging;
using QSB.Player;

namespace QSB.Tools.TranslatorTool.Events
{
	public class PlayerTranslatorEvent : QSBEvent<ToggleMessage>
	{
		public override bool RequireWorldObjectsReady() => false;

		public override void SetupListener()
		{
			GlobalMessenger.AddListener(EventNames.EquipTranslator, HandleEquip);
			GlobalMessenger.AddListener(EventNames.UnequipTranslator, HandleUnequip);
		}

		public override void CloseListener()
		{
			GlobalMessenger.RemoveListener(EventNames.EquipTranslator, HandleEquip);
			GlobalMessenger.RemoveListener(EventNames.UnequipTranslator, HandleUnequip);
		}

		private void HandleEquip() => SendEvent(CreateMessage(true));
		private void HandleUnequip() => SendEvent(CreateMessage(false));

		private ToggleMessage CreateMessage(bool value) => new()
		{
			AboutId = LocalPlayerId,
			ToggleValue = value
		};

		public override void OnReceiveRemote(bool server, ToggleMessage message)
		{
			var player = QSBPlayerManager.GetPlayer(message.AboutId);
			player.TranslatorEquipped = message.ToggleValue;
			player.Translator?.ChangeEquipState(message.ToggleValue);
		}

		public override void OnReceiveLocal(bool server, ToggleMessage message) =>
			QSBPlayerManager.LocalPlayer.TranslatorEquipped = message.ToggleValue;
	}
}