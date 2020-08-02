using OWML.ModHelper.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.Tools
{
    public class QSBProbe : MonoBehaviour
    {
        private uint _attachedNetId;

        public void Init(uint netid)
        {
            _attachedNetId = netid;
        }

        public void Activate()
        {
            gameObject.SetActive(true);
            gameObject.transform.parent = null;
        }

        public void Deactivate()
        {
            //gameObject.SetActive(false);
            gameObject.transform.parent = PlayerRegistry.GetPlayer(_attachedNetId).Body.transform;
            gameObject.transform.localPosition = Vector3.zero;
        }
    }
}
