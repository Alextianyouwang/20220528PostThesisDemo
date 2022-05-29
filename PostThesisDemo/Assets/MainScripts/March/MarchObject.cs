using System.Collections;
using UnityEngine;
using System;
public partial class MarchManager
{
    public class MarchObject
    {
        public string name;
        private MarchObjectGroup parent;
        public GameObject marchObject;
        public GameObject staticObject;
        public Transform playerTransform;

        public Vector3 objectTransformScale;
        public Vector3 smoothedPosition;
        public Vector3 smoothedScale;
        public Vector3 posRef;
        public Vector3 scaleRef;
        public Quaternion smoothedRotation;
        public Vector3 rotRef;
        public Vector3 snapshotPosition;
        public Quaternion snapshotRotation;

        public float smoothSpeed = 0.5f;
        public Coroutine defaultUpdateAnimation;
        public Coroutine alternateUpdateTimer;

        public bool hasBeenActivated = false; 
        public float animationPercent;
        public bool hasBeenAffected = false;
        public bool isInAnimation = false;
        public bool hasAllObjInPosition = true;
        public bool hasBeenPushed = false;
        public bool defaultUpdate = true;
        public bool allowReActivate = true;
        public bool aboveCompletionThreshold = false;
        public bool hasStartedLockingProcess = false;
        public bool hasStartedCountDown= false;
        public bool isStatic = false;
        public bool hasBeenSetVisible = false;
        public bool allowSmooth = true;
        public bool customizeOLColor = true;
        public bool hasTriggeredFromBelow = false;
        public bool hasTriggeredFromAbove = false;

        public float individualExternalControlValue;
        public enum TransformTarget {Auto,StaticObject, Circle}
        public TransformTarget targetState;

        public Vector3 startPosition;
        public Vector3 targetPosition;
        public Vector3 startScale;
        private Vector3 startScaleHolder;
        public Vector3 targetScale;
        public Quaternion startRotation;
        public Quaternion targetRotation;

        private Vector3 fakeParentStartScale;
        private Vector3 posRel;
        private Vector3 fwRel;
        private Vector3 upRel;

        private float speed;
        private Vector3 posPreviousFrame;
        private float initailVolume;
        public float invokeAnimationPercentage;
        public event Action<string, Vector3, AudioRolloffMode, float, int> OnPlaySound;
        public MarchObject(GameObject marcher, GameObject stayer, bool state, Transform transform,  MarchObjectGroup objParent)
        {
            name = stayer.name;
            marchObject = marcher;
            staticObject = stayer;
            hasBeenAffected = state;
            playerTransform = transform;
            parent = objParent;
            objectTransformScale = marchObject.transform.localScale;
            targetState = TransformTarget.Auto;
            InitializeTransforms();
            SetInitialVolume();
        }
        void SetInitialVolume() 
        {
            if (marchObject.GetComponent<AudioSource>()) 
            {
               initailVolume = marchObject.GetComponent<AudioSource>().volume;
            }
        }
        public void InitializeTransforms() 
        {
            Vector3 marcherParentScale = parent.marchingTransformGroup.localScale;
            Vector3 finalScale = new Vector3(
                staticObject.transform.lossyScale.x / marcherParentScale.x,
                staticObject.transform.lossyScale.y / marcherParentScale.y,
                staticObject.transform.lossyScale.z / marcherParentScale.z
                );
            startPosition = marchObject.transform.position;
            targetPosition = staticObject.transform.position;
            startScale = marchObject.transform.localScale;
            startScaleHolder = startScale;
            targetScale = finalScale;
            startRotation = marchObject.transform.rotation;
            targetRotation = staticObject.transform.rotation;
        }

        public void RecordStartTransfrom(Transform parent) 
        {
            fakeParentStartScale = parent.localScale;
            posRel = parent.transform.InverseTransformPoint(marchObject. transform.position);
            fwRel = parent.transform.InverseTransformDirection(marchObject.transform.forward);
            upRel = parent.transform.InverseTransformDirection(marchObject.transform.up);
        }
        public void UpdateStartTransform(Transform parent) 
        {
            float xscaleRatio = fakeParentStartScale.x / parent.localScale.x;
            float yscaleRatio = fakeParentStartScale.x / parent.localScale.y;
            float zscaleRatio = fakeParentStartScale.x / parent.localScale.z;
            Vector3 newScale = new Vector3(
                startScaleHolder.x / xscaleRatio,
                startScaleHolder.y/yscaleRatio,
                startScaleHolder.z/zscaleRatio);
            startPosition = parent.transform.TransformPoint(posRel);
            Vector3 newFw = parent.transform.TransformDirection(fwRel);
            Vector3 newUp = parent.transform.TransformDirection(upRel);
            startRotation = Quaternion.LookRotation(newFw, newUp);
            startScale = newScale;
        }

        public void SetActive(bool state) 
        {
            marchObject.gameObject.SetActive(state);
            hasBeenActivated = state;
        }
        public void SetVisible(bool visible) 
        {
            Material material = marchObject.GetComponent<MeshRenderer>().material;
            material.SetFloat("_Opacity",visible?0:0.1f);
            material.SetFloat("OL_FilterMult",visible?0:1);
            if (!customizeOLColor) 
            {
                material.SetColor("OL_Color",visible?instance.visibleOutlineColor:instance.invisibleOutlineColor);
            }

            hasBeenSetVisible = visible;
            marchObject.GetComponent<MeshRenderer>().shadowCastingMode = 
                visible? UnityEngine.Rendering.ShadowCastingMode.On: UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        public void SetWarnning(bool warnning) 
        {
            Material material = marchObject.GetComponent<MeshRenderer>().material;
            material.SetColor("OL_Color", warnning ? instance.readyOutlineColor : instance.visibleOutlineColor);

        }
        public void SetCustomOLColor(bool target) 
        {
            customizeOLColor = target;
        }

        public void UpdateSmoothSpeed(float target)
        {
            smoothSpeed = target;
        }

        public void PlaySoundOnSetVisible() 
        {
            OnPlaySound?.Invoke("activate",marchObject.transform.position, AudioRolloffMode.Linear,0.5f,10);
        }
        public void PlaySoundOnEnable() 
        {
            OnPlaySound?.Invoke("enable", marchObject.transform.position, AudioRolloffMode.Linear,0.5f,5);
        }
        public void OnTriggerPercentFromBelow() 
        {
            OnPlaySound?.Invoke(parent.soundToPlayBelow, marchObject.transform.position,AudioRolloffMode.Linear,0.9f,parent.belowPriority);
        }
        public void OnTriggerPercentFromAbove() 
        {
            OnPlaySound?.Invoke(parent.soundToPlayAbove, marchObject.transform.position, AudioRolloffMode.Linear,1f, parent.abovePriority);
        } 
        public void UpdateHasBeenPushedState(bool state) 
        {
            hasBeenPushed = state;
        }
        public void DetectMovement() 
        {
            Vector3 distPerFrame = marchObject.transform.position - posPreviousFrame;
            speed = distPerFrame.magnitude / Time.deltaTime;
            posPreviousFrame = marchObject.transform.position;
        }
        public void MakeSoundBySpeed() 
        {
            if (marchObject.GetComponent<AudioSource>()) 
            {
                AudioSource au = marchObject.GetComponent<AudioSource>();
                float volume = Mathf.InverseLerp(0, 50f, speed);
                float adjustedVolume = Mathf.Lerp(0, initailVolume, volume);
                au.volume = adjustedVolume;
            }
        }
        public IEnumerator AlternateTargetTimer(float time, bool isFinal)
        {
            bool confirmFinalMove = isFinal;

            defaultUpdate = false;
            allowReActivate = isFinal? true : false;

            Vector3 playerToObject = marchObject.transform.position - playerTransform.position;
            float distance = playerToObject.magnitude;
            Vector3 randomDirection = new Vector3(
                playerToObject.x + UnityEngine.Random.Range(0.5f, -0.5f),
                playerToObject.y + UnityEngine.Random.Range(0.5f, -0.5f),
                playerToObject.z + UnityEngine.Random.Range(0.5f, -0.5f)).normalized;
            snapshotPosition = playerTransform.position + randomDirection * distance * (confirmFinalMove ? 1.1f : 1.5f);
            snapshotRotation = confirmFinalMove? Quaternion.identity: UnityEngine.Random.rotation;
            smoothSpeed = confirmFinalMove ? 0.1f : 0.2f;

            yield return new WaitForSeconds(time);

            smoothSpeed = 0.5f;
            defaultUpdate = true;

            if (targetState != TransformTarget.Circle) 
            {
                targetState = targetState != TransformTarget.StaticObject ?
                   confirmFinalMove ? TransformTarget.StaticObject : TransformTarget.Auto :
                   TransformTarget.StaticObject;
            }
            if (targetState == TransformTarget.StaticObject) 
            {
                if (!hasStartedCountDown)
                {
                    parent.OnCompletionCallback();
                    hasStartedCountDown = true;
                    if (!parent.willFormCircleFormation)
                    {
                        instance.StartCoroutine(InitiateFinalShutDown(3f));
                    }
                }
            }
            yield return new WaitForSeconds(0.8f);
            allowReActivate = true;
        }
        public IEnumerator InvokeTransformUpdate(float time, bool reset) 
        {
            float percentage = 0;
            float startAnimationPercent = !reset ? 0 : 1;
            float targetAnimiationPercent = !reset ? 1 : 0;
            while (percentage < time)
            {
                invokeAnimationPercentage = Mathf.Lerp(startAnimationPercent, targetAnimiationPercent, percentage);
                percentage += 0.01f;
                yield return 0.01f;
            }
        }
       public IEnumerator InitiateFinalShutDown(float waitTime) 
        {
            float time = 0;
            while (time < waitTime) 
            {
                time += Time.deltaTime;
                yield return null;
            }
            ShutDown();
        }
        public void ShutDown()
        {
            isStatic = true;
            parent.OnShutDownCallBack();
        }
    }
}
