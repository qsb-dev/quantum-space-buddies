using OWML.ModHelper.Events;
using UnityEngine;

namespace QSB.TimeSync
{
    public class PreventShipDestruction : MonoBehaviour
    {
        private ShipModule[] _shipModules;

        private void Awake()
        {
            QSB.Helper.HarmonyHelper.EmptyMethod<ShipDamageController>("OnImpact");
            QSB.Helper.HarmonyHelper.EmptyMethod<ShipDamageController>("Explode");

            QSB.Helper.Events.Subscribe<ShipDamageController>(OWML.Common.Events.AfterAwake);
            QSB.Helper.Events.OnEvent += OnEvent;
        }

        private void OnEvent(MonoBehaviour behaviour, OWML.Common.Events ev)
        {
            if (behaviour.GetType() == typeof(ShipDamageController) && ev == OWML.Common.Events.AfterAwake)
            {
                _shipModules = behaviour.GetValue<ShipModule[]>("_shipModules");
                var impactSensor = behaviour.GetValue<ImpactSensor>("_impactSensor");
                impactSensor.OnImpact += OnImpact;
            }
        }

        private void OnImpact(ImpactData impact)
        {
            if (impact.otherCollider.attachedRigidbody != null && impact.otherCollider.attachedRigidbody.CompareTag("Player") && PlayerState.IsInsideShip())
            {
                return;
            }
            foreach (var module in _shipModules)
            {
                module.ApplyImpact(impact);
            }
        }

    }
}
