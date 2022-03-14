using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlacementCriteria
{
    public class PositionCriterion : PlacementCriterion
    {
        [SerializeField] private float maxX = Mathf.Infinity;
        [SerializeField] private float minX = Mathf.NegativeInfinity;
        [SerializeField] private float maxY = Mathf.Infinity;
        [SerializeField] private float minY = Mathf.NegativeInfinity;
        [SerializeField] private float maxZ = Mathf.Infinity;
        [SerializeField] private float minZ = Mathf.NegativeInfinity;
    
    
        public override float Score(RaycastHit hit)
        {
            Vector3 position = hit.point;
            if (position.x <= maxX && position.x >= minX && position.y <= maxY && position.y >= minY &&
                position.z <= maxZ && position.z >= minZ)
                return 1f;
            return 0f;
        }
    }
}

