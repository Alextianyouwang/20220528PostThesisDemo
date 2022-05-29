using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility
{

    public static bool IsVisibleFromCamera(Camera cam, GameObject target,bool hardTest) 
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        foreach (Plane p in planes) 
        {
            if (p.GetDistanceToPoint(target.transform.position)<0) 
            {

                return false;
            }
        }
        if (hardTest)
        {
            if (target.GetComponent<Renderer>().isVisible)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else 
        {
            return true;
        }
      
    }
    public static bool IsPositionInCamera(Camera cam, Vector3 pos)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        foreach (Plane p in planes)
        {
            if (p.GetDistanceToPoint(pos) < 0)
            {

                return false;
            }
        }
        return true;
        
    }

    public static Quaternion SmoothDampQuaternion(Quaternion current, Quaternion target, ref Vector3 currentVelocity, float smoothTime,float maxSpeed, float deltaTime)
    {
        Vector3 c = current.eulerAngles;
        Vector3 t = target.eulerAngles;
        return Quaternion.Euler(
          Mathf.SmoothDampAngle(c.x, t.x, ref currentVelocity.x, smoothTime,maxSpeed,deltaTime),
          Mathf.SmoothDampAngle(c.y, t.y, ref currentVelocity.y, smoothTime,maxSpeed,deltaTime),
          Mathf.SmoothDampAngle(c.z, t.z, ref currentVelocity.z, smoothTime,maxSpeed,deltaTime)
        );
    }

    public static float Remap( float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
