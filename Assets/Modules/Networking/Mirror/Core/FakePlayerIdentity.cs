using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace com.playbux.networking.mirror.core
{
    [Serializable]
    public class FakePartCollection
    {
        public string key;
        public uint[] ids;
    }

    [Serializable]
    public class FakePlayerIdentity
    {
        public uint Id => networkIdentity.netId;
        public NetworkIdentity NetworkIdentity => networkIdentity;
        public TextMeshPro NameText => nameText;
        public TextMeshPro BackgroundNameText => backgroundNameText;
        public FakePartCollection HatCollection => hatCollection;
        public FakePartCollection FaceCollection => faceCollection;
        public FakePartCollection HeadCollection => headCollection;
        public FakePartCollection ShirtCollection => shirtCollection;
        public FakePartCollection PantsCollection => pantsCollection;
        public FakePartCollection ShoesCollection => shoesCollection;
        public FakePartCollection BackCollection => backCollection;

        public uint assetId;

        public string name;

        public int TimeToChangeEquipment => timeToChangeEquipment;

        [SerializeField]
        private int timeToChangeEquipment = 900;

        [SerializeField]
        private NetworkIdentity networkIdentity;

        [SerializeField]
        private TextMeshPro nameText;

        [SerializeField]
        private TextMeshPro backgroundNameText;

        [SerializeField]
        private FakePartCollection hatCollection;

        [SerializeField]
        private FakePartCollection faceCollection;

        [SerializeField]
        private FakePartCollection headCollection;

        [SerializeField]
        private FakePartCollection shirtCollection;

        [SerializeField]
        private FakePartCollection pantsCollection;

        [SerializeField]
        private FakePartCollection shoesCollection;

        [SerializeField]
        private FakePartCollection backCollection;
    }
}