using UnityEngine.Networking;

namespace QSB
{
    public class NetPlayer : NetworkBehaviour
    {
        public static NetPlayer LocalInstance { get; private set; }

        public override void OnStartLocalPlayer()
        {
            LocalInstance = this;
        }
    }
}
