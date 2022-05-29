using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VisualManager : MonoBehaviour
{

    public List<Material> scanMats;
   //public Material fogMat;


    [Range(1,6)]
    public int maxRipples;
    private int rippleIndex;

    private Vector3[] positionList;
    private float[] percentageList;
    private float[] radiusList;
    private float[] thicknessList;
    private float[] lastingTimeList;
    private bool[] inverse;
 
    private Vector3 playerPosition;
    private float outerRadius;
    private float innerRadius;


    public static event Action<string, Vector3, AudioRolloffMode,float,float> OnScanSound;



    void Start()
    {
      
        positionList = new Vector3[maxRipples];
        percentageList = new float[maxRipples];
        radiusList = new float[maxRipples];
        thicknessList = new float[maxRipples];
        lastingTimeList = new float[maxRipples];
        inverse = new bool[maxRipples];
        
        for (int i = 0; i < maxRipples; i++)
        {
            positionList[i] = Vector3.zero;
            percentageList[i] = 1;
            radiusList[i] = 0;
            thicknessList[i] = 0;
            lastingTimeList[i] = 0;
            inverse[i] = false;
        }
       
    }

    private void OnEnable()
    {
        PlayerInteraction.OnEntrophyReduce += RippleRefresh;
        MarchManager.OnNewMaterialAdd += AddingNewmaterial;
        //MarchManager.OnNewMaterialRemove += RemoveMaterial;
        MarchManager.OnRippleEffect += RippleRefresh;
        PlayerInteraction.OnCharactorMove += ReceivePlayerInfo;


    }
    private void OnDisable()
    {
        PlayerInteraction.OnEntrophyReduce -= RippleRefresh;
        MarchManager.OnNewMaterialAdd -= AddingNewmaterial;
        //MarchManager.OnNewMaterialRemove += RemoveMaterial;
        MarchManager.OnRippleEffect -= RippleRefresh;
        PlayerInteraction.OnCharactorMove -= ReceivePlayerInfo;

    }

    void AddingNewmaterial(Material m)
    {
        //if (!scanMat.Contains(m)) 
        {
            scanMats.Add(m);
        }
        
    }
    void RemoveMaterial(Material m) 
    {
        scanMats.Remove(m);
    }
    void Update()
    {
        //RayCastCheck();
        RippleUpdate();
        //FogValueUpdate();

    }

    void FogTargerValueChange (float target)
    {
    }


    /*   void RayCastCheck()
       {
           if (Input.GetMouseButtonDown(0))
           {
               Ray mouseRay = cam.ScreenPointToRay(Input.mousePosition);
               RaycastHit hit;
               if (Physics.Raycast(mouseRay, out hit, 100f, mask))
               {
                   hitPos = hit.point;
                   RippleRefresh(hitPos, finalRaidus, initialThickness, lastingTime);
               }
           }

       }
   */

    void ReceivePlayerInfo(PlayerInteraction player) 
    {
        playerPosition = player.transform.position;
        outerRadius = player.smoothedOuterRadius;
        innerRadius = player.smoothedInnerRadius;


    }

   
    public void RippleRefresh(Vector3 _hitPos,float _radius, float _thickness, float _speed,bool _inverse) 
    {
        float pitchFromSpeed = Mathf.InverseLerp(1f, 10f, _speed);
        float pitchOffset = Mathf.Lerp(0.5f, -0.5f, pitchFromSpeed);
        float volumeFromRadius = Mathf.InverseLerp(50f, 400f, _radius);
        float volumeOffset = Mathf.Lerp(0, 1f, volumeFromRadius);
        OnScanSound?.Invoke("scan_0", _hitPos, AudioRolloffMode.Logarithmic, volumeOffset, pitchOffset);
        if (_radius >= 200f)
        {
            OnScanSound?.Invoke("base_0", _hitPos, AudioRolloffMode.Linear, volumeOffset, pitchOffset);
            //OnScanSound?.Invoke("base_1", _hitPos,AudioRolloffMode.Linear,volumeOffset,pitchOffset);

        }
        else 
        {
         

        }
        Shader.SetGlobalVector("ScanPos" + rippleIndex.ToString(), _hitPos);
        /*foreach (Material m in scanMats) 
        {
          m.SetVector("ScanPos" + rippleIndex.ToString(), _hitPos);
        }*/
        positionList[rippleIndex] =_hitPos;
        percentageList[rippleIndex] = 0;
        radiusList[rippleIndex] = _radius;
        thicknessList[rippleIndex] = _thickness;
        lastingTimeList[rippleIndex] = _speed;
        inverse[rippleIndex] = _inverse;

        rippleIndex += 1;
        rippleIndex %= maxRipples;
    }
    
    void RippleUpdate() 
    {
        for (int i = 0; i < maxRipples; i++)
        {
            if (percentageList[i] <= 1.02f)
            {
                percentageList[i] += Time.deltaTime / lastingTimeList[i];
                
            }
            float radius = !inverse[i] ? Mathf.Lerp(0.0f, radiusList[i], percentageList[i]) : Mathf.Lerp(radiusList[i], 0.0f, percentageList[i]);
            float thickness = !inverse[i] ? Mathf.Lerp(thicknessList[i], 0f, percentageList[i]) : Mathf.Lerp(0f, thicknessList[i], percentageList[i]);
           /* foreach (Material m in scanMats)
            {


               m.SetFloat("Radius" + i.ToString(), radius);
               m.SetFloat("Thickness" + i.ToString(), thickness);
            }*/
            Shader.SetGlobalFloat("Radius" + i.ToString(), radius);
            Shader.SetGlobalFloat("Thickness" + i.ToString(), thickness);

        }

        foreach (Material m in scanMats)
        {
            m.SetVector("_Position", playerPosition);
            m.SetFloat("_InnerRadius", innerRadius);
            m.SetFloat("_OuterRadius", outerRadius);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Gizmos.DrawSphere(hitPos, 0.1f);
    }
}
