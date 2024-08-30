using TMPro;
using UnityEngine;
using com.playbux.ability;
using Zenject;

namespace com.playbux.networking.client.ability
{
    public class HotbarSlot : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI descText;

        private AbilityData data;

        public void ShowDesc()
        {
            if (data == null)
                return;

            descText.text = data.desc;
        }

        public void Assgin(AbilityData data)
        {
            this.data = data;
        }

        public void Unassign()
        {
            data = null;
        }

        public class Factory : PlaceholderFactory<Object, Transform, HotbarSlot>
        {

        }
    }
}