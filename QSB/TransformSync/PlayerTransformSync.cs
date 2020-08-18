using QSB.Animation;
using QSB.Utility;
using System;
using UnityEngine;

namespace QSB.TransformSync
{
    public class PlayerTransformSync : TransformSync
    {
        public static PlayerTransformSync LocalInstance { get; private set; }

        static PlayerTransformSync()
        {
            AnimControllerPatch.Init();
        }

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
                    id = netId.Value - 0;
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

        private Transform GetPlayerModel()
        {
            return Locator.GetPlayerTransform().Find("Traveller_HEA_Player_v2");
        }

        protected override Transform InitLocalTransform()
        {
            var body = GetPlayerModel();

            GetComponent<AnimationSync>().InitLocal(body);

            Player.Body = body.gameObject;

            return body;
        }

        protected override Transform InitRemoteTransform()
        {
            var body = Instantiate(GetPlayerModel());

            GetComponent<AnimationSync>().InitRemote(body);

            var marker = body.gameObject.AddComponent<PlayerHUDMarker>();
            marker.Init(Player);

            Player.Body = body.gameObject;

            return body;
        }

        public override bool IsReady => Locator.GetPlayerTransform() != null && Player != null && Player.IsReady;
    }
}
