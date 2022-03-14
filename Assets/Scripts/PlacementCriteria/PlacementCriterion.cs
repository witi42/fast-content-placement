using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlacementCriteria
{
    
    public abstract class PlacementCriterion : MonoBehaviour
    {
        public virtual float Score(RaycastHit hit)
        {
            return 1f;
        }

        public virtual float Score(RaycastHit hit, MyTriangulation triangulation)
        {
            return Score(hit);
        }
    }
}

