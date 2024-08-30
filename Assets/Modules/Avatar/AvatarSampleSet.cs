using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace com.playbux.avatar
{
    [CreateAssetMenu(fileName = "AvatarSampleSet", menuName = "Avatar/SetList")]
    public class AvatarSampleSet : ScriptableObject
    {
        [TextArea(11, 11)]
        public string[] slotData;
    }
}
