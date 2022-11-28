using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SunRotater : MonoBehaviour
{
    //public List<RoationInfo> rotationInfos = new List<RoationInfo>();

    public Transform rotationInfoParent;
    private List<Transform> rotationInfoList = new List<Transform>();
    Vector3 rotRef;
    private Quaternion targetRot;
    void Start()
    {
        targetRot = transform.rotation;
        Initiate();
    }
    private void Initiate() 
    {
        foreach (Transform t in rotationInfoParent) 
        {
            rotationInfoList.Add(t);
        }
    }

    Quaternion GetSelectedRotation(string transformName) 
    {
        return Array.Find(rotationInfoList.ToArray(), x => x.name == transformName).rotation;
    }
    private void OnEnable()
    {
        EventExecuter.OnRotateObject += StartRotate;
        EventExecuter.OnRotateObjectLerp += RotateLerp;
       
    }

    private void OnDisable()
    {
        EventExecuter.OnRotateObject -= StartRotate;
        EventExecuter.OnRotateObjectLerp -= RotateLerp;

    }
    private void Update()
    {
        //transform.eulerAngles = Vector3.SmoothDamp(transform.eulerAngles, targetRot, ref rotRef, 0.5f);
        transform.rotation = Utility.SmoothDampQuaternion(transform.rotation, targetRot, ref rotRef, 0.5f,100f,Time.deltaTime);
    }

    void StartRotate(string transformName, float time) 
    {
        StartCoroutine(RotateTo(time, GetSelectedRotation(transformName)));
    }

    void RotateLerp(string startTransform, string endTransform, float interpolate) 
    {
        targetRot = Quaternion.Slerp(GetSelectedRotation(startTransform), GetSelectedRotation(endTransform), interpolate);
    }

    IEnumerator RotateTo(float time, Quaternion target) 
    {
        float percent = 0;
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = target;
        while (percent < 1) 
        {
            percent += Time.deltaTime / time;
            targetRot = Quaternion.Slerp(currentRotation, targetRotation, percent);
            
            yield return null;
        }
    }

    [System.Serializable]
    public class RoationInfo 
    {
        public Transform target;
        public string name;
    }
}
