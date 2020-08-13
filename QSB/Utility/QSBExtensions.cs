using UnityEngine;

namespace QSB.Utility
{
    public static class QSBExtensions
    {
        public static void ChangeEquipState(this PlayerTool tool, bool equipState)
        {
            if (equipState)
            {
                tool.EquipTool();
            }
            else
            {
                tool.UnequipTool();
            }
        }

        public static string GetHierarchy(this GameObject go)
        {
            var name = go.name;
            while (go.transform.parent != null)
            {
                go = go.transform.parent.gameObject;
                name = go.name + "/" + name;
            }
            return name;
        }

        public static object Call(this object obj, string methodName, params object[] args)
        {
            var method = obj.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                return method.Invoke(obj, args);
            }
            return null;
        }
    }
}
