using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace QSB.Events
{
    public class EventListener : MonoBehaviour
    {
        public Dictionary<EventType, List<Type>> TypedList = new Dictionary<EventType, List<Type>>
        {
            {EventType.EquipSignalscope, new List<Type>{typeof(Signalscope)}},
            {EventType.ProbeLauncherEquipped, new List<Type>{typeof(ProbeLauncher)}},
            {EventType.ProbeLauncherUnequipped, new List<Type>{typeof(ProbeLauncher)}},
            {EventType.RetrieveProbe, new List<Type>{typeof(SurveyorProbe)}},
            {EventType.LaunchProbe, new List<Type>{typeof(SurveyorProbe)}}
        };

        private void Awake()
        {
            foreach (var item in Enum.GetNames(typeof(EventType)))
            {
                if (!TypedList.Keys.Contains((EventType)Enum.Parse(typeof(EventType), item)))
                {
                    DebugLog.ToConsole($"Adding listener for {item}");
                    GlobalMessenger.AddListener(item, () => SendEvent(item));
                }
            }
            DebugLog.ToConsole("--------------------");
            // the following code is garbage and stupid and i should be ashamed
            // it's too hot right now i just cant be bothered
            MethodInfo oldMethod;
            MethodInfo newMethod;
            foreach (var item in TypedList)
            {
                switch (item.Value.Count)
                {
                    case 1:
                        oldMethod = GetGenericMethod(typeof(EventListener), "Listen", 1);
                        newMethod = oldMethod.MakeGenericMethod(item.Value[0]);
                        newMethod.Invoke(this, new[] { (object)Enum.GetName(typeof(EventType), item.Key) });
                        break;
                    case 2:
                        oldMethod = GetGenericMethod(typeof(EventListener), "Listen", 2);
                        newMethod = oldMethod.MakeGenericMethod(item.Value[0], item.Value[1]);
                        newMethod.Invoke(this, new[] { (object)Enum.GetName(typeof(EventType), item.Key) });
                        break;
                }
            }
        }

        private void SendEvent(string eventName)
        {
            EventHandler.LocalInstance.Send((EventType)Enum.Parse(typeof(EventType), eventName));
        }

        public void Listen<T>(string eventName)
        {
            DebugLog.ToConsole($"Adding listener for {eventName}");
            GlobalMessenger<T>.AddListener(eventName, (T var) => SendEvent(eventName));
        }

        public void Listen<T,U>(string eventName)
        {
            DebugLog.ToConsole($"Adding listener for {eventName}");
            GlobalMessenger<T,U>.AddListener(eventName, (T var, U var2) => SendEvent(eventName));
        }

        private MethodInfo GetGenericMethod(Type type, string name, int typeCount)
        {
            var methods = from m in type.GetMethods()
                          where m.Name == name
                             && m.IsGenericMethodDefinition

                          let typeParams = m.GetGenericArguments()
                          let normalParams = m.GetParameters()

                          where typeParams.Length == typeCount

                          select m;

            return methods.Single();
        }

        // this is the file i hate most in QSB. it should burn forever.
    }
}
