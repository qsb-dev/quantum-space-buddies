using QSB.Events;
using QSB.Tools;
using QSB.Utility;
using System;
using UnityEngine;

namespace QSB.TransformSync
{
    public class PlayerCameraSync : TransformSync
    {
        public static PlayerCameraSync LocalInstance { get; private set; }

        public override void OnStartLocalPlayer()
        {
            LocalInstance = this;
        }

        public override uint PlayerId
        {
            get
            {
                uint id = uint.MaxValue;
                try
                {
                    id = netId.Value - 2;
                }
                catch
                {
                    DebugLog.ToConsole($"Error while geting netId of {GetType().Name}! " +
                        $"{Environment.NewLine}     - Did you destroy the TransformSync without destroying the {GetType().Name}?" +
                        $"{Environment.NewLine}     - Did a destroyed TransformSync/{GetType().Name} still have an active action/event listener?" +
                        $"{Environment.NewLine}     If you are a user seeing this, please report this error.", OWML.Common.MessageType.Error);
                }
                return id;
            }
        }

        protected override Transform InitLocalTransform()
        {
            var body = Locator.GetPlayerCamera().gameObject.transform;

            PlayerToolsManager.Init(body);

            Player.Camera = body.gameObject;

            Player.IsReady = true;
            GlobalMessenger<bool>.FireEvent(EventNames.QSBPlayerReady, true);
            GlobalMessenger.FireEvent(EventNames.QSBPlayerStatesRequest);

            return body;
        }

        protected override Transform InitRemoteTransform()
        {
            var body = new GameObject("PlayerCamera");

            PlayerToolsManager.Init(body.transform);

            Player.Camera = body;

            return body.transform;
        }

        public override bool IsReady => Locator.GetPlayerTransform() != null && Player != null;
    }
}
