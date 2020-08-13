using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB
{
    public static class WorldRegistry
    {
        private static List<QSBWorldComponent> ComponentList = new List<QSBWorldComponent>();

        public static Dictionary<int, object> GetInstanceIds(Type typeToFind)
        {
            var components = GameObject.FindObjectsOfType(typeToFind);
            var dict = new Dictionary<int, object>();
            foreach (var component in components)
            {
                dict.Add(component.GetInstanceID(), component);
            }
            return dict;
        }
    }

    class QSBWorldComponent
    {
        public int InstanceID;
        public string Hierarchy;
        public object Instance;
    }
}
