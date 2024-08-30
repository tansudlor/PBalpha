using AYellowpaper.SerializedCollections;
using JetBrains.Annotations;
using UnityEngine;
namespace com.playbux.ability
{
    [CreateAssetMenu(menuName = "Playbux/Ability/Create ItemAbilityDatabase", fileName = "ItemAbilityDatabase", order = 0)]
    public class ItemAbilityDatabase : ScriptableObject
    {
        public AbilityDatabase AbilityDatabase => abilityDatabase;

        [SerializeField]
        private AbilityDatabase abilityDatabase;

        [SerializeField]
        private SerializedDictionary<uint, uint[]> itemToAbilityIds = new SerializedDictionary<uint, uint[]>();

        public bool HasKey(uint id)
        {
            return itemToAbilityIds.ContainsKey(id);
        }

        [CanBeNull]
        public AbilityData[] Get(uint id)
        {
            if (!HasKey(id))
                return null;

            uint[] ids = itemToAbilityIds[id];
            AbilityData[] datas = new AbilityData[ids.Length];

            for (int i = 0; i < datas.Length; i++)
            {
                if (!abilityDatabase.HasKey(ids[i]))
                    continue;

                datas[i] = abilityDatabase.Get(ids[i]);
            }

            return datas;
        }

#if UNITY_EDITOR
        public void Add(uint id, uint[] abilityIds)
        {
            itemToAbilityIds.Add(id, abilityIds);
        }

        public void Remove(uint id)
        {
            itemToAbilityIds.Remove(id);
        }
#endif
    }
}