using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.Events
{
    class PlayerProbeLauncherEvent : QSBEvent
    {
        public override EventType Type => EventType.ProbeLauncherActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger<ProbeLauncher>.AddListener("ProbeLauncherEquipped", var => EventSender.SendEvent(this, PlayerRegistry.LocalPlayer.NetId, true));
            GlobalMessenger<ProbeLauncher>.AddListener("ProbeLauncherUnequipped", var => EventSender.SendEvent(this, PlayerRegistry.LocalPlayer.NetId, false));
        }

        public override void OnReceive(uint sender, object[] data)
        {
            var player = PlayerRegistry.GetPlayer(sender);
            var tool = player.ProbeLauncher;
            player.UpdateState(State.ProbeLauncher, (bool)data[0]);
            if ((bool)data[0] == true)
            {
                tool.EquipTool();
            }
            else
            {
                tool.UnequipTool();
            }
        }

        public override void OnReceiveLocal(object[] data)
        {
            var tool = PlayerRegistry.LocalPlayer.ProbeLauncher;
            if ((bool)data[0] == true)
            {
                tool.EquipTool();
            }
            else
            {
                tool.UnequipTool();
            }
        }
    }
}
