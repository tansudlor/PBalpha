using Zenject;
using UnityEngine;
using com.playbux.entities;

namespace com.playbux.networking.mirror.core
{
    public class NetworkEnemyFactory : PlaceholderFactory<GameObject, Vector3, IEntity<EnemyIdentity>>
    {

    }

    public class NetworkFakePlayerFactory : PlaceholderFactory<GameObject, Vector3, IEntity<FakePlayerIdentity>>
    {
        
    }
}