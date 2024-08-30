using System;
using UnityEngine;
namespace com.playbux.zone
{
    [Serializable]
    public struct ZoneAsset
    {
        public GameObject prefab;
        public Collider2D collider;
    }
}