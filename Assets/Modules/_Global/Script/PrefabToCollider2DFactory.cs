using UnityEngine;
using Zenject;
namespace com.playbux
{
    public class PrefabToCollider2DFactory : PrefabToComponentFactory<Collider2D>
    {
        public PrefabToCollider2DFactory(DiContainer container) : base(container) { }
        public override void OnCreated(Collider2D component)
        {

        }
    }
}