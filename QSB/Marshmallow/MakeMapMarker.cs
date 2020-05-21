using OWML.ModHelper.Events;
using System.Reflection;
using UnityEngine;

namespace Marshmallow.General
{
    static class MakeMapMarker
    {
        public static void Make(GameObject body, string name)
        {
            var MM = body.AddComponent<MapMarker>();
            MM.SetValue("_labelID", (UITextType)UI.AddToUITable.Add(name));
            MM.SetValue("_markerType", MM.GetType().GetNestedType("MarkerType", BindingFlags.NonPublic).GetField("Planet").GetValue(MM));

            QSB.DebugLog.Console("Map Marker - body : " + body.name + ", labelID : " + name);
        }
    }
}
