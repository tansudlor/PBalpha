using UnityEngine;
using Zenject;
namespace com.playbux
{
    public class PrefabToSpriteRendererFactory : PrefabToComponentFactory<SpriteRenderer>
    {
        public PrefabToSpriteRendererFactory(DiContainer container) : base(container) { }
        public override void OnCreated(SpriteRenderer component)
        {

        }
    }
}
