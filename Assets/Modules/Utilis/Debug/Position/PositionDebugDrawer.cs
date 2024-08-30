using Zenject;
using UnityEngine;
using UnityEngine.Assertions;
using com.playbux.utilis.canvas;
using com.playbux.utilis.uitext;
using System.Collections.Generic;

namespace com.playbux.utilis.debug
{
    public class PositionDebugDrawer : ILateTickable
    {
        private DebugCanvas canvas;

        private WorldToScreenText.Pool textPool;

        private Dictionary<Vector3, WorldToScreenText> positionTextDictionary = new();
        private Dictionary<Transform, WorldToScreenText> transformTextDictionary = new();

        public PositionDebugDrawer(DebugCanvas canvas, WorldToScreenText.Pool textPool)
        {
            this.canvas = canvas;
            this.textPool = textPool;
        }

        public void Draw(Transform transform)
        {
            var text = textPool.Spawn(transform.position, canvas.transform);
            Assert.IsFalse(transformTextDictionary.ContainsKey(transform));
            transformTextDictionary.Add(transform, text);
        }

        public void Draw(Vector3 position)
        {
            if (textPool.NumTotal > 5)
                Return();

            if (positionTextDictionary.ContainsKey(position))
                return;

            var text = textPool.Spawn(position, canvas.transform);
            Assert.IsFalse(positionTextDictionary.ContainsKey(position));
            positionTextDictionary.Add(position, text);
        }

        public void Return(Transform transform)
        {
            textPool.Despawn(transformTextDictionary[transform]);

            if (!transformTextDictionary.ContainsKey(transform))
                return;

            transformTextDictionary.Remove(transform);
        }

        public void Return()
        {
            foreach (var pair in transformTextDictionary)
            {
                textPool.Despawn(transformTextDictionary[pair.Key]);
            }

            transformTextDictionary.Clear();

            foreach (var pair in positionTextDictionary)
            {
                textPool.Despawn(positionTextDictionary[pair.Key]);
            }

            positionTextDictionary.Clear();
            textPool.Resize(5);
        }

        public void LateTick()
        {
            foreach (var pair in transformTextDictionary)
            {
                pair.Value.UpdatePosition(pair.Key.position);
            }

            foreach (var pair in positionTextDictionary)
            {
                pair.Value.UpdatePosition(pair.Key);
            }
        }
    }
}