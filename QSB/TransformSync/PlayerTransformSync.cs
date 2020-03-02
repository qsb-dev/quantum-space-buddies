using QSB.Animation;
using UnityEngine;

namespace QSB.TransformSync
{
    public class PlayerTransformSync : TransformSync
    {
        public static PlayerTransformSync LocalInstance { get; private set; }

        private Transform _playerModel;

        private Transform GetPlayerModel()
        {
            if (!_playerModel)
            {
                _playerModel = Locator.GetPlayerBody().transform.Find("Traveller_HEA_Player_v2");
            }
            return _playerModel;
        }

        protected override Transform InitLocalTransform()
        {
            LocalInstance = this;
            var body = GetPlayerModel();

            GetComponent<AnimationSync>().InitLocal(body);

            return body;
        }

        protected override Transform InitRemoteTransform()
        {
            var body = Instantiate(GetPlayerModel());

            GetComponent<AnimationSync>().InitRemote(body);

            return body;
        }

        private void Update()
        {
            if (!isLocalPlayer && transform.position == Vector3.zero)
            {
                transform.position = Locator.GetAstroObject(AstroObject.Name.Sun).transform.position;
            }
        }

    }
}
