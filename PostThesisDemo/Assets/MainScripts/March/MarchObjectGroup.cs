using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public partial class MarchManager
{
    public class MarchObjectGroup
    {
        // Base Data
        public Transform staticTransformGroup;
        public Transform marchingTransformGroup;
        public Transform playerTransform;
        public List<MarchObject> marchObjects = new List<MarchObject>();
        public List<Material> uniqueMaterials = new List<Material>();
        public List<Material> allMaterials = new List<Material>();
        public Bounds staticGroupBound;
        public string name;
        public Transform fakeParent;

        // Manual Initialization

        public float activationRadius = 20f;
        public bool willFormCircleFormation = false;
        public bool willBeActivatedAtStart = true;
        public bool willReactToInteraction = true;
        public bool independentAffectRadius = false;
        public float indOutterRadius;
        public float indInnerRadius;
        private float flashCounter;

        // External Controls

        public float externalControlPercentage;
        public Vector3 invokeAnimationCenter;
        public float invokeAnimationRadius;
        public float invokeAnimationMaxRadius;
        public bool completeFormation = false;
        public bool completeShutDown = false;
        public bool disableAutoEntropyReduce = false;

        public Vector3[] formationCircles;
        private float numberOfShutDown = 0;
        private float numberOfCompletion = 0;
        public float slowMarchingOffset;
        private float activatedPercentage;
        public bool flexibleActivation = true;
        public bool followTransform = false;
        public bool disableMovements = false;
        public bool customizeOLColor = true;
        public bool independedntSmoothSpeed = false;
        public float indSmoothSpeed;
        public bool willNotSetVisible = false;
        public bool willCombineMesh = true;
        public bool inverseMarchStatic = false;
        public bool detectMovement = false;
        public bool audioEventTriggerdBelow = false;
        public bool audioEventTriggerdAbove = false;
        public float eventTriggerPercentage = 0.95f;
        public string soundToPlayBelow;
        public int belowPriority;
        public string soundToPlayAbove;
        public int abovePriority;
        public string completeSound;
        public string meshCombineSound;
        public bool tutorial = false;
        public bool jobActivate = false;

        //Flags

        private bool hasChangedIndicatorState = false;
        public bool hasCalledCircleFormation = false;
        public bool hasCompletedFormation = false;
        public bool hasBeenShutDown = false;
        public bool hasTotalActivationCharged = false;
        public bool nextChargeTotalActivation = false;
        public bool hasLaunchedRippleEffect = false;
        public bool readyToNormalMarch = true;
        public bool hasAllObjectsInPosition = false;
        public bool aboveCompletionThreshold = false;
        public bool readyToComplete;
        public bool flipIndicationColor = false;
        

        public event Action<MarchObjectGroup> OnFormationComplete;
        public event Action<string, Vector3, AudioRolloffMode, float,int> OnCompletionSound;
        public event Func<MarchObjectGroup, EventExecuter.ObjectActivationState> OnCheckChildActivation;
        public enum MarchMode { distanceLerp, mainInteraction, distanceInvoke, playerDistanceInvoke,  externalControl }
        public MarchMode marchMode;
        public enum ScanMode { normalActivation, onSightActivation}
        public ScanMode scanMode;
      
        //Preparation
        public MarchObjectGroup(Transform staticGroup, Transform marchingGroup, Transform player)
        {
            staticTransformGroup = staticGroup;
            name = staticGroup.name + "_MarchGroup";
            marchingTransformGroup = marchingGroup;
            playerTransform = player;
            formationCircles = new Vector3[staticTransformGroup.childCount];

            if (staticTransformGroup.childCount != marchingTransformGroup.childCount)
            {
                Debug.LogWarning("Transform Child Cound not Equal.");
                return;
            }
            else
            {
                for (int i = 0; i < staticTransformGroup.childCount; i++)
                {
                    marchObjects.Add(
                        new MarchObject(
                        marchingTransformGroup.GetChild(i).gameObject,
                        staticTransformGroup.GetChild(i).gameObject,
                        false,
                        playerTransform,
                        this)
                        ) ;
                }
            }
        }
        public void Initialize()
        {
            SetActivationState();
            RecordFakeParentTransform();
            GroupingMaterials();
            SetBounds();
        }
        public void SetActivationState()
        {
            for (int j = 0; j < marchObjects.Count; j++)
            {
                marchObjects[j].SetActive(willBeActivatedAtStart);
                marchObjects[j].SetCustomOLColor(customizeOLColor);
                if (independedntSmoothSpeed)
                {
                    marchObjects[j].UpdateSmoothSpeed(indSmoothSpeed);
                }
            }
        }
        void RecordFakeParentTransform() 
        {
            if (followTransform && fakeParent != null) 
            {
                for (int j = 0; j < marchObjects.Count; j++)
                {
                    marchObjects[j].RecordStartTransfrom(fakeParent);
                }
            }
        }
        public void GroupingMaterials()
        {
            for (int i = 0; i < staticTransformGroup.childCount; i++)
            {
                Material matI = marchingTransformGroup.GetChild(i).GetComponent<MeshRenderer>().sharedMaterial;
                int j = 0;
                for (j = 0; j < i; j++)
                {
                    Material matJ = marchingTransformGroup.GetChild(j).GetComponent<MeshRenderer>().sharedMaterial;
                    if (matI.name == matJ.name)
                    {
                        break;
                    }
                }
                if (i == j)
                {
                    uniqueMaterials.Add(matI);
                    OnNewMaterialAdd?.Invoke(matI);
                }
                allMaterials.Add(matI);
            }
        }
        public void SetBounds()
        {
            staticGroupBound = GetStaticGroupTotalBounds();
        }
        Bounds GetStaticGroupTotalBounds()
        {
            Bounds totalBound = new Bounds();
            for (int i = 0; i < staticTransformGroup.childCount; i++)
            {
                Bounds individualBound = !inverseMarchStatic ?
                    staticTransformGroup.GetChild(i).gameObject.GetComponent<MeshRenderer>().bounds :
                    marchingTransformGroup.GetChild(i).gameObject.GetComponent<MeshRenderer>().bounds;
                if (i == 0)
                {
                    totalBound = individualBound;
                }
                else
                {
                    totalBound.Encapsulate(individualBound);
                }
            }
            return totalBound;
        }

        //Utilities
        public bool CheckChildActivation()
        {
            int numberOfActivation = 0;
            foreach (MarchObject m in marchObjects)
            {
                if (m.hasBeenActivated)
                {
                    numberOfActivation += 1;
                }
            }

            return numberOfActivation > 0 ? true : false;
        }
        public void DisableVisibility() 
        {
            flexibleActivation = false;
            foreach (MarchObject m in marchObjects) 
            {
                m.SetActive(false);
            }
        }
        IEnumerator TemperoryShutdown(float time)
        {
            OnTogglePlayerInteraction?.Invoke(true);
            disableAutoEntropyReduce = true;
            yield return new WaitForSeconds(time);
            disableAutoEntropyReduce = false;
            OnTogglePlayerInteraction?.Invoke(false);
        }


        //StatusUpdate
        public void MarchCompletionUpdate()
        {

            if (marchMode != MarchMode.mainInteraction) 
            {
                return;
            }


            hasAllObjectsInPosition = true;
            int amountHasBeenActivated = 0;
            for (int i = 0; i < marchingTransformGroup.childCount; i++)
            {

                if (!marchObjects[i].allowReActivate)
                {
                    hasAllObjectsInPosition = false;
                }
                if (marchObjects[i].hasBeenSetVisible)
                {
                    amountHasBeenActivated += 1;

                }
            }
            
            activatedPercentage = (float)amountHasBeenActivated / marchingTransformGroup.childCount;

            readyToNormalMarch = activatedPercentage > 0.7f;
            readyToComplete = Vector3.SqrMagnitude(playerTransform.position - staticGroupBound.center) < Mathf.Pow(activationRadius, 2);
            if (readyToNormalMarch && !hasLaunchedRippleEffect)
            {
                hasLaunchedRippleEffect = true;
                OnRippleEffect?.Invoke(staticGroupBound.center, 50f, 50f, 1f, true);
                instance.StartCoroutine(TemperoryShutdown(1f));
            }
            Tutorial();

            if (((readyToComplete && hasAllObjectsInPosition) || completeFormation) && !hasChangedIndicatorState)
            {
                aboveCompletionThreshold = true;
                hasChangedIndicatorState = true;
            }
            else if (((!readyToComplete || !hasAllObjectsInPosition) && !completeFormation) && hasChangedIndicatorState)
            {
                aboveCompletionThreshold = false;
                hasChangedIndicatorState = false;
            }
            if (readyToNormalMarch && readyToComplete && !flipIndicationColor)
            {
                flipIndicationColor = true;
                IndicateCompletionThreshold(true);
            }
            else if (!(readyToNormalMarch && readyToComplete) && flipIndicationColor)
            {
                flipIndicationColor = false;
                IndicateCompletionThreshold(false);
            }
            if (completeFormation && !hasCompletedFormation)
            {

                hasCompletedFormation = true;
                OnFormationComplete?.Invoke(this);
                if (completeSound != null)
                    OnCompletionSound?.Invoke(completeSound, staticGroupBound.center, AudioRolloffMode.Linear, 1f, 3);

            }
        }
        private void Tutorial() 
        {
            if (!tutorial || completeFormation || readyToComplete)
                return;

            if (!readyToNormalMarch)
            {
                FlashingRipple(3f, 5f, 60f, 5f);
            }

            else if (readyToNormalMarch)
            {
                FlashingRipple(1f, 20f, 20f, 5f);
            }
            else
                return;
        }

        private void FlashingRipple(float interval,float thickness, float radius,float lastingTime) 
        {

            if (flashCounter < Time.time) 
            {
                flashCounter += interval;
                OnRippleEffect?.Invoke(staticGroupBound.center, radius, thickness,lastingTime, true);
            }
        }
        //InteractionUpdate
        public void ObjectActivationUpdate(PlayerInteraction playerControl) 
        {
            foreach (MarchObject currentObj in marchObjects) 
            {
                SetObjectActivationState(currentObj, playerControl);
            }
        }
        public void ObjectsDynamicUpdate(PlayerInteraction playerControl)
        {
            foreach (MarchObject currentObj in marchObjects)
            {
                TransformUpdateWithPlayer(currentObj, fakeParent);
                SetObjectVisibleState(currentObj, playerControl);
                DetectMovement(currentObj);

                switch (marchMode)
                {
                    case (MarchMode.distanceLerp):
                        slowMarchingOffset = playerControl.affectInnerRadius;
                        if (willReactToInteraction)
                        {
                            InteractWithShockwave(currentObj, playerControl, false);
                        }
                        break;
                    case (MarchMode.mainInteraction):

                        slowMarchingOffset = readyToNormalMarch ? playerControl.affectInnerRadius : 0f;
                        InteractWithShockwave(currentObj, playerControl, false);
                        break;
                    case (MarchMode.playerDistanceInvoke):
                        InvokeAnimation(currentObj, playerControl.transform.position, invokeAnimationRadius);

                        break;
                    case (MarchMode.distanceInvoke):

                        InvokeAnimation(currentObj, invokeAnimationCenter, invokeAnimationRadius);
                        break;
                    case (MarchMode.externalControl):
                        if (willReactToInteraction)
                        {
                            InteractWithShockwave(currentObj, playerControl, true);
                        }
                        if (externalControlPercentage >= 1 && !hasBeenShutDown)
                        {
                            hasBeenShutDown = true;
                            instance.StartCoroutine(CombineMeshCountdown(5f));
                        }

                        break;
                }
            }
        }
        void DetectMovement(MarchObject currentObj)
        {
            if (detectMovement) 
            {
                currentObj.DetectMovement();
            }
        }

        void SetObjectVisibleState(MarchObject currentObj,  PlayerInteraction playerControl) 
        {
            if (Mathf.Pow(playerControl.indicatorRadius, 2) > Vector3.SqrMagnitude(playerControl.playerEffectReferencePosition - currentObj.marchObject.transform.position) && currentObj.hasBeenActivated && !currentObj.hasBeenPushed) 
            {
                if (scanMode == ScanMode.onSightActivation)
                {
                    if (Utility.IsVisibleFromCamera(Camera.main, currentObj.marchObject.gameObject,true) )
                    {
                        if (!currentObj.hasBeenSetVisible)
                        {
                            currentObj.PlaySoundOnSetVisible();
                        }
                        currentObj.SetVisible(true);
                    }
                    else
                    {
                        currentObj.SetVisible(currentObj.hasBeenSetVisible);
                    }
                }
                else
                {
                    if (!willNotSetVisible) 
                    {
                    currentObj.SetVisible(true);
                    }
                }
            }
        }
        void SetObjectActivationState(MarchObject currentObj, PlayerInteraction playerControl)
        {
            if (Mathf.Pow(playerControl.indicatorRadius, 2) > Vector3.SqrMagnitude(playerControl.playerEffectReferencePosition - currentObj.marchObject.transform.position) &&
                !currentObj.hasBeenPushed)
            {
                bool activation;
                if (OnCheckChildActivation?.Invoke(this) == EventExecuter.ObjectActivationState.unchange)
                {
                    activation = currentObj.hasBeenActivated;
                }
                else if (OnCheckChildActivation?.Invoke(this) == EventExecuter.ObjectActivationState.activate)
                {
                    activation = true;
                }
                else if (OnCheckChildActivation?.Invoke(this) == EventExecuter.ObjectActivationState.deactivate)
                {
                    activation = false;
                }
                else
                {
                    activation = true;
                }
                if (currentObj.hasBeenActivated != activation) 
                {
                    currentObj.SetActive(activation);
                    currentObj.PlaySoundOnEnable();
                }
            }

        }
        void TransformUpdateWithPlayer(MarchObject currentObj, Transform transformToFollow)
        {
            if (followTransform)
            {
                currentObj.UpdateStartTransform(transformToFollow);
            }
        }
        void InteractWithShockwave(MarchObject currentObj, PlayerInteraction playerControl,bool normalInteraction) 
        {
            if (currentObj.allowReActivate && !currentObj.hasBeenPushed)
            {
                if (normalInteraction)
                {
                    if (Mathf.Pow(playerControl.indicatorRadius, 2) > Vector3.SqrMagnitude(playerControl.playerEffectReferencePosition - currentObj.marchObject.transform.position))
                    {
                        currentObj.hasBeenPushed = true;
                        if (currentObj.alternateUpdateTimer == null)
                        {
                            currentObj.alternateUpdateTimer = instance.StartCoroutine(currentObj.AlternateTargetTimer(0.5f, false));
                        }
                        else
                        {
                            instance.StopCoroutine(currentObj.alternateUpdateTimer);
                            currentObj.alternateUpdateTimer = instance.StartCoroutine(currentObj.AlternateTargetTimer(0.5f, false));
                        }
                       
                    };
                }
                else 
                {
                    if (!aboveCompletionThreshold || !nextChargeTotalActivation)
                    {
                        if (readyToNormalMarch && !hasTotalActivationCharged)
                        {
                            hasTotalActivationCharged = true;
                            nextChargeTotalActivation = true;
                        }
                        if (Mathf.Pow(playerControl.indicatorRadius, 2) > Vector3.SqrMagnitude(playerControl.playerEffectReferencePosition - currentObj.marchObject.transform.position))
                        {
                            currentObj.hasBeenPushed = true;
                            if (currentObj.alternateUpdateTimer == null)
                            {
                                currentObj.alternateUpdateTimer = instance.StartCoroutine(currentObj.AlternateTargetTimer(0.5f, false));
                            }
                            else
                            {
                                instance.StopCoroutine(currentObj.alternateUpdateTimer);
                                currentObj.alternateUpdateTimer = instance.StartCoroutine(currentObj.AlternateTargetTimer(0.5f, false));
                            }
                        };
                    }
                    else
                    {
                        if (!Utility.IsPositionInCamera(Camera.main, staticGroupBound.center)) 
                        {
                            return;
                        }
                        if (Mathf.Pow(playerControl.indicatorRadius, 2) > Vector3.SqrMagnitude(playerControl.playerEffectReferencePosition - currentObj.staticObject.transform.position))
                        {
                            currentObj.SetVisible(true);
                            currentObj.hasBeenPushed = true;
                            if (currentObj.alternateUpdateTimer == null)
                            {
                                currentObj.alternateUpdateTimer = instance.StartCoroutine(currentObj.AlternateTargetTimer(0.5f, true));
                            }
                            else
                            {
                                instance.StopCoroutine(currentObj.alternateUpdateTimer);
                                currentObj.alternateUpdateTimer = instance.StartCoroutine(currentObj.AlternateTargetTimer(0.5f, true));
                            }
                        }
                    }
                }
            }
        }

        //Completion
        public void IndicateCompletionThreshold(bool state)
        {
            foreach (MarchObject m in marchObjects)
            {
                m.SetWarnning(state);
            }
        }
        public void OnCompletionCallback()
        {
            numberOfCompletion += 1;
            if (numberOfCompletion == marchObjects.Count)
            {
                completeFormation = true;
            }
        }
        public void OnShutDownCallBack()
        {
            numberOfShutDown += 1;
            if (numberOfShutDown == marchObjects.Count)
            {
                completeShutDown = true;
                instance.StartCoroutine(CombineMeshCountdown(0.5f));
            }

        }
        IEnumerator CombineMeshCountdown(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            if (willCombineMesh)
            {
                CombineMeshes();
            }
        }
        public void CombineMeshes()
        {
            MeshFilter[] allMeshFilters = new MeshFilter[marchObjects.Count];
            Mesh[] submeshes = new Mesh[uniqueMaterials.Count];
            for (int i = 0; i < marchObjects.Count; i++)
            {
                allMeshFilters[i] = marchObjects[i].marchObject.transform.GetComponent<MeshFilter>();
            }
            Vector3 oldPosition = marchingTransformGroup.transform.position;
            Vector3 oldScale = marchingTransformGroup.transform.localScale;
            Quaternion oldRotation = marchingTransformGroup.transform.rotation;
            marchingTransformGroup.position = Vector3.zero;
            marchingTransformGroup.rotation = Quaternion.identity;
            marchingTransformGroup.localScale = Vector3.one;
            for (int i = 0; i < uniqueMaterials.Count; i++)
            {
                List<CombineInstance> uniqueMaterialCombiners = new List<CombineInstance>();

                for (int j = 0; j < allMeshFilters.Length; j++)
                {

                    if (allMeshFilters[j].GetComponent<MeshRenderer>().material.name != uniqueMaterials[i].name + " (Instance)"
                        //|| allMeshFilters[j].GetComponent<MeshRenderer>().material == uniqueMaterials[i]
                        )
                        continue;
                    CombineInstance ci = new CombineInstance();
                    ci.mesh = allMeshFilters[j].mesh;
                    ci.transform = allMeshFilters[j].transform.localToWorldMatrix;
                    uniqueMaterialCombiners.Add(ci);
                    allMeshFilters[j].gameObject.SetActive(false);

                }
                Mesh meshForUniqueMaterial = new Mesh();
                meshForUniqueMaterial.CombineMeshes(uniqueMaterialCombiners.ToArray(), true);
                submeshes[i] = meshForUniqueMaterial;
            }
            //print(submeshes.Length);
            CombineInstance[] finalCombiners = new CombineInstance[submeshes.Length];
            for (int i = 0; i < submeshes.Length; i++)
            {
                finalCombiners[i].mesh = submeshes[i];
                finalCombiners[i].transform = Matrix4x4.identity;
            }
            Mesh finalMesh = new Mesh();
            finalMesh.CombineMeshes(finalCombiners, false);
            finalMesh.RecalculateBounds();
            finalMesh.RecalculateNormals();
            finalMesh.RecalculateTangents();
            marchingTransformGroup.position = oldPosition;
            marchingTransformGroup.rotation = oldRotation;
            marchingTransformGroup.localScale = oldScale;
            MeshFilter parentMeshFilter = marchingTransformGroup.transform.gameObject.GetComponent<MeshFilter>() ?
                marchingTransformGroup.transform.gameObject.GetComponent<MeshFilter>() :
                marchingTransformGroup.transform.gameObject.AddComponent<MeshFilter>();
            MeshRenderer parentMeshRenderer = marchingTransformGroup.transform.gameObject.GetComponent<MeshRenderer>() ?
                marchingTransformGroup.transform.gameObject.GetComponent<MeshRenderer>() :
                marchingTransformGroup.transform.gameObject.AddComponent<MeshRenderer>();
            parentMeshRenderer.materials = uniqueMaterials.ToArray();

            parentMeshFilter.mesh = finalMesh;
            jobActivate = false;
        }
        void InvokeAnimation(MarchObject currentObj, Vector3 position, float radius) 
        {
            float distanceToPlayer = Vector3.Distance(position, currentObj.staticObject.transform.position);
            if (distanceToPlayer < radius &&
           !currentObj.hasBeenAffected &&
           !currentObj.isInAnimation)
            {
                currentObj.hasBeenAffected = true;
                if (currentObj.defaultUpdateAnimation == null)
                {
                    currentObj.defaultUpdateAnimation = instance.StartCoroutine(currentObj.InvokeTransformUpdate(2f, false));
                }
                else
                {
                    instance.StopCoroutine(currentObj.defaultUpdateAnimation);
                    currentObj.defaultUpdateAnimation = instance.StartCoroutine(currentObj.InvokeTransformUpdate(2f, false));
                }

            }
            else if (distanceToPlayer >= radius &&
                currentObj.hasBeenAffected &&
                !currentObj.isInAnimation)
            {
                currentObj.hasBeenAffected = false;
                if (currentObj.defaultUpdateAnimation == null)
                {
                    currentObj.defaultUpdateAnimation = instance.StartCoroutine(currentObj.InvokeTransformUpdate(2f, true));
                }
                else
                {
                    instance.StopCoroutine(currentObj.defaultUpdateAnimation);
                    currentObj.defaultUpdateAnimation = instance.StartCoroutine(currentObj.InvokeTransformUpdate(2f, true));
                }

            }
        }
        
      
    }
}
