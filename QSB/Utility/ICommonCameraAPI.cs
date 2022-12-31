using UnityEngine;
using UnityEngine.Events;

namespace QSB.Utility;

public interface ICommonCameraAPI
{
    void RegisterCustomCamera(OWCamera OWCamera);
    (OWCamera, Camera) CreateCustomCamera(string name);
    UnityEvent<PlayerTool> EquipTool();
    UnityEvent<PlayerTool> UnequipTool();
}
