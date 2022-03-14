using System;
using System.Collections;
using System.Collections.Generic;
using PlacementCriteria;
using UnityEngine;

public class CenterCriterion : PlacementCriterion
{

    public override float Score(RaycastHit hit)
    {
        if (Camera.main == null)
        {
            Debug.LogError("Camera not found");
            return 1f;
        }

        //in degrees
        float angle = Vector3.Angle(Camera.main.transform.forward,
            hit.point - Camera.main.transform.position);

        return 1f - (angle / 180f);

    }
}
