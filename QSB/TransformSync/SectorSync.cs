using UnityEngine;
using QSB.Events;
using QSB.Utility;

namespace QSB.TransformSync
{
    public class SectorSync : MonoBehaviour
    {
        private float _checkTimer;
        private const float CheckInterval = 0.5f;

        private void Update()
        {
            if (!QSBSectorManager.Instance.IsReady)
            {
                return;
            }
            _checkTimer += Time.unscaledDeltaTime;
            if (_checkTimer < CheckInterval)
            {
                return;
            }
            PlayerRegistry.LocalTransformSyncs.ForEach(CheckTransformSyncSector);
            _checkTimer = 0;
        }

        private void CheckTransformSyncSector(TransformSync transformSync)
        {
            var syncedTransform = transformSync.SyncedTransform;
            if (syncedTransform == null ||
                syncedTransform.position == Vector3.zero ||
                syncedTransform.position == Locator.GetAstroObject(AstroObject.Name.Sun).transform.position)
            {
                return;
            }
            var closestSector = QSBSectorManager.Instance.GetClosestSector(syncedTransform);
            if (closestSector == transformSync.ReferenceSector)
            {
                return;
            }
            transformSync.ReferenceSector = closestSector;
            SendSector(transformSync.netId.Value, closestSector);
        }

        private void SendSector(uint id, QSBSector sector)
        {
            DebugLog.ToScreen($"Sending sector {sector.GOName} for id {id}");
            GlobalMessenger<uint, QSBSector>.FireEvent(EventNames.QSBSectorChange, id, sector);
        }
    }
}
