using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
public class PlayerInteraction : MonoBehaviour
{

    public float affectOuterRadius = 100;
    public float affectInnerRadius = 5;

    private float outerRadiusRef;
    private float innerRadiusRef;

    [HideInInspector]
    public float smoothedOuterRadius;
    [HideInInspector]
    public float smoothedInnerRadius;

    private float scanLastingTime = 1.5f;

    public bool disableInteraction;

    //public GameObject indicator;

    [HideInInspector]
    public float indicatorRadius;
    

    public static event Action<Vector3,float,float,float,bool> OnEntrophyReduce;
    public static event Action OnIndicatorReturnToZero;
    public static event Action<PlayerInteraction> OnCharactorMove;

    public EventContainer eventHandler;

    [HideInInspector]
    public Vector3 playerEffectReferencePosition;
    [HideInInspector]
    public bool duringEntrophyReduce = false;

    private Coroutine entrophyReduceCo;

    public static event Action OnInteract;


  


    private void OnEnable()
    {
        EventContainer.OnChangePlayerAffectRadius += ChangeInteractParameters;
        EventContainer.OnTogglePlayerInteraction += ToggleDisableInteraction;
        EventContainer.OnPassiveActivateInteraction += EntropyReduceInteract;
        MarchManager.OnTogglePlayerInteraction += ToggleDisableInteraction;
        //GlobalEventCaller.OnEnablePlayerActivationCharge += ToggleThisChargeTriggerActivation;
    }
    private void OnDisable()
    {
        EventContainer.OnChangePlayerAffectRadius -= ChangeInteractParameters;
        EventContainer.OnTogglePlayerInteraction -= ToggleDisableInteraction;
        EventContainer.OnPassiveActivateInteraction -= EntropyReduceInteract;
        MarchManager.OnTogglePlayerInteraction -= ToggleDisableInteraction;

        //GlobalEventCaller.OnEnablePlayerActivationCharge -= ToggleThisChargeTriggerActivation;
    }
    void Start()
    {

        StartCoroutine(UpdateMovement());
    }

    int[] ShuixianNumber(int end)
    {
        int digitCount = (int)Mathf.Log10(end) + 1;
        List<int> result = new List<int>();
        for (int i = 0; i < end + 1; i++)
        {
            int leftovers = 0;
            int sum = 0;
            for (int j = 0; j < digitCount; j++)
            {
                int pureTenth = (i % (int)Mathf.Pow(10, j + 1) - leftovers) / (int)Mathf.Pow(10, j);
                leftovers += pureTenth;
                sum += (int)Mathf.Pow(pureTenth, 3);
            }
            if (sum == i)
            {
                result.Add(i);
            }
        }
        return result.ToArray();
    }

    int[] PrimeNumber( int start, int end) 
    {
        List<int> result = new List<int>();
        for (int i = start; i < end; i++) 
        {
            int counter = 0;
            for (int j = 2; j < i ; j++) 
            {
                if ((float)i / j % 1 != 0 ) 
                {
                    counter += 1;
                }
            }
            if (counter == i - 2) 
            {
                result.Add(i);
            }
        }
        return result.ToArray();
    }

    int[] PerfectNumber(int start, int end) 
    {
        List<int> result = new List<int>();
        for (int i = start; i < end; i++) 
        {
            List<int> partition = new List<int>();

            for (int j = 1; j < i; j++) 
            {
                if ((float)i / j % 1 == 0) 
                {
                    partition.Add(j);
                }
            }
            int sum = 0;
            for (int k = 0; k < partition.Count; k++) 
            {
                sum += partition[k];
            }
            if (sum == i) 
            {
                result.Add(sum);
            }
        }
        return result.ToArray();
    }

    int[] BubbleSort(int[] input) 
    {
        int compareTime = input.Length;
        for (int j = 0; j < input.Length; j++) 
        {
            for (int i = 1; i < input.Length; i++)
            {
                if (compareTime < i)
                    continue;
                if (input[i - 1] > input[i])
                    Swap(ref input[i - 1], ref input[i]);
            }
            compareTime -= 1;
        }
        return input;
    }
    void Swap(ref int a,ref int b) 
    {
        int holder = a;
        a = b;
        b = holder;
    }

    void ToggleDisableInteraction(bool state) 
    {
        disableInteraction = state;
        //print(disableInteraction);
    }

    void ChangeInteractParameters(float newInnerRadius,float newOuterRadius,float newLastingTime) 
    {
        affectInnerRadius = newInnerRadius;
        affectOuterRadius = newOuterRadius;
        scanLastingTime = newLastingTime;
    }

    IEnumerator UpdateMovement() 
    {
        while (true) 
        {
            if (Input.GetKeyDown(KeyCode.Escape)) 
            {
                //SceneManager.LoadScene(0);
            }
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
            //OnIndicatorReturnToZero?.Invoke();
            entrophyReduceCo = StartCoroutine(EntropyReduce(scanLastingTime, 0f, affectOuterRadius));
        }

        OnEntrophyReduce?.Invoke(transform.position, affectOuterRadius, affectOuterRadius * 2f, scanLastingTime,false);
    }

    IEnumerator EntropyReduce(float lastingTime, float startRadius, float endRadius) 
    {
        Vector3 startScale = startRadius * Vector3.one;
        Vector3 targetScale = endRadius * Vector3.one;
        Vector3 startPosition = transform.position;
        //GameObject effect = Instantiate(indicator);
        // effect.transform.position = startPosition;
        //effect.transform.localScale = startScale;
        //indicatorRadius = 0;
        duringEntrophyReduce = true;
        float percent = 0;
        while (percent < 1) 
        {
            percent += Time.deltaTime/lastingTime;
            //effect.transform.localScale = Vector3.Lerp(startScale, targetScale, percent);
            //effect.transform.position = transform.position;
            indicatorRadius = percent * affectOuterRadius;
            playerEffectReferencePosition = startPosition;
           yield return null;
        }
        duringEntrophyReduce = false;
        //Destroy(effect);
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
