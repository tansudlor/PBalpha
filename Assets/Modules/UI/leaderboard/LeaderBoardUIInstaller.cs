using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace com.playbux.ui.leaderboard
{
    [CreateAssetMenu(menuName = "Playbux/UI/LeaderboardUI", fileName = "LeaderboardUIInstaller")]
    public class LeaderBoardUIInstaller : ScriptableObjectInstaller<LeaderBoardUIInstaller>
    {

        public GameObject LeaderBoardUIPrefab;

        private UICanvas canvas;
        [Inject]
        void Setup(UICanvas canvas)
        {
            this.canvas = canvas;

        }
        public override void InstallBindings()
        {
#if !SERVER
            BindClientSide();
#endif
        }
#if !SERVER
        private void BindClientSide()
        {

            Container.Bind<LeaderBoardUI>()
               .FromComponentInNewPrefab(LeaderBoardUIPrefab)
               .UnderTransform(canvas.transform)
               .AsSingle().NonLazy();


        }
#endif
    }
}
