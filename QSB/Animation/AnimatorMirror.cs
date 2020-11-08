using OWML.Common;
using QSB.Player;
using QSB.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.Animation
{
    public class AnimatorMirror : MonoBehaviour
    {
        private const float SmoothTime = 0.05f;

        private Animator _from;
        private Animator _to;

        private readonly Dictionary<string, AnimFloatParam> _floatParams = new Dictionary<string, AnimFloatParam>();

        public void Init(Animator from, Animator to)
        {
            _from = from;
            _to = to;
            if (_from.runtimeAnimatorController == null)
            {
                DebugLog.DebugWrite($"Warning - \"From\" ({from.name}) controller is null.", MessageType.Warning);
                _from.runtimeAnimatorController = _to.runtimeAnimatorController;
            }
            else if (_to.runtimeAnimatorController == null)
            {
                DebugLog.DebugWrite($"Warning - \"To\" ({to.name}) controller is null.", MessageType.Warning);
                _to.runtimeAnimatorController = _from.runtimeAnimatorController;
            }
            foreach (var param in _from.parameters.Where(p => p.type == AnimatorControllerParameterType.Float))
            {
                _floatParams.Add(param.name, new AnimFloatParam());
            }
        }

        private PlayerInfo GetPlayer()
        {
            return QSBPlayerManager.GetSyncObjects<AnimationSync>().First(x => x.Mirror == this).Player;
        }

        private void Update()
        {
            if (_to == null || _from == null)
            {
                return;
            }
            if (_to.runtimeAnimatorController != _from.runtimeAnimatorController)
            {
                _to.runtimeAnimatorController = _from.runtimeAnimatorController;
            }
            SyncParams();
            SmoothFloats();
        }

        private void SyncParams()
        {
            foreach (var fromParam in _from.parameters)
            {
                switch (fromParam.type)
                {
                    case AnimatorControllerParameterType.Float:
                        if (!_floatParams.ContainsKey(fromParam.name))
                        {
                            DebugLog.ToConsole($"Warning - Tried to sync anim float that doesn't exist in dict : {fromParam.name}", MessageType.Warning);
                            RebuildFloatParams();
                            break;
                        }
                        _floatParams[fromParam.name].Target = _from.GetFloat(fromParam.name);
                        break;
                    case AnimatorControllerParameterType.Bool:
                        _to.SetBool(fromParam.name, _from.GetBool(fromParam.name));
                        break;
                }
            }
        }

        private void SmoothFloats()
        {
            foreach (var floatParam in _floatParams)
            {
                var current = floatParam.Value.Smooth(SmoothTime);
                _to.SetFloat(floatParam.Key, current);
            }
        }

        public void RebuildFloatParams()
        {
            if (_from.runtimeAnimatorController == null)
            {
                DebugLog.ToConsole($"Error - Controller of \"from\" for player {GetPlayer().PlayerId} is null! Current animtype is {GetPlayer().Animator.CurrentType}.", MessageType.Error);
            }
            _floatParams.Clear();
            DebugLog.DebugWrite($"REBUILD FLOAT PARAMS id {GetPlayer().PlayerId}");
            foreach (var param in _from.parameters.Where(p => p.type == AnimatorControllerParameterType.Float))
            {
                _floatParams.Add(param.name, new AnimFloatParam());
            }
        }
    }
}