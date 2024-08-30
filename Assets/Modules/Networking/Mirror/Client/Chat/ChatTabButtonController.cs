using System;
using System.Linq;
using UnityEngine;
using Zenject;
namespace com.playbux.networking.mirror.client.chat
{

    public class ChatTabButtonController : IInitializable, IDisposable
    {
        public ChatTab[] Tabs => tabs;

        private readonly ChatTab.Pool pool;
        private readonly Transform tabContainer;
        private readonly ChatTabSettings[] settings;
        
        private ChatTab[] tabs;

        public ChatTabButtonController(Transform tabContainer, ChatTab.Pool pool, ChatTabSettings[] settings)
        {
            this.pool = pool;
            this.settings = settings;
            this.tabContainer = tabContainer;
            tabs = new ChatTab[this.settings.Length];
        }
        public void Initialize()
        {
            for (int i = 0; i < settings.Length; i++)
            {
                ChatTab tab = pool.Spawn(settings[i].ableToRemove, (short)i, tabContainer, settings[i], DespawnButton);
                tabs[i] = tab;
            }
        }
        public void Dispose()
        {
            pool.Clear();
            tabs = null;
        }

        private void DespawnButton(int index)
        {
            Debug.Log(index);
            pool.Despawn(tabs[index]);
        }
    }
}