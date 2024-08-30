using System;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;


namespace com.playbux.action
{
    public interface IAction
    {
        event Action Event;
    }

    public interface IActionArea
    {
        int UUID { get; }
        bool NeedInteractButton { get; }
        Transform transform { get; }
        void Initialize();
        void Dispose();
        void OnAreaEnter();
        bool Validate(Vector3 position);

        public class Factory : PlaceholderFactory<Vector2, Object, IActionArea>
        {

        }
    }

    public class ActionAreaFactory : IFactory<Vector2, Object, IActionArea>
    {
        private readonly DiContainer container;
        public ActionAreaFactory(DiContainer container)
        {
            this.container = container;
        }

        public IActionArea Create(Vector2 position, Object prefab)
        {
            return container.InstantiatePrefabForComponent<IActionArea>(prefab, position, Quaternion.Euler(0, 0, 45));
        }
    }
}