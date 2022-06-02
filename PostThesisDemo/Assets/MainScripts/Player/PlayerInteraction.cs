using System.Collections;
using UnityEngine;
using System;
public class PlayerInteraction : MonoBehaviour
{

    public float affectOuterRadius = 100;
    public float affectInnerRadius = 5;

    private float outerRadiusRef;
    private float innerRadiusRef;

    private float scanLastingTime = 1.5f;

    public bool disableInteraction;
    public bool duringEntrophyReduce = false;

    [HideInInspector]
    public float indicatorRadius;
    [HideInInspector]
    public float smoothedOuterRadius;
    [HideInInspector]
    public float smoothedInnerRadius;
    [HideInInspector]
    public Vector3 playerEffectReferencePosition;
    [HideInInspector]

    private Coroutine entrophyReduceCo;

    public static event Action OnInteract;
    public static event Action<Vector3, float, float, float, bool> OnEntrophyReduce;
    public static event Action OnIndicatorReturnToZero;
    public static event Action<PlayerInteraction> OnCharactorMove;

    private void OnEnable()
    {
        EventContainer.OnChangePlayerAffectRadius += ChangeInteractParameters;
        EventContainer.OnTogglePlayerInteraction += ToggleDisableInteraction;
        EventContainer.OnPassiveActivateInteraction += EntropyReduceInteract;
        MarchManager.OnTogglePlayerInteraction += ToggleDisableInteraction;
    }
    private void OnDisable()
    {
        EventContainer.OnChangePlayerAffectRadius -= ChangeInteractParameters;
        EventContainer.OnTogglePlayerInteraction -= ToggleDisableInteraction;
        EventContainer.OnPassiveActivateInteraction -= EntropyReduceInteract;
        MarchManager.OnTogglePlayerInteraction -= ToggleDisableInteraction;
    }
    void Start()
    {
        StartCoroutine(UpdateInteraction());
    }
    void ToggleDisableInteraction(bool state) 
    {
        disableInteraction = state;
    }
    void ChangeInteractParameters(float newInnerRadius,float newOuterRadius,float newLastingTime) 
    {
        affectInnerRadius = newInnerRadius;
        affectOuterRadius = newOuterRadius;
        scanLastingTime = newLastingTime;
    }
    IEnumerator UpdateInteraction() 
    {
        while (true) 
        {
            smoothedOuterRadius = Mathf.SmoothDamp(smoothedOuterRadius, affectOuterRadius, ref outerRadiusRef, 0.5f);
            smoothedInnerRadius = Mathf.SmoothDamp(smoothedInnerRadius, affectInnerRadius, ref innerRadiusRef, 0.5f);
            if (Input.GetKeyDown(KeyCode.F) && !disableInteraction && indicatorRadius == 0)
            {
                EntropyReduceInteract();
            }
            OnCharactorMove?.Invoke(this);
            if (!duringEntrophyReduce)
            {
                playerEffectReferencePosition = transform.position;
            }
            yield return null;
        }
    }
    void EntropyReduceInteract() 
    {
        OnInteract?.Invoke();
        if (entrophyReduceCo == null)
        {
            entrophyReduceCo = StartCoroutine(EntropyReduce(scanLastingTime, 0f, affectOuterRadius));
        }
        else
        {
            StopCoroutine(entrophyReduceCo);
            entrophyReduceCo = StartCoroutine(EntropyReduce(scanLastingTime, 0f, affectOuterRadius));
        }
        OnEntrophyReduce?.Invoke(transform.position, affectOuterRadius, affectOuterRadius * 2f, scanLastingTime,false);
    }
    IEnumerator EntropyReduce(float lastingTime, float startRadius, float endRadius) 
    {
        Vector3 startScale = startRadius * Vector3.one;
        Vector3 targetScale = endRadius * Vector3.one;
        Vector3 startPosition = transform.position;
        duringEntrophyReduce = true;
        float percent = 0;
        while (percent < 1) 
        {
            percent += Time.deltaTime/lastingTime;
            indicatorRadius = percent * affectOuterRadius;
            playerEffectReferencePosition = startPosition;
           yield return null;
        }
        duringEntrophyReduce = false;
        indicatorRadius = 0;
        OnIndicatorReturnToZero?.Invoke();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, affectInnerRadius);
        Gizmos.DrawWireSphere(transform.position, affectOuterRadius);
    }
}
