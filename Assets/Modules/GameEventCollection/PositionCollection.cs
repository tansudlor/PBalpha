
using UnityEngine;

namespace com.playbux.gameeventcollection
{
    [CreateAssetMenu(menuName = "Playbux/GameEvent/PositionCollection", fileName = "PositionCollection")]
    public class PositionCollection : ScriptableObject
    {

        [SerializeField]
        Vector2[] positoions;

        [SerializeField]
        GameObject areaPrefab;

        [SerializeField]
        Color[] colors;

        public Vector2[] Positoions { get => positoions; set => positoions = value; }
        public GameObject AreaPrefab { get => areaPrefab; set => areaPrefab = value; }
        public Color[] Colors { get => colors; set => colors = value; }
    }
}
