using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace com.playbux.ui.bubble
{
    public enum BubbleChannel
    {
        Default = 0,
        Chat,
        Interaction
    }

    public class BubbleController<TPool> where TPool : MemoryPool<string, Vector3, Transform, IBubble>
    {
        private readonly TPool pool;
        private readonly BubbleContainer container;

        private List<IBubble> bubbles = new List<IBubble>();

        public BubbleController(TPool pool, BubbleContainer container)
        {
            this.pool = pool;
            this.container = container;
        }

        public IBubble GetBubble(string message, Vector3 position, BubbleChannel channel)
        {
            int intChannel = (int)channel;
            if (intChannel >= container.Channels.Length)
                intChannel = 0;

            var bubble = pool.Spawn(message, position, container.Channels[intChannel]);
            bubbles.Add(bubble);
            return bubble;
        }

        public void ReturnBubble(IBubble bubble)
        {
            bubbles.Remove(bubble);
            pool.Despawn(bubble);
        }
    }
}