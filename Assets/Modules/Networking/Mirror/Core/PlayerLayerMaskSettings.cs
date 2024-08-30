using System;
using UnityEngine;

namespace com.playbux.networking.mirror.core
{
    [Serializable]
    public struct PlayerLayerMaskSettings
    {
        public LayerMask colliderMask;
        public LayerMask teleportMask;
    }
}