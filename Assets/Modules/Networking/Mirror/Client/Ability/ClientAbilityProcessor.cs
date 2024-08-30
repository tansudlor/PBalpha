using System;
using System.Collections.Generic;
using com.playbux.networking.mirror.message;

namespace com.playbux.networking.client.ability
{
    public class ClientAbilityProcessor
    {
        private Dictionary<uint, Action<StartCastMessage>> startCastDictionary = new Dictionary<uint, Action<StartCastMessage>>();
    }
}