using UnityEngine;

namespace Marshmallow.General
{
    static class MakeSpawnPoint
    {
        public static SpawnPoint Make(GameObject body, Vector3 position)
        {
            GameObject spawn = new GameObject();
            spawn.transform.parent = body.transform;
            spawn.layer = 8;

            spawn.transform.localPosition = position;

            //QSB.DebugLog.Console("Made spawnpoint on [" + body.name + "] at " + position);

            return spawn.AddComponent<SpawnPoint>();
        }
    }
}
