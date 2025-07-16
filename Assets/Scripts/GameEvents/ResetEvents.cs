using UnityEngine;

namespace GameEvents
{
    public struct LevelResetEvent
    {
        public string LevelName;
        public float Timestamp;
    }

    public struct PlayerRespawnEvent
    {
        public Vector3 SpawnPosition;
        public float Timestamp;
    }
}
