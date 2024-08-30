using UnityEngine;
using Zenject;
namespace com.playbux
{
    public class PrefabToCompositeCollider2DFactory : PrefabToComponentFactory<CompositeCollider2D>
    {
        public PrefabToCompositeCollider2DFactory(DiContainer container) : base(container) { }
        public override void OnCreated(CompositeCollider2D component)
        {

        }
    }
}