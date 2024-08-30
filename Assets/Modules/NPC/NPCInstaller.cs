using com.playbux.tool;
using UnityEngine;
using Zenject;
using Mirror;
using com.playbux.networking.mirror.client.prop;

namespace com.playbux.npc
{
    [CreateAssetMenu(menuName = "Playbux/NPC/NPCInstaller", fileName = "NPCInstaller")]
    public class NPCInstaller : ScriptableObjectInstaller<NPCInstaller>
    {
        [SerializeField]
        private GameObject arrow;
        [SerializeField]
        private QuestData[] quests;
        [SerializeField]
        private NPCDataBase npcData;


        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<NPCModel>().AsSingle().NonLazy(); // Singleton binding
            Container.BindInstance(quests);
            Container.Bind<NPCDataBase>().FromInstance(npcData).AsSingle().NonLazy();
#if !SERVER
            Container.BindFactory<ArrowAngleController, ArrowAngleFactory>().FromComponentInNewPrefab(arrow).AsSingle();
            Container.Bind<INPCDirectionObserver>().To<NPCDirectionObserver>().AsSingle();
            Container.BindFactory<GameObject, Vector3, NPCSorter, NPCSorter.Factory>().FromFactory<NPCSorterFactory>();
            Container.Bind<NPCDisplayer>().AsSingle();
            

#endif
        }
    }
}
