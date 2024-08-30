using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace com.playbux.ui.leaderboard
{
    public class LeaderBoardUIWindowInstaller : MonoInstaller<LeaderBoardUIWindowInstaller>
    {
        // Start is called before the first frame update
        public override void InstallBindings()
        {
            Container.Bind<LeaderBoardUI>().FromComponentOn(gameObject).AsSingle();
        }
    }
}
