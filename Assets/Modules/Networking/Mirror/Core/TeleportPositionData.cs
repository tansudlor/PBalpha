using System;
using UnityEngine;

namespace com.playbux.networking.mirror.core
{
    [Serializable]
    public struct TeleportPositionData
    {
        public Vector2 areaPosition;
        public Vector2 targetPosition;
        public TeleportableArea areaPrefab;
    }
}