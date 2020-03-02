using System;
using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Animation
{
    public class QSBNetAnim : NetworkAnimator
    {
        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            var anim = this.GetValue<Animator>("m_Animator");
            if (anim == null || anim.parameters == null || anim.parameters.Length == 0)
            {
                return;
            }
            for (int index = 0; index < anim.parameters.Length; ++index)
            {
                if (!this.GetValue<bool>("autoSend") || this.GetParameterAutoSend(index))
                {
                    AnimatorControllerParameter parameter = anim.parameters[index];
                    if (parameter.type == AnimatorControllerParameterType.Int)
                    {
                        int num = (int)reader.ReadPackedUInt32();
                        anim.SetInteger(parameter.nameHash, num);
                        this.Invoke("SetRecvTrackingParam", parameter.name + ":" + (object)num, index);
                    }

                    if (parameter.type == AnimatorControllerParameterType.Float)
                    {
                        float num;
                        try
                        {
                            num = reader.ReadSingle();
                        }
                        catch (Exception ex)
                        {
                            //DebugLog.Screen($"Error when reading float {parameter.name}: " + ex);
                            return;
                        }
                        anim.SetFloat(parameter.nameHash, num);
                        this.Invoke("SetRecvTrackingParam", parameter.name + ":" + (object)num, index);
                    }

                    if (parameter.type == AnimatorControllerParameterType.Bool)
                    {
                        bool flag = reader.ReadBoolean();
                        anim.SetBool(parameter.nameHash, flag);
                        this.Invoke("SetRecvTrackingParam", parameter.name + ":" + (object)flag, index);
                    }
                }
            }
        }

    }
}
