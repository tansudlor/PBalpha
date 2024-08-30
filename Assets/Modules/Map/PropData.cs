using System;
using UnityEngine;

namespace com.playbux.map
{
    [Serializable]
    public struct PropData
    {
        public GameObject propObject;
        public Collider2D[] propCollider;
    }
}