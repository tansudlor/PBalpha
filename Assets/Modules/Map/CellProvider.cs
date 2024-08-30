using UnityEngine;
using System.Collections.Generic;

namespace com.playbux.map
{
    public class CellProvider
    {
        private readonly RuntimeCell.Pool pool;
        public CellProvider(RuntimeCell.Pool pool)
        {
            this.pool = pool;
        }

        public RuntimeCell Get(Vector3 position, Sprite sprite)
        {
            var instance = pool.Spawn(position, sprite);
            return instance;
        }

        public void Return(RuntimeCell cell)
        {
            pool.Despawn(cell);
        }
    }
}