using System;
using UnityEngine;
namespace com.playbux.map
{
    [Serializable]
    public class PropDataWrapper
    {
        public string name;
        public bool flip;
        public Vector3 scale;
        public Vector2 position;
        public Vector2 offsetPostion;
    }

    [Serializable]
    public class PropDataCollection
    {
        public PropDataWrapper[] data;
    }
}