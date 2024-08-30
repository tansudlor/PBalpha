using System;
using UnityEngine;

namespace com.playbux.networking.mirror.core
{
    [Serializable]
    public class CommandInstruction
    {
        public string Name => name;
        public string AltName => altName;
        public ParameterType[] Parameters => parameters;

        [SerializeField]
        private string name;

        [SerializeField]
        private string altName;

        [SerializeField]
        private ParameterType[] parameters;
        public CommandInstruction(string name, string altName, params ParameterType[] parameters)
        {
            this.name = name;
            this.altName = altName;
            this.parameters = parameters;
        }
    }
}