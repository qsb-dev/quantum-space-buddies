using UnityEngine;

namespace QSB
{
    public class PlayerInfo
    {
        public uint NetId { get; set; }
        public GameObject Body { get; set; }
        public GameObject Camera { get; set; }
        public string Name { get; set; }
        public bool Ready { get; set; }
        public Transform ReferenceSector { get; set; }
        public State State { get; set; }
    }
}