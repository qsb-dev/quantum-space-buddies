using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.Animation
{
    public class QSBTool : PlayerTool
    {
        public ToolType Type;
        public GameObject _scopeGameObject;

        private void OnEnable()
        {
            _scopeGameObject.SetActive(true);
        }

        private void OnDisable()
        {
            _scopeGameObject.SetActive(false);
        }
    }
}
