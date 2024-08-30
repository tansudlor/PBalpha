using System.Collections.Generic;

namespace com.playbux.enemy
{
    public class EnemyEnmityList
    {
        public uint? TargetPlayer
        {
            get
            {

                if (list == null)
                    return null;

                if (list.Count <= 0)
                    return null;

                return list.GetEnumerator().Current.Key;
            }
        }

        private SortedDictionary<uint, int> list = new SortedDictionary<uint, int>();

        public void Add(uint playerNetId, int enmityGenerated)
        {
            if (list.ContainsKey(playerNetId))
            {
                list[playerNetId] += enmityGenerated;

                if (list[playerNetId] <= 0)
                    list.Remove(playerNetId);

                return;
            }

            list.Add(playerNetId, enmityGenerated);
        }

        public void Remove(uint playerNetId)
        {
            if (!list.ContainsKey(playerNetId))
                return;
            
            list.Remove(playerNetId);
        }
    }
}