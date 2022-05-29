using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjSpawnerBase
{

    public GameObject gridParent;
    public GameObject objectToPlace;
    public int gridRes;
    public float size;
    public float positionBlend = 1f;
    public bool enableRandomRotation;

    private List<GameObject> spawnedObject = new List<GameObject>();
    public void SpawnInGrid( GameObject objectToSpawn) 
    {
        Vector3 offset = new Vector3(-size / 2, 0, -size / 2);
        float increment = size / (gridRes - 1);
        for (int y = 0; y < gridRes; y++) 
        {
            for (int x = 0; x < gridRes; x++) 
            {
                Quaternion randomRotation = Random.rotation;
                Quaternion finalRotation = enableRandomRotation ? randomRotation : objectToSpawn.transform.rotation;
                Vector3 position = new Vector3(x, 0, y) * increment + offset + gridParent.transform.position;
                SpawnPoint(position,finalRotation, objectToSpawn);
               
            }
        }
    }
    public void SpawnInUnitCircle(int num, GameObject[] objectsToSpawn,bool single) 
    {

        for (int i = 0; i < num; i++) 
        {
            Vector3 randomPosition = Random.insideUnitSphere * size + gridParent.transform.position;
            Quaternion randomRotation = Random.rotation;
            Vector3 originalPosition = objectsToSpawn[i].transform.localPosition + gridParent.transform.position;
            Vector3 lerpedPosition = single? randomPosition: Vector3.Lerp(originalPosition,randomPosition, positionBlend);
            Quaternion finalRotation = enableRandomRotation ? randomRotation : objectsToSpawn[i].transform.rotation;

         
            SpawnPoint(lerpedPosition, finalRotation, objectsToSpawn[i]);

        }
    }
    
    void SpawnPoint(Vector3 position, Quaternion rotation, GameObject objectToSpawn) 
    {
        GameObject newObj = GameObject.Instantiate(objectToSpawn);
        newObj.name = position.x.ToString() + "," + position.z.ToString();
        newObj.transform.position = position;
        newObj.transform.rotation = rotation;
        newObj.transform.parent = gridParent.transform;
    }

    public void Clear() 
    {
        foreach (Transform t in gridParent.transform) 
        {
            spawnedObject.Add(t.gameObject);

        }
        foreach (GameObject  g in spawnedObject) 
        {
            GameObject.DestroyImmediate(g);
        }
    }


}
