using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EventTrigger : MonoBehaviour
{
    public float triggerRadius;
    public enum Mode { circleRange, boxRange };
    public Mode triggerMode;
    public enum AlignAxis {X,Y,Z};
    public AlignAxis alignAxis;
    public enum ExitCondition { Null,One, Zero };
    public ExitCondition exitCondition;
    public Vector3 triggerDimentions;
    private bool hasBeenTriggered;
    public Color triggerIndicatorColor = Color.red;
    private PlayerInteraction player;
    public string eventName;
    public float eventLastingTime;
    public bool oneTimeActivation = true;
    public float lerpPerc;
    
    private Coroutine rangeCheckCo;
    private BoxCollider triggerCollider;
  
    public static event Action<string,float,bool> OnEventCalled;
    public static event Action<string> OnEventEnd;

    void Start()
    {
        player = FindObjectOfType<PlayerInteraction>();
        if (triggerMode == Mode.boxRange) 
        {
            triggerCollider = gameObject.AddComponent<BoxCollider>();
            triggerCollider.size = triggerDimentions;
            triggerCollider.center = triggerDimentions / 2;
            triggerCollider.isTrigger = true;
        }
    }
    
    void Update()
    {
        if (triggerMode == Mode.circleRange)
        {
            if (Vector3.SqrMagnitude(player.transform.position - transform.position) < Mathf.Pow(triggerRadius, 2) &&
            !hasBeenTriggered)
            {
                hasBeenTriggered = true;
                OnEventCalled?.Invoke(eventName, eventLastingTime,oneTimeActivation);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = triggerIndicatorColor;
        if (triggerMode == Mode.circleRange)
        {
            Gizmos.DrawWireSphere(transform.position, triggerRadius);
        }
        else 
        {
            Gizmos.DrawWireCube(transform.position + triggerDimentions / 2, triggerDimentions);
        }
    }

    IEnumerator RangeChecker() 
    {
        float distanceRef = 1;
        while (true) 
        {
            Vector3 localPosition = transform.InverseTransformPoint(player.transform.position);
            switch (alignAxis)
            {
                case AlignAxis.X:
                    distanceRef = localPosition.x / (triggerDimentions.x);
                    break;
                case AlignAxis.Y:
                    distanceRef = localPosition.y / (triggerDimentions.y);
                    break;
                case AlignAxis.Z:
                    distanceRef = localPosition.z / (triggerDimentions.z);
                    break;
            }
            lerpPerc = Mathf.InverseLerp(1, 0, distanceRef);
            yield return null;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player")) 
        {
            OnEventCalled?.Invoke(eventName, eventLastingTime,oneTimeActivation);
            rangeCheckCo = StartCoroutine(RangeChecker());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Player")) 
        {
            if (rangeCheckCo != null)
            {
                StopCoroutine(rangeCheckCo);
            }
                Vector3 localPosition = transform.InverseTransformPoint(player.transform.position);
            if (exitCondition != ExitCondition.Null) 
            {
                switch (alignAxis)
                {
                    case AlignAxis.X:
                        if (exitCondition == ExitCondition.Zero)
                        {
                            if (localPosition.x > triggerDimentions.x / 2)
                            {
                                OnEventEnd?.Invoke(eventName);
                            }
                        }
                        else if (exitCondition == ExitCondition.One)
                        {
                            if (localPosition.x < triggerDimentions.x / 2)
                            {
                                OnEventEnd?.Invoke(eventName);
                            }
                        }
                        break;
                    case AlignAxis.Y:
                        if (exitCondition == ExitCondition.Zero)
                        {
                            if (localPosition.y > triggerDimentions.y / 2)
                            {
                                OnEventEnd?.Invoke(eventName);
                            }
                        }
                        else if (exitCondition == ExitCondition.One)
                        {
                            if (localPosition.y < triggerDimentions.y / 2)
                            {
                                OnEventEnd?.Invoke(eventName);
                            }
                        }
                        break;
                    case AlignAxis.Z:
                        if (exitCondition == ExitCondition.Zero)
                        {
                            if (localPosition.z > triggerDimentions.z / 2)
                            {
                                OnEventEnd?.Invoke(eventName);
                            }
                        }
                        else if (exitCondition == ExitCondition.One)
                        {
                            if (localPosition.z < triggerDimentions.z / 2)
                            {
                                OnEventEnd?.Invoke(eventName);
                            }
                        }
                        break;
                }
            }
            else 
            {
                OnEventEnd?.Invoke(eventName);
            }
        }
    }
}
