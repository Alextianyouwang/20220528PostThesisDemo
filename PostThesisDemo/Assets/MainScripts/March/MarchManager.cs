using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Burst;

public partial class MarchManager : MonoBehaviour
{

    public static MarchManager instance;
    public PlayerInteraction playerControl;
    public Transform grandMarchingTransform, grandStaticTransform;
    public List<MarchObjectGroup> marchObjectGroups = new List<MarchObjectGroup>();
    private EventExecuter eveExe;

    public static event Action<Material> OnNewMaterialAdd;
    public static event Action<Vector3, float, float, float,bool> OnRippleEffect;
    public static event Action<bool> OnTogglePlayerInteraction;

    private bool globleAntrophyReduceOveride = false;

    [ColorUsage(true,true )]
    public Color invisibleOutlineColor;
    [ColorUsage(true, true)]
    public Color visibleOutlineColor;
    [ColorUsage(true, true)]
    public Color readyOutlineColor;

    NativeArray<Vector3>[] 
        _posRef, _scaleRef, _rotRef,
        _initialPosition, _finalPosition,_initialScale, _finalScale,
        _snapshotPosition,
        _circleFormationPosition, _circleFormationScaleMultiplier,
         _objectPosition, _objectScale;

    NativeArray<Quaternion>[] 
        _initialRotaiton, _finalRotation,
        _snapshotRoation,
        _circleFormationRotation,
        _objectRotation;

    NativeArray<float>[]
        _smoothSpeed,
        _animationPercent,
        _invokeAnimationPercent,
        _externalControlPercent;
        
    NativeArray<MarchObject.TransformTarget>[] _targetState;
    NativeArray<bool>[]
        _defaultUpdate,
        _allowSmooth;

    [BurstCompile(CompileSynchronously = true)]
    private struct ObjectMarchingJobs : IJobParallelFor
    {
        public Vector3 playerPosition;
        public float deltaTime;
        
        public float innerRadius;
        public float outerRadius;

        public NativeArray<float> animationPercent;
        public NativeArray<float> smoothSpeed;

        public NativeArray<Vector3> posRef;
        public NativeArray<Vector3> initialPosition;
        public NativeArray<Vector3> finalPosition;

        public NativeArray<Vector3> scaleRef;
        public NativeArray<Vector3> initialScale;
        public NativeArray<Vector3> finalScale;

        public NativeArray<Vector3> rotRef;
        public NativeArray<Quaternion> initialRotation;
        public NativeArray<Quaternion> finalRotation;
        
        public NativeArray<Vector3> snapshotPosition;
        public NativeArray<Quaternion> snapshotRotation;

        public NativeArray<Vector3> circleFormationPosition;
        public NativeArray<Vector3> circleFormationScaleMultiplier;
        public NativeArray<Quaternion> circleFormationRotation;

        public MarchObjectGroup.MarchMode marchMode;
        public NativeArray<MarchObject.TransformTarget> targetState;
        public NativeArray<bool> defaultUpdate;
        public NativeArray<bool> allowSmooth;

        public NativeArray<float>  externalControlPercent;
        public NativeArray<float> invokeAnimationPercent;

        public bool disableEntropyReduce;
        public bool disableMovement;
        public bool playerInteractionReady;
        
        public NativeArray<Vector3> objectPosition;
        public NativeArray<Vector3> objectScale;
        public NativeArray<Quaternion> objectRotation;
        public void Execute(int index)
        {
            Vector3 smoothedPosition = Vector3.zero;
            Vector3 smoothedScale = Vector3.one;
            Quaternion smoothedRotation = Quaternion.identity;
            float distanceToPlayer = Vector3.Distance(playerPosition, finalPosition[index]);
            switch (marchMode)
            {
                case MarchObjectGroup.MarchMode.distanceLerp:
                    if (!disableEntropyReduce)
                    {
                        animationPercent[index] = Mathf.InverseLerp(outerRadius, innerRadius, distanceToPlayer);
                    }
                    break;
                case MarchObjectGroup.MarchMode.mainInteraction:
                    if (!disableEntropyReduce && playerInteractionReady)
                    {
                        animationPercent[index] = Mathf.InverseLerp(outerRadius, innerRadius, distanceToPlayer);
                    }
                    break;
            }

            switch (marchMode)
            {
                case MarchObjectGroup.MarchMode.externalControl:
                    if (!disableMovement)
                    {
                        animationPercent[index] = externalControlPercent[index];
                    }
                    smoothedPosition = defaultUpdate[index] ? Vector3.Lerp(initialPosition[index], finalPosition[index], animationPercent[index]) : snapshotPosition[index];
                    smoothedScale = Vector3.Lerp(initialScale[index], finalScale[index], animationPercent[index]);
                    smoothedRotation = Quaternion.Slerp(initialRotation[index], finalRotation[index], animationPercent[index]);

                    break;
                case MarchObjectGroup.MarchMode.playerDistanceInvoke:
                case MarchObjectGroup.MarchMode.distanceInvoke:
                    animationPercent[index] = invokeAnimationPercent[index];
                    smoothedPosition = Vector3.Lerp(initialPosition[index], finalPosition[index], animationPercent[index]);
                    smoothedScale = Vector3.Lerp(initialScale[index], finalScale[index], animationPercent[index]);
                    smoothedRotation = Quaternion.Slerp(initialRotation[index], finalRotation[index], animationPercent[index]);
                    break;
                case MarchObjectGroup.MarchMode.distanceLerp:

                case MarchObjectGroup.MarchMode.mainInteraction:

                    switch (targetState[index])
                    {
                        case (MarchObject.TransformTarget.Auto):
                            if (Vector3.SqrMagnitude(playerPosition - finalPosition[index]) < Mathf.Pow(outerRadius, 2))
                            {
                                smoothedPosition = defaultUpdate[index] ? Vector3.Lerp(initialPosition[index], finalPosition[index], animationPercent[index]) : snapshotPosition[index];
                                smoothedScale = Vector3.Lerp(initialScale[index], finalScale[index], animationPercent[index]);

                                smoothedRotation = defaultUpdate[index] ? Quaternion.Slerp(initialRotation[index], finalRotation[index], animationPercent[index]) : snapshotRotation[index];
                            }
                            else
                            {
                                smoothedPosition = initialPosition[index];
                                smoothedScale = initialScale[index];
                                smoothedRotation = initialRotation[index];
                            }

                            break;
                        case (MarchObject.TransformTarget.StaticObject):
                            smoothedPosition = finalPosition[index];
                            smoothedScale = finalScale[index];
                            smoothedRotation = finalRotation[index];
                            break;
                    }
                    break;
            }
            Vector3 currentPosRef = posRef[index];
            Vector3 currentScaleRef = scaleRef[index];
            Vector3 currentRotRef = rotRef[index];

            if (allowSmooth[index])
            {
                objectPosition[index] = Vector3.SmoothDamp(objectPosition[index], smoothedPosition, ref currentPosRef, smoothSpeed[index], 10000f, deltaTime);
                objectScale[index] = Vector3.SmoothDamp(objectScale[index], smoothedScale, ref currentScaleRef, smoothSpeed[index], 100000f, deltaTime);
                objectRotation[index] = Utility.SmoothDampQuaternion(objectRotation[index], smoothedRotation, ref currentRotRef, smoothSpeed[index], 100f, deltaTime);
            }
            else
            {
                objectPosition[index] = smoothedPosition;
                objectScale[index] = smoothedScale;
                objectRotation[index] = smoothedRotation;
            }
            posRef[index] = currentPosRef;
            scaleRef[index] = currentScaleRef;
            rotRef[index] = currentRotRef;
        }
    }
    private void OnEnable()
    {
        EventContainer.OnDisableEntropyReduce += ToggleActivation;
        PlayerInteraction.OnIndicatorReturnToZero += RefreshObjectsPushingState;
    }
    private void OnDisable()
    {
        EventContainer.OnDisableEntropyReduce -= ToggleActivation;
        PlayerInteraction.OnIndicatorReturnToZero -= RefreshObjectsPushingState;
        
    }
    private void Awake()
    {
        eveExe = FindObjectOfType<EventExecuter>();
        if (instance == null)
        {
            instance = this;
        }
        else 
        {
            Destroy(instance);
        }
        if (grandMarchingTransform.childCount !=grandStaticTransform.childCount) 
        {
            Debug.LogWarning("Transform Group Count not Equal.");
            return;
        }
        else 
        {
            for (int i = 0; i < grandStaticTransform.childCount; i++) 
            {
                marchObjectGroups.Add(
                    new MarchObjectGroup (
                        grandStaticTransform.GetChild(i),
                        grandMarchingTransform.GetChild(i),
                        playerControl.transform
                        ));
            }
        }
        eveExe.Initialze();
        AllocateNativeLists();
  
     
    }
    private void OnDestroy()
    {
        DisposeNativeLists();
    }
    void AllocateNativeLists()
    {
        _posRef = new NativeArray<Vector3>[marchObjectGroups.Count];
        _scaleRef = new NativeArray<Vector3>[marchObjectGroups.Count];
        _rotRef = new NativeArray<Vector3>[marchObjectGroups.Count];

        _initialPosition = new NativeArray<Vector3>[marchObjectGroups.Count];
        _finalPosition = new NativeArray<Vector3>[marchObjectGroups.Count];

        _initialScale = new NativeArray<Vector3>[marchObjectGroups.Count];
        _finalScale = new NativeArray<Vector3>[marchObjectGroups.Count];

        _initialRotaiton = new NativeArray<Quaternion>[marchObjectGroups.Count];
        _finalRotation = new NativeArray<Quaternion>[marchObjectGroups.Count];

        _snapshotPosition = new NativeArray<Vector3>[marchObjectGroups.Count];
        _snapshotRoation = new NativeArray<Quaternion>[marchObjectGroups.Count];

        _circleFormationPosition = new NativeArray<Vector3>[marchObjectGroups.Count];
        _circleFormationScaleMultiplier = new NativeArray<Vector3>[marchObjectGroups.Count];
        _circleFormationRotation = new NativeArray<Quaternion>[marchObjectGroups.Count];

        _objectPosition = new NativeArray<Vector3>[marchObjectGroups.Count];
        _objectScale = new NativeArray<Vector3>[marchObjectGroups.Count];
        _objectRotation = new NativeArray<Quaternion>[marchObjectGroups.Count];

        _smoothSpeed = new NativeArray<float>[marchObjectGroups.Count];

        _targetState = new NativeArray<MarchObject.TransformTarget>[marchObjectGroups.Count];

        _defaultUpdate = new NativeArray<bool>[marchObjectGroups.Count];
        _allowSmooth = new NativeArray<bool>[marchObjectGroups.Count];
       
        _animationPercent = new NativeArray<float>[marchObjectGroups.Count];
        _invokeAnimationPercent = new NativeArray<float>[marchObjectGroups.Count];
        _externalControlPercent = new NativeArray<float>[marchObjectGroups.Count];

            for (int i = 0; i < marchObjectGroups.Count; i++)
        {
            _posRef[i] = new NativeArray<Vector3>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);
            _scaleRef[i] = new NativeArray<Vector3>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);
            _rotRef[i] = new NativeArray<Vector3>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);

            _initialPosition[i] = new NativeArray<Vector3>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);
            _finalPosition[i] = new NativeArray<Vector3>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);

            _initialScale[i] = new NativeArray<Vector3>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);
            _finalScale[i] = new NativeArray<Vector3>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);

            _initialRotaiton[i] = new NativeArray<Quaternion>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);
            _finalRotation[i] = new NativeArray<Quaternion>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);

            _snapshotPosition[i] = new NativeArray<Vector3>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);
            _snapshotRoation[i] = new NativeArray<Quaternion>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);

            _circleFormationPosition[i] = new NativeArray<Vector3>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);
            _circleFormationScaleMultiplier[i] = new NativeArray<Vector3>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);
            _circleFormationRotation[i] = new NativeArray<Quaternion>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);

            _objectPosition[i] = new NativeArray<Vector3>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);
            _objectScale[i] = new NativeArray<Vector3>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);
            _objectRotation[i] = new NativeArray<Quaternion>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);

            _smoothSpeed[i] = new NativeArray<float>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);

            _targetState[i] = new NativeArray<MarchObject.TransformTarget>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);

            _defaultUpdate[i] = new NativeArray<bool>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);
            _allowSmooth[i] = new NativeArray<bool>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);

            _animationPercent[i] = new NativeArray<float>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);
            _invokeAnimationPercent[i] = new NativeArray<float>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);
            _externalControlPercent[i] = new NativeArray<float>(marchObjectGroups[i].marchObjects.Count, Allocator.Persistent);
        }
    }
    void DispatchJobs()
    {
        for (int i = 0; i < marchObjectGroups.Count; i++)
        {
            if (!marchObjectGroups[i].jobActivate) 
            {
                continue;
            }
            int arrayLength = marchObjectGroups[i].marchObjects.Count;
            for (int j = 0; j < arrayLength; j++)
            {
                MarchObject currentMarchObj = marchObjectGroups[i].marchObjects[j];
                _initialPosition[i][j] = currentMarchObj.startPosition;
                _finalPosition[i][j] = currentMarchObj.targetPosition;
                _initialScale[i][j] = currentMarchObj.startScale;
                _finalScale[i][j] = currentMarchObj.targetScale;
                _initialRotaiton[i][j] = currentMarchObj.startRotation;
                _finalRotation[i][j] = currentMarchObj.targetRotation;
                _snapshotPosition[i][j] = currentMarchObj.snapshotPosition;
                _snapshotRoation[i][j] = currentMarchObj.snapshotRotation;
                _objectPosition[i][j] = currentMarchObj.marchObject.transform.position;
                _objectScale[i][j] = currentMarchObj.marchObject.transform.localScale;
                _objectRotation[i][j] = currentMarchObj.marchObject.transform.rotation;
                _smoothSpeed[i][j] = currentMarchObj.smoothSpeed;

                _targetState[i][j] = currentMarchObj.targetState;

                _defaultUpdate[i][j] = currentMarchObj.defaultUpdate;
                _allowSmooth[i][j] = currentMarchObj.allowSmooth;

                _animationPercent[i][j] = currentMarchObj.animationPercent;
                _invokeAnimationPercent[i][j] = currentMarchObj.invokeAnimationPercentage;
                _externalControlPercent[i][j] = currentMarchObj.individualExternalControlValue;
            }

            ObjectMarchingJobs march = new ObjectMarchingJobs
            {
                playerPosition = playerControl.transform.position,
                deltaTime = Time.deltaTime,

                innerRadius = marchObjectGroups[i].independentAffectRadius ? marchObjectGroups[i].indInnerRadius : marchObjectGroups[i].slowMarchingOffset,
                outerRadius = marchObjectGroups[i].independentAffectRadius ? marchObjectGroups[i].indOutterRadius : playerControl.affectOuterRadius,

                marchMode = marchObjectGroups[i].marchMode,
                externalControlPercent = _externalControlPercent[i],
                disableEntropyReduce = marchObjectGroups[i].disableAutoEntropyReduce,
                disableMovement = marchObjectGroups[i].disableMovements,
                playerInteractionReady = playerControl.indicatorRadius == 0,

                posRef = _posRef[i],
                scaleRef = _scaleRef[i],
                rotRef = _rotRef[i],

                initialPosition = _initialPosition[i],
                finalPosition = _finalPosition[i],

                initialScale = _initialScale[i],
                finalScale = _finalScale[i],

                initialRotation = _initialRotaiton[i],
                finalRotation = _finalRotation[i],

                circleFormationPosition = _circleFormationPosition[i],
                circleFormationScaleMultiplier = _circleFormationScaleMultiplier[i],
                circleFormationRotation = _circleFormationRotation[i],

                snapshotPosition = _snapshotPosition[i],
                snapshotRotation = _snapshotRoation[i],
                smoothSpeed = _smoothSpeed[i],

                targetState = _targetState[i],

                defaultUpdate = _defaultUpdate[i],
                allowSmooth = _allowSmooth[i],

                animationPercent = _animationPercent[i],
                invokeAnimationPercent = _invokeAnimationPercent[i],
                objectPosition = _objectPosition[i],
                objectScale = _objectScale[i],
                objectRotation = _objectRotation[i],
            };

            JobHandle marchJH = march.Schedule(marchObjectGroups[i].marchObjects.Count, 10);
            marchJH.Complete();

            MarchObjectGroup currentMOG = marchObjectGroups[i];
            
            for (int j = 0; j < arrayLength; j++)
            {
                MarchObject currentMO = currentMOG.marchObjects[j];
                currentMO.marchObject.transform.position = _objectPosition[i][j];
                currentMO.marchObject.transform.localScale = _objectScale[i][j];
                currentMO.marchObject.transform.rotation = _objectRotation[i][j];
                currentMO.animationPercent = _animationPercent[i][j];
                if (!currentMOG.audioEventTriggerdBelow) 
                {
                    if (_animationPercent[i][j] >= currentMOG.eventTriggerPercentage && !currentMO.hasTriggeredFromBelow)
                    {
                        currentMO.OnTriggerPercentFromBelow();
                        currentMO.hasTriggeredFromBelow = true;
                    }
                    if (_animationPercent[i][j] < currentMOG.eventTriggerPercentage)
                    {
                        currentMO.hasTriggeredFromBelow = false;
                    }
                }
                if (!currentMOG.audioEventTriggerdAbove) 
                {
                    if (_animationPercent[i][j] <= currentMOG.eventTriggerPercentage && !currentMO.hasTriggeredFromAbove)
                    {
                        currentMO.OnTriggerPercentFromAbove();
                        currentMO.hasTriggeredFromAbove = true;
                    }
                    if (_animationPercent[i][j] > currentMOG.eventTriggerPercentage)
                    {
                        currentMO.hasTriggeredFromAbove = false;
                    }
                }
            }
        }
    }
    void DisposeNativeLists() 
    {
        for (int i = 0; i < marchObjectGroups.Count; i++) 
        {
            _posRef[i].Dispose();
            _scaleRef[i].Dispose();
            _rotRef[i].Dispose();

            _initialPosition[i].Dispose();
            _finalPosition[i].Dispose();

            _initialScale[i].Dispose();
            _finalScale[i].Dispose();

            _initialRotaiton[i].Dispose();
            _finalRotation[i].Dispose();

            _snapshotPosition[i].Dispose();
            _snapshotRoation[i].Dispose();

            _circleFormationPosition[i].Dispose();
            _circleFormationScaleMultiplier[i].Dispose();
            _circleFormationRotation[i].Dispose();

            _objectPosition[i].Dispose();
            _objectScale[i].Dispose();
            _objectRotation[i].Dispose();

            _smoothSpeed[i].Dispose();

            _targetState[i].Dispose();

            _defaultUpdate[i].Dispose();
            _allowSmooth[i].Dispose();
            
            _animationPercent[i].Dispose();
            _invokeAnimationPercent[i].Dispose();
            _externalControlPercent[i].Dispose();
        }
    }
    void Start()
    {
        for (int i = 0; i < marchObjectGroups.Count; i++)
        {
            marchObjectGroups[i].Initialize();
        }
    }
    private void Update()
    {
        for (int i = 0; i < marchObjectGroups.Count; i++)
        {
            marchObjectGroups[i].ObjectActivationUpdate(playerControl);
            if (!marchObjectGroups[i].jobActivate) 
                continue;
            marchObjectGroups[i].MarchCompletionUpdate();
            marchObjectGroups[i].ObjectsDynamicUpdate(playerControl);
        }
    }
    private void LateUpdate()
    {
        DispatchJobs();
    }
    private void OnDrawGizmos()
    {
        foreach (MarchObjectGroup m in marchObjectGroups) 
        {
            if (m.marchMode == MarchObjectGroup.MarchMode.mainInteraction) 
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(m.staticGroupBound.center, 2f);
                Gizmos.color = m.readyToComplete ? Color.green : Color.red;
                Gizmos.DrawWireSphere(m.staticGroupBound.center, m.activationRadius);
            }
            
        }
    }
    void ToggleActivation(bool state) 
    {
        globleAntrophyReduceOveride = state;
        foreach (MarchObjectGroup m in marchObjectGroups) 
        {
            m.disableAutoEntropyReduce = globleAntrophyReduceOveride;
        }
    }
    void RefreshObjectsPushingState()
    {
        for (int i = 0; i < marchObjectGroups.Count; i++)
        {
            for (int j = 0; j < marchObjectGroups[i].marchObjects.Count; j++)
            {
                marchObjectGroups[i].marchObjects[j].UpdateHasBeenPushedState(false);
            }
        }
    }
}
