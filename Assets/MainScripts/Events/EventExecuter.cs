using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public abstract class EventExecuter : MonoBehaviour
{
    protected EventTrigger[] triggers;
    protected MarchManager marchManager;
    protected List<MarchManager.MarchObjectGroup> marchObjectGroups = new List<MarchManager.MarchObjectGroup>();

    public static Action<string, float> OnBGM;
    public static Action<float> OnBGMStop;
    public static Action<string, float> OnRotateObject;
    public static Action<string, string,float> OnRotateObjectLerp;
    public static Action<string, Vector3, AudioRolloffMode, float, int> OnPlaySoundSpacial;
    public static Action<string> OnEnablePlayArea;
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
        MyEventsDispatcher.OnEnterEvent += EnterEvent;
        MyEventsDispatcher.OnExitEvent += ExitEvent;
    }
    protected virtual void OnDisable()
    {
        EventTrigger.OnEventCalled -= ReceiveEventTrigger;
        EventTrigger.OnEventEnd -= ReceiveEventEndTrigger;
        MyEventsDispatcher.OnEnterEvent -= EnterEvent;
        MyEventsDispatcher.OnExitEvent -= ExitEvent;
    }
    public void Awake()
    {
        MyEventsDispatcher.eventList = EventContainer.GetAllEvents();
    }
    public void Initialze()
    {
        triggers = FindObjectsOfType<EventTrigger>();
        marchManager = GetComponent<MarchManager>();
        marchObjectGroups = marchManager.marchObjectGroups;
        IOnSetMOGFeatures i = GetComponent<IOnSetMOGFeatures>();
        i?. SetMarchObjectGroupFeatures();
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
        MyEventsDispatcher.InitiateSelectedEvent(eventName, eventTime,true);
        if (combineMeshes) 
        {
            yield return new WaitForSeconds(3f);
            currentMOG.CombineMeshes();
        }
    }
    protected void ReceiveEventTrigger(string eventName, float eventLastingTime,bool oneTimeActivation)
    {
        MyEventsDispatcher.InitiateSelectedEvent(eventName, eventLastingTime,oneTimeActivation);
    }
    protected void ReceiveEventEndTrigger(string eventName)
    {
        MyEventsDispatcher.EndSelectedEvent(eventName);
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
    public void EnterEvent(string eventName) 
    {
        IOnEventEnter i = GetComponent<IOnEventEnter>();
        i?.QuickExecuteOnEnterEvent(eventName);
    }
    public void ExitEvent(string eventName)
    {
        IOnEventExit i = GetComponent<IOnEventExit>();
        i?.QuickExecuteOnExitEvent(eventName);
    }
}
