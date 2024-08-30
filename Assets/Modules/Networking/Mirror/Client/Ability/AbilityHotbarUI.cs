using UnityEngine;
using com.playbux.ability;

namespace com.playbux.networking.client.ability
{
    public class AbilityHotbarUI : MonoBehaviour
    {
        private HotbarSlot prefab;
        private Transform container;
        private AbilityDatabase database;
        private HotbarSlot.Factory slotFactory;

        private HotbarSlot[] slots;

        private void Start()
        {
            slots = new HotbarSlot[8];
            // inventory.OnHotbarChanged += OnHotbarChanged;
        }

        private void OnHotbarChanged(uint[] abilityIds)
        {
            for (int i = 0; i < abilityIds.Length; i++)
            {
                slots[i] ??= slotFactory.Create(prefab, container);
                var abilityData = database.Get(abilityIds[i]);
                slots[i].Assgin(abilityData);
            }
        }
    }
}