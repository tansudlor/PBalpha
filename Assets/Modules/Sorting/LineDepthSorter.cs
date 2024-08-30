using System;
using UnityEngine;
using com.playbux.map;
using UnityEngine.Assertions;

namespace com.playbux.sorting
{
    public class LineSortable : ISortable
    {
        public bool IgnoreSorting => pivotWrapper.IgnoreSorting;
        public Vector3 Position => pivotWrapper.RootPivot == null ? mainObject.position : pivotWrapper.RootPivot.position;

        private readonly Transform mainObject;
        private readonly ISortComponent sortComponent;
        private readonly DepthPivotWrapper pivotWrapper;
        private Vector2[] comparingPoints;

        private const int DEPTH_OFFSET_MULTIPLIER = 100;

        private float slope;
        private float yIntercept;
        private int sortingOrder;

        public LineSortable(
            Transform mainObject,
            ISortComponent sortComponent,
            DepthPivotWrapper pivotWrapper)
        {
            this.mainObject = mainObject;
            this.pivotWrapper = pivotWrapper;
            this.sortComponent = sortComponent;

            Assert.IsNotNull(this.mainObject);
        }

        public void Initialize()
        {
            pivotWrapper.Initialize();

            if (mainObject.localScale.x < 0)
                pivotWrapper.Swap();

            if (!pivotWrapper.IgnoreSorting)
            {
                Assert.IsNotNull(pivotWrapper.LeftPivot);
                Assert.IsNotNull(pivotWrapper.RightPivot);
            }

            comparingPoints = new Vector2[3 + (pivotWrapper.ComparePoints == null ? 0 : pivotWrapper.ComparePoints.Length)];
            comparingPoints[0] = pivotWrapper.RootPivot == null ? mainObject.position : pivotWrapper.RootPivot.position;
            comparingPoints[1] = pivotWrapper.LeftPivot.transform.position;
            comparingPoints[2] =pivotWrapper.RightPivot.transform.position;

            for (int i = 0; i < pivotWrapper.ComparePoints.Length; i++)
            {
                comparingPoints[i + 3] = pivotWrapper.ComparePoints[i].position;
            }

            int calculatedOrder = Mathf.RoundToInt((pivotWrapper.RootPivot == null ? mainObject.position.y : pivotWrapper.RootPivot.position.y) * DEPTH_OFFSET_MULTIPLIER);
            calculatedOrder *= -1;

            sortingOrder = pivotWrapper.IgnoreSorting ? pivotWrapper.IgnoreSortingOrder : calculatedOrder;
            sortComponent.Sort(sortingOrder);

            float deltaY = pivotWrapper.RightPivot.position.y - pivotWrapper.LeftPivot.position.y;
            float deltaX = pivotWrapper.RightPivot.position.x - pivotWrapper.LeftPivot.position.x;

            slope = deltaY / deltaX;
            yIntercept = pivotWrapper.LeftPivot.position.y - slope * pivotWrapper.LeftPivot.position.x;

            Assert.IsFalse(deltaX == 0);
        }

        public Vector2? Distance(Vector2 movingObjectPosition)
        {
            int closestIndex = -1;
            float distance = float.PositiveInfinity;
            for (int i = 0; i < comparingPoints.Length; i++)
            {
                float delta = Vector2.Distance(comparingPoints[i], movingObjectPosition);

                if (delta < distance)
                {
                    distance = delta;
                    closestIndex = i;
                }
            }

            if (closestIndex < 0)
                return null;

            return comparingPoints[closestIndex];
        }

        public int GetSortOrder(Vector2 movingObjectPosition)
        {
            // Calculate the y position on the line at the GameObject's x position
            float lineYAtObjectX = slope * movingObjectPosition.x + yIntercept;

            // Check if the GameObject's y position is higher than the line's y at the same x
            if (movingObjectPosition.y > lineYAtObjectX)
                return sortingOrder - 1;

            return sortingOrder + 1;
        }
    }
}