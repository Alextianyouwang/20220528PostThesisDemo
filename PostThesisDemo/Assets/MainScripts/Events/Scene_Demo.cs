using System.Collections;
using UnityEngine;
public class Scene_Demo : EventExecuter,
    IOnSceneStart,
    IOnEventEnter,
    IOnEventExit,
    IOnSetMOGFeatures,
    IOnMOGComplete
{
    public AnimationCurve fireworkAnimation;

    protected MarchManager.MarchObjectGroup pivot;
    protected MarchManager.MarchObjectGroup starPieces;
    protected MarchManager.MarchObjectGroup boundaries;
    protected MarchManager.MarchObjectGroup grids;
    protected MarchManager.MarchObject fireworkCenter;

    protected override void Start()
    {
        base.Start();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        EventContainer.OnSphereLerpPrepare += SphereLerpPrepare;
        EventContainer.OnSphereLerp += SphereLerp;
        EventContainer.OnSphereLerpExit += SphereLerpExit;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        EventContainer.OnSphereLerpPrepare -= SphereLerpPrepare;
        EventContainer.OnSphereLerp -= SphereLerp;
        EventContainer.OnSphereLerpExit -= SphereLerpExit;
    }
    public void StartScene() 
    {
        OnBGM?.Invoke("TheCity_0", 5f);
        OnEnablePlayArea?.Invoke("Area_0");
        StartCoroutine(FormBoundary());
    }
    IEnumerator FormBoundary() 
    {
        yield return new WaitForSeconds(1f);
        FindMarchObjectGroup("End_Boundry_MarchGroup").jobActivate = true;
        StartCoroutine(SceneryReform(0f, 6f, 0f, 1f, FindMarchObjectGroup("End_Boundry_MarchGroup"), 1f, null, 6f, false));
    }
    public void SetMarchObjectGroupFeatures()
    {
        foreach (MarchManager.MarchObjectGroup m in marchObjectGroups)
        {
            m.OnFormationComplete += MarchObjectGroupFormationCompletionCallBack;
            m.OnCheckChildActivation += WillMarchObjectBeActivated;
            if (FindMarchObjectGroup("End_Head_MarchGroup") == m)
            {
                m.marchMode = MarchManager.MarchObjectGroup.MarchMode.mainInteraction;
                m.customizeOLColor = false;
                m.willBeActivatedAtStart = true;
                m.tutorial = true;
                m.jobActivate = true;
            }
            if (FindMarchObjectGroup("End_Ground_MarchGroup") == m)
            {
                m.marchMode = MarchManager.MarchObjectGroup.MarchMode.distanceLerp;
                m.willBeActivatedAtStart = true;
                m.willReactToInteraction = false;
                m.independentAffectRadius = true;
                m.independedntSmoothSpeed = true;
                m.indSmoothSpeed = 0.4f;
                m.indInnerRadius = 50f;
                m.indOutterRadius = 60f;
                m.jobActivate = true;
            }

            if (FindMarchObjectGroup("End_Narwhal_MarchGroup") == m)
            {
                m.marchMode = MarchManager.MarchObjectGroup.MarchMode.externalControl;
                m.willBeActivatedAtStart = true;
                m.willReactToInteraction = false;
            }
            if (FindMarchObjectGroup("End_Boundry_MarchGroup") == m)
            {
                m.marchMode = MarchManager.MarchObjectGroup.MarchMode.externalControl;
                m.willBeActivatedAtStart = true;
                m.jobActivate = true;
            }
            if (FindMarchObjectGroup("End_FirstActionCubes_MarchGroup") == m 
                || FindMarchObjectGroup("End_DemoCompletedCubes_MarchGroup") == m
                )
            {
                m.marchMode = MarchManager.MarchObjectGroup.MarchMode.externalControl;
                m.scanMode = MarchManager.MarchObjectGroup.ScanMode.normalActivation;
                m.willBeActivatedAtStart = false;
            }
            
            if (FindMarchObjectGroup("End_Star_Pivot_MarchGroup") == m)
            {
                m.marchMode = MarchManager.MarchObjectGroup.MarchMode.externalControl;
                m.willBeActivatedAtStart = false;
            } 
            
            if (FindMarchObjectGroup("End_Star_MarchGroup") == m)
            {
                m.marchMode = MarchManager.MarchObjectGroup.MarchMode.externalControl;
                m.willBeActivatedAtStart = false;
                m.followTransform = true;
                m.independedntSmoothSpeed = true;
                m.indSmoothSpeed = 0.3f;
                m.fakeParent = FindMarchObject("End_Star_Pivot_MarchGroup", "Sphere").marchObject.transform;
            }

            if (FindMarchObjectGroup("End_Grid_MarchGroup") == m)
            {
                m.marchMode = MarchManager.MarchObjectGroup.MarchMode.externalControl;
                m.willBeActivatedAtStart = false;
            }
            if (FindMarchObjectGroup("End_Guide_MarchGroup") == m)
            {
                m.marchMode = MarchManager.MarchObjectGroup.MarchMode.playerDistanceInvoke;
                m.willBeActivatedAtStart = true;
                m.invokeAnimationRadius = 15f;
                m.jobActivate = true;
            }

            if (FindMarchObjectGroup("End_Building_MarchGroup") == m)
            {
                m.marchMode = MarchManager.MarchObjectGroup.MarchMode.externalControl;
                m.scanMode = MarchManager.MarchObjectGroup.ScanMode.normalActivation;
                m.willBeActivatedAtStart = false;
            }
        }
    }
    public ObjectActivationState WillMarchObjectBeActivated(MarchManager.MarchObjectGroup currentMOG)
    {
        ObjectActivationState state = ObjectActivationState.unchange;
        if (currentMOG.flexibleActivation && MyEventsDispatcher.currentEvent == null)
            return ObjectActivationState.unchange;
            if (MyEventsDispatcher.currentEvent.name == "AfterAssembleNarwhal")
            {
                if (currentMOG.name == "End_Star_MarchGroup" ||
                    currentMOG.name == "End_FirstActionCubes_MarchGroup" ||
                    currentMOG.name == "End_Grid_MarchGroup")
                {
                    state = ObjectActivationState.activate;
                }
            }
            if (MyEventsDispatcher.currentEvent.name == "EndDemo")
            {
                if (currentMOG.name == "End_Star_MarchGroup" ||
                    currentMOG.name == "End_FirstActionCubes_MarchGroup" ||
                    currentMOG.name == "End_Grid_MarchGroup" )
                {
                    state = ObjectActivationState.deactivate;
                }
                if (currentMOG.name == "End_DemoCompletedCubes_MarchGroup" ||
                    currentMOG.name == "End_Building_MarchGroup") 
                {
                    state = ObjectActivationState.activate;
                }
            }
        return state;
    }

    public void MarchObjectGroupFormationCompletionCallBack(MarchManager.MarchObjectGroup currentMOG)
    {
        if (FindMarchObjectGroup("End_Head_MarchGroup") == currentMOG) 
        {
            FindMarchObjectGroup("End_Narwhal_MarchGroup").jobActivate = true;
            StartCoroutine(SceneryReform(0f, 6f, 0f, 1f, FindMarchObjectGroup("End_Narwhal_MarchGroup"), 1f, "AfterAssembleNarwhal", 8f, true));
            OnRotateObject?.Invoke("Rot_1", 10f);
        }
    }
    public void QuickExecuteOnEnterEvent(string eventName) 
    {
        if (eventName == "AfterAssembleNarwhal") 
        {
            OnEnablePlayArea?.Invoke("Area_1");
            FindMarchObjectGroup("End_Grid_MarchGroup").jobActivate = true;
            FindMarchObjectGroup("End_FirstActionCubes_MarchGroup").jobActivate = true;
        }
        if (eventName == "EndDemo") 
        {
            FindMarchObjectGroup("End_DemoCompletedCubes_MarchGroup").jobActivate = true;
            FindMarchObjectGroup("End_Building_MarchGroup").jobActivate = true;
            StartCoroutine(SceneryReform(4f, 10f, 0f, 1f, FindMarchObjectGroup("End_Building_MarchGroup"), 1f, null, 0, true));
            OnRotateObject?.Invoke("Rot_2", 10f);
        }
    }
    public void QuickExecuteOnExitEvent(string eventName)
    {
        if (eventName == "AfterAssembleNarwhal")
        {
            FindMarchObjectGroup("End_Grid_MarchGroup").jobActivate = false;
            FindMarchObjectGroup("End_FirstActionCubes_MarchGroup").jobActivate = false;
        }
        if (eventName == "EndDemo") 
        {
            FindMarchObjectGroup("End_DemoCompletedCubes_MarchGroup").jobActivate = false;
        }
    }

    public void SphereLerpPrepare() 
    {
        pivot = FindMarchObjectGroup("End_Star_Pivot_MarchGroup");
        starPieces = FindMarchObjectGroup("End_Star_MarchGroup");
        boundaries = FindMarchObjectGroup("End_Boundry_MarchGroup");
        grids = FindMarchObjectGroup("End_Grid_MarchGroup");
        fireworkCenter = FindMarchObject("End_Star_Pivot_MarchGroup", "Sphere");
        if (MyEventsDispatcher.currentEvent.name != "SphereLerp" || !MyEventsDispatcher.currentEvent.isInprogress)
            return;
        pivot.jobActivate = true;
        starPieces.jobActivate = true;
        boundaries.jobActivate = true;
        grids.jobActivate = true;
    }
    public void SphereLerp() 
    {
        float lerpValue = GetLerpValueFromTrigger("EventRange_0");
        Vector3 center = fireworkCenter.marchObject.transform.position;
        foreach (MarchManager.MarchObject m in pivot.marchObjects)
        {
            m.individualExternalControlValue = 1- lerpValue;
        }
        foreach (MarchManager.MarchObject m in boundaries.marchObjects)
        {
            m.individualExternalControlValue = lerpValue;
        }
        foreach (MarchManager.MarchObject m in starPieces.marchObjects)
        {
            float fireworkLerpValue = fireworkAnimation.Evaluate(1 - lerpValue);
            m.individualExternalControlValue = fireworkLerpValue;
        }
        foreach (MarchManager.MarchObject m in grids.marchObjects)
        {
            if (lerpValue > 0.2f)
            {
                float distance = Vector3.Distance(center, m.marchObject.transform.position);
                float gridAnimation = Utility.Remap(distance, 0, 8, 1, 0);
                m.individualExternalControlValue = gridAnimation;
            }
            else 
            {
                m.individualExternalControlValue = 1 - lerpValue;
            }
        }
    }
    public void SphereLerpExit() 
    {
        pivot.jobActivate = false;
        starPieces.jobActivate = false;
        boundaries.jobActivate = false;
        grids.jobActivate = false;
    }
}
