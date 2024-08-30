using Zenject;
using UnityEngine;

namespace com.playbux.networking.server.ability
{
    public class ServerAbilityFacade : MonoBehaviour
    {
        public IServerAbility Ability => ability;

        private IServerAbility ability;

        [Inject]
        private void Construct(IServerAbility ability)
        {
            this.ability = ability;
        }
    }
}