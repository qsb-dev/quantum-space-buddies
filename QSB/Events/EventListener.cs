using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace QSB.Events
{
    public class EventListener : MonoBehaviour
    {
        private readonly Dictionary<EventType, List<Type>> _typedList = new Dictionary<EventType, List<Type>>
        {
            { EventType.EquipSignalscope, new List<Type> { typeof(Signalscope)} },
            { EventType.ProbeLauncherEquipped, new List<Type> { typeof(ProbeLauncher) } },
            { EventType.ProbeLauncherUnequipped, new List<Type> { typeof(ProbeLauncher) } },
            { EventType.RetrieveProbe, new List<Type> { typeof(SurveyorProbe) } },
            { EventType.LaunchProbe, new List<Type> { typeof(SurveyorProbe) } }
        };

        private void Awake()
        {
            foreach (var item in Enum.GetNames(typeof(EventType)))
            {
                if (!_typedList.Keys.Contains((EventType)Enum.Parse(typeof(EventType), item)))
                {
                    GlobalMessenger.AddListener(item, () => SendEvent(item));
                }
            }

            foreach (var item in _typedList)
            {
                InvokeGenericMethod(item.Key, item.Value);
            }
        }

        private void InvokeGenericMethod(EventType eventType, List<Type> items)
        {
            var oldMethod = GetGenericMethod(typeof(EventListener), "Listen", items.Count);
            var newMethod = oldMethod.MakeGenericMethod(items.ToArray());
            newMethod.Invoke(this, new[] { (object)Enum.GetName(typeof(EventType), eventType) });
        }

        private void SendEvent(string eventName)
        {
            EventHandler.LocalInstance.Send((EventType)Enum.Parse(typeof(EventType), eventName));
        }

        public void Listen<T>(string eventName)
        {
            GlobalMessenger<T>.AddListener(eventName, var => SendEvent(eventName));
        }

        public void Listen<T, TU>(string eventName)
        {
            GlobalMessenger<T, TU>.AddListener(eventName, (var, var2) => SendEvent(eventName));
        }

        private MethodInfo GetGenericMethod(Type type, string methodName, int typeCount)
        {
            var methods = type.GetMethods()
                .Where(m => m.Name == methodName && m.IsGenericMethodDefinition)
                .Select(m => new { m, typeParams = m.GetGenericArguments() })
                .Select(t => new { t, normalParams = t.m.GetParameters() })
                .Where(t => t.t.typeParams.Length == typeCount)
                .Select(t => t.t.m);

            return methods.Single();
        }

    }
}
