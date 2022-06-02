using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

// A Container Class Responsible for Immediate Scene Actions.
public static class EventContainer
{
    public static event Action OnPassiveActivateInteraction;
    public static event Action<float, float, float> OnChangePlayerAffectRadius;
    public static event Action<bool> OnDisableEntropyReduce;
    public static event Action<bool> OnTogglePlayerInteraction;

    public static event Action OnSphereLerp;
    public static event Action OnSphereLerpPrepare;
    public static event Action OnSphereLerpExit;

    public static List<MyEvent> GetAllEvents()
    {
        List<MyEvent> allEvents = new List<MyEvent>{
            new MyEvent(
                "AfterAssembleNarwhal",
                new IAction[]{new ToggleEntropyReduce(true), new ChangePlayerEffectRadius(0f,400f,12f), new PassiveActivateInteraction()},
                null,
                new IAction[]{new ToggleEntropyReduce(false), new ChangePlayerEffectRadius(20f, 50f, 1.5f), new TogglePlayerInteraction(false)}),

            new MyEvent(
                "SphereLerp",
                new IAction[]{new SphereLerpPrepare()},
                new SphereLerp(),
                null),

            new MyEvent(
                "EndDemo",
                new IAction[]{new ToggleEntropyReduce(true), new ChangePlayerEffectRadius(0f,1000f,12f), new PassiveActivateInteraction(),new SphereLerpExit()},
                null,
                new IAction[]{ new ToggleEntropyReduce(false), new ChangePlayerEffectRadius(20f, 50f, 1.5f), new TogglePlayerInteraction(false)}
                )
       };
        return allEvents;
    }
    private class ChangePlayerEffectRadius : IAction
    {
        float innerRadius, outerRadius, lastingTime;
        public ChangePlayerEffectRadius(float _inner, float _outer, float _time)
        {
            innerRadius = _inner;
            outerRadius = _outer;
            lastingTime = _time;
        }
        public void Invoke() => OnChangePlayerAffectRadius.Invoke(innerRadius, outerRadius, lastingTime);
    }
    private class ToggleEntropyReduce : IAction
    {
        bool state;
        public ToggleEntropyReduce(bool _state)
        {
            state = _state;
        }
        public void Invoke() => OnDisableEntropyReduce.Invoke(state);
    }
    private class TogglePlayerInteraction : IAction
    {
        bool state;
        public TogglePlayerInteraction(bool _state)
        {
            state = _state;
        }
        public void Invoke() => OnTogglePlayerInteraction.Invoke(state);
    }
    private class PassiveActivateInteraction : IAction
    {
        public PassiveActivateInteraction() { }
        public void Invoke() => OnPassiveActivateInteraction.Invoke();
    }
    private class SphereLerpPrepare : IAction
    {
        public SphereLerpPrepare() { }
        public void Invoke() => OnSphereLerpPrepare.Invoke();
    }
    private class SphereLerp : IAction
    {
        public SphereLerp() { }
        public void Invoke() => OnSphereLerp.Invoke();
    }
    private class SphereLerpExit : IAction
    {
        public SphereLerpExit() { }
        public void Invoke() => OnSphereLerpExit.Invoke();
    }
}
// A Utility Class that Manage The Execution and Termination of Events.
public static class MyEventsDispatcher 
{
    public static event Action<string> OnEnterEvent;
    public static event Action<string> OnExitEvent;

    public static List<MyEvent> eventList = new List<MyEvent>();
    public static MyEvent currentEvent;

    public static MyEvent InitiateSelectedEvent(string name, float autoTriggerTimer, bool oneTimeActivation)
    {
        OnEnterEvent?.Invoke(name);
        MyEvent selectedEvent = eventList.Find(x => x.name == name);
        if (selectedEvent == null || selectedEvent.hasActivated)
            return null;
        currentEvent = selectedEvent;
        selectedEvent.isInprogress = true;
        LoadCurrentEventAction(selectedEvent, autoTriggerTimer);
        if (oneTimeActivation)
            selectedEvent.hasActivated = true;
        return selectedEvent;
    }
    public static void EndSelectedEvent(string name)
    {
        MyEvent selectedEvent = eventList.Find(x => x.name == name);
        selectedEvent.isInprogress = false;
 
    }
    public static async void LoadCurrentEventAction(MyEvent currentEvent, float waitTime)
    {
        float time = 0;
        if (currentEvent.StartEventActions != null)
            foreach (IAction a in currentEvent.StartEventActions)
                a.Invoke();
        if (waitTime != 0)
            while (waitTime > 60 ? currentEvent.isInprogress : currentEvent.isInprogress && time < waitTime)
            {
                time += Time.deltaTime;
                currentEvent.OnEventAction?.Invoke();
                await Task.Yield();
            }
        if (currentEvent.EndEventActions != null)
            foreach (IAction a in currentEvent.EndEventActions)
                a.Invoke();
        currentEvent.isInprogress = false;
        currentEvent.hasEnd = true;
        OnExitEvent?.Invoke(currentEvent.name);
    }
}
// Individual Event.
public class MyEvent
{
    public string name;
    public IAction[] StartEventActions;
    public IAction OnEventAction;
    public IAction[] EndEventActions;
    public bool hasActivated = false;
    public bool isInprogress = false;
    public bool hasEnd = false;
    public MyEvent(string _name, IAction[] _StartEventAction, IAction _OnEventAction, IAction[] _EndEventAction)
    {
        name = _name;
        StartEventActions = _StartEventAction;
        OnEventAction = _OnEventAction;
        EndEventActions = _EndEventAction;
    }
}
public interface IAction{ public void Invoke(); }