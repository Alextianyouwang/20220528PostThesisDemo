using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EventContainer
{
    private EventExecuter caller;
    public Event currentEvent;
    public List<Event> eventList = new List<Event>();
    public static event Action OnPassiveActivateInteraction;
    public static event Action<float, float, float> OnChangePlayerAffectRadius;
    public static event Action<bool> OnDisableEntropyReduce;
    public static event Action<bool> OnTogglePlayerInteraction;

    public static event Action OnSphereLerp;
    public static event Action OnSphereLerpPrepare;
    public static event Action OnSphereLerpExit;
    public EventContainer(EventExecuter _eventCaller) 
    {
        caller = _eventCaller;
        eventList = GetAllEvents();
    }
    public List<Event>  GetAllEvents()
    {
        List<Event> allEvents = new List<Event>{
            new Event("AfterAssembleNarwhal",new Action[]{ScanAndUnveilObject_Fast},null,new Action[]{ReadyToFreeMove}),
            new Event("SphereLerp",new Action[]{SphereLerpPrepare},SphereLerp,null),
            new Event("EndDemo",new Action[]{ScanAndUnveilObject_Fast,SphereLerpExit},null,new Action[]{ReadyToFreeMove}),
        };
        return allEvents;
    }
    public void SphereLerp() 
    {
        OnSphereLerp?.Invoke();
    }
    public void SphereLerpPrepare() 
    {
        OnSphereLerpPrepare?.Invoke();
    }
    public void SphereLerpExit() 
    {
        OnSphereLerpExit?.Invoke();
    }
    public void ScanAndUnveilObject_Fast()
    {
        OnDisableEntropyReduce?.Invoke(true);
        OnChangePlayerAffectRadius?.Invoke(0f, 600f, 10f);
        OnPassiveActivateInteraction?.Invoke();
    }
    public void ReadyToFreeMove()
    {
        OnChangePlayerAffectRadius?.Invoke(20f, 50f, 1.5f);
        OnDisableEntropyReduce?.Invoke(false);
        OnTogglePlayerInteraction?.Invoke(false);
    }
    public void InitiateSelectedEvent(string name, float autoTriggerTimer, bool oneTimeActivation) 
    { 
        Event selectedEvent = eventList.Find(x => x.name == name);
        if (selectedEvent == null || selectedEvent.hasActivated)
            return;
        currentEvent = selectedEvent;
        selectedEvent.isInprogress = true;
        caller.StartCoroutine(LoadCurrentEventAction(selectedEvent, autoTriggerTimer));
        if (oneTimeActivation)
            selectedEvent.hasActivated = true;
    }
    public void EndSelectedEvent(string name) 
    {
        Event selectedEvent = eventList.Find(x => x.name == name);
        selectedEvent.isInprogress = false;
    }
    public IEnumerator LoadCurrentEventAction(Event currentEvent, float waitTime)
    {
        float time = 0;
        if (currentEvent.StartEventActions != null) 
            foreach (Action a in currentEvent.StartEventActions)
                a?.Invoke();
        if (waitTime != 0) 
            while (waitTime > 60? currentEvent.isInprogress: currentEvent.isInprogress && time < waitTime)
            {
                time += Time.deltaTime;
                currentEvent.OnEventAction?.Invoke();
                yield return null;
            }
        if (currentEvent.EndEventActions != null) 
            foreach (Action a in currentEvent.EndEventActions)
                a?.Invoke();
        currentEvent.isInprogress = false;
        currentEvent.hasEnd = true;
    }
    public class Event
    {
        public string name;
        public Action[] StartEventActions;
        public Action OnEventAction;
        public Action[] EndEventActions;
        public bool hasActivated = false;
        public bool isInprogress = false;
        public bool hasEnd = false;
        public Event(string _name,Action[] _StartEventAction,Action _OnEventAction,Action[] _EndEventAction) 
        {
            name = _name;
            StartEventActions = _StartEventAction;
            OnEventAction = _OnEventAction;
            EndEventActions = _EndEventAction;
        }

    }
}
