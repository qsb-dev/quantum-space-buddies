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
            var sunPos = Locator.GetAstroObject(AstroObject.Name.Sun).transform.position;
            var body = Instantiate(GetPlayerModel(), sunPos, Quaternion.identity);

            GetComponent<AnimationSync>().InitRemote(body);

            return body;
        }

    }
}
