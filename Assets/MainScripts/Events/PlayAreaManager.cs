using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayAreaManager : MonoBehaviour
{
    public GameObject colliderContainer;
    [HideInInspector]
    public List<GameObject> colliders;

    private void OnEnable()
    {
        EventExecuter.OnEnablePlayArea += OnlyEnableSelectedCollider;
    }
    private void OnDisable()
    {
        EventExecuter.OnEnablePlayArea -= OnlyEnableSelectedCollider;
    }
    private void Awake()
    {
        foreach (Transform t in colliderContainer.transform) 
        {
            colliders.Add(t.gameObject);
            t.gameObject.SetActive(false);
        }
    }

    public void OnlyEnableSelectedCollider(string name) 
    {
        GameObject selected = Array.Find(colliders.ToArray(), x => x.name == name);
        GameObject[] notSelected = Array.FindAll(colliders.ToArray(), x => x.name != name);
        for (int i = 0; i < notSelected.Length; i++) 
        {
            notSelected[i].SetActive(false);
        }
        selected.SetActive(true);
    }
}
