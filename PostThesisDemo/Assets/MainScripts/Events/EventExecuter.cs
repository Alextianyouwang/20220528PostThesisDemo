using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public abstract class EventExecuter : MonoBehaviour
{
    protected EventContainer ec;
    protected MarchManager marchManager;
    protected EventTrigger[] triggers;
    protected List<MarchManager.MarchObjectGroup> marchObjectGroups = new List<MarchManager.MarchObjectGroup>();

    public static Action<string, float> OnBGM;
    public static Action<float> OnBGMStop;
    public static Action<string, float> OnRotateObject;
    public static Action<string, string,float> OnRotateObjectLerp;
    public static Action<string, Vector3, AudioRolloffMode, float, int> OnPlaySoundSpacial;
    public enum ObjectActivationState { activate, unchange, deactivate };
    [HideInInspector]
    public ObjectActivationState objectActivationState;
    protected virtual void Start()
    {
        IOnSceneStart i = GetComponent<IOnSceneStart>();
        i?.StartScene();
    }

    protected virtual void OnEnable()
    {
        EventTrigger.OnEventCalled += ReceiveEventTrigger;
        EventTrigger.OnEventEnd += ReceiveEventEndTrigger;
    }
    protected virtual void OnDisable()
    {
        EventTrigger.OnEventCalled -= ReceiveEventTrigger;
        EventTrigger.OnEventEnd -= ReceiveEventEndTrigger;
    }
    public void Initialze()
    {
        triggers = FindObjectsOfType<EventTrigger>();
        marchManager = GetComponent<MarchManager>();
        marchObjectGroups = marchManager.marchObjectGroups;
        IOnSetMOGFeatures i = GetComponent<IOnSetMOGFeatures>();
        i?. SetMarchObjectGroupFeatures();
        ec = new EventContainer(this);
    }

    void MarchObjectGroupFormationCompletionCallBack(MarchManager.MarchObjectGroup currentMOG) 
    {
        IOnMOGComplete i = GetComponent<IOnMOGComplete>();
        i?.MarchObjectGroupFormationCompletionCallBack(currentMOG); 
    }

    protected IEnumerator SceneryReform(float startDelay,float lastingTime, float from, float to,
        MarchManager.MarchObjectGroup currentMOG,
       float waitTime, string eventName,float eventTime,bool combineMeshes ) 
    {
        yield return new WaitForSeconds(startDelay);
        float percentage = from;
        while (percentage < to) 
        {
            if (currentMOG != null) 
            {
                foreach (MarchManager.MarchObject m in currentMOG.marchObjects) 
                {
                    m.individualExternalControlValue = percentage;
                }
            }
            percentage += Time.deltaTime/lastingTime;
            yield return null;
        }
        MarchObjectGroupFormationCompletionCallBack(currentMOG);
        yield return new WaitForSeconds(waitTime);
        ec.InitiateSelectedEvent(eventName, eventTime,true);
        if (combineMeshes) 
        {
            yield return new WaitForSeconds(3f);
            currentMOG.CombineMeshes();
        }
    }
    protected void ReceiveEventTrigger(string eventName, float eventLastingTime,bool oneTimeActivation)
    {
        ec.InitiateSelectedEvent(eventName, eventLastingTime,oneTimeActivation);
        IOnEventTrigger i = GetComponent<IOnEventTrigger>();
        i?.QuickExecuteOnEventTriggered(eventName);
    }
    protected void ReceiveEventEndTrigger(string eventName)
    {
        ec.EndSelectedEvent(eventName);
    }
    public float GetLerpValueFromTrigger(string name) 
    {
       return Array.Find(triggers, x => x.name == name).lerpPerc;
    }
    public MarchManager.MarchObjectGroup FindMarchObjectGroup(string MOGName)
    {
        return marchObjectGroups.Find(x => x.name == MOGName);
    }
    public MarchManager.MarchObject FindMarchObject(string parentName, string name)
    {
        return FindMarchObjectGroup(parentName).marchObjects.Find(x => x.name == name);
    }
}
