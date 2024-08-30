using System;
using UnityEngine;

namespace com.playbux.map
{
    [Serializable]
    public class DepthPivotWrapper
    {
        public bool IgnoreSorting => ignoreSorting;
        public int IgnoreSortingOrder => ignoreSortingOrder;
        public Transform RootPivot => rootPivot;
        public Transform LeftPivot { get; private set; }
        public Transform RightPivot { get; private set; }
        public Transform[] ComparePoints => comparePoints;

        [SerializeField]
        private bool ignoreSorting;

        [SerializeField]
        private int ignoreSortingOrder;

        [SerializeField]
        private Transform rootPivot;

        [SerializeField]
        private Transform leftPivot;

        [SerializeField]
        private Transform rightPivot;

        [SerializeField]
        private Transform[] comparePoints;

        public void Initialize()
        {
            LeftPivot = leftPivot;
            RightPivot = rightPivot;
        }

        public void Swap()
        {
            RightPivot = leftPivot;
            LeftPivot = rightPivot;
        }
    }
}