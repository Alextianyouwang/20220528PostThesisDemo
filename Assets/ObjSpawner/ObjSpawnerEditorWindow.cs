#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class ObjSpawnerEditorWindow : EditorWindow
{
    ObjSpawnerBase gridBase;

    int gridRes = 10;
    float gridSize = 10;
    float positionBlend = 0f;
    public GameObject objectToPlace;
    public GameObject objectsContainer;
    public GameObject gridParent;

    private bool randomRotation;
    private float rotaionBlend;

    public enum ObjectTypes { single,multiple};
    public ObjectTypes objectTypes;
    public enum SpawnMode { grid, unitCircle };
    public SpawnMode spawnMode;

    private bool allowReplacementObject;
    private float replacementScale = 1f;


    private bool hasGenerated = false;

    

    [MenuItem("Tool/Object Spawner")]
    static void Init()
    {

        ObjSpawnerEditorWindow window = GetWindow<ObjSpawnerEditorWindow>("Object Spawner");
        window.minSize = new Vector2(400f, 400f);
        window.Show();

    }

    private void OnEnable()
    {
        gridBase = new ObjSpawnerBase();
    }
    private void OnGUI()
    {
        gridParent = (GameObject)EditorGUILayout.ObjectField("Spawn Parent", gridParent, typeof(GameObject), true);
        
        objectTypes = (ObjectTypes)EditorGUILayout.EnumPopup("ObjectTypes", objectTypes);
        if (objectTypes == ObjectTypes.single)
        {
            spawnMode = (SpawnMode)EditorGUILayout.EnumPopup("SpawnMode", spawnMode);
            objectToPlace = (GameObject)EditorGUILayout.ObjectField("Object To Spawn", objectToPlace, typeof(GameObject), true);
            
        }
        else 
        {
            spawnMode = (SpawnMode)EditorGUILayout.EnumPopup("SpawnMode", SpawnMode.unitCircle);
            objectsContainer = (GameObject)EditorGUILayout.ObjectField("Parent of Objects", objectsContainer, typeof(GameObject), true);
            //allowReplacementObject = EditorGUILayout.Toggle("Allow Replacement", allowReplacementObject);
            if (allowReplacementObject) 
            {
                objectToPlace = (GameObject)EditorGUILayout.ObjectField("Object To Spawn", objectToPlace, typeof(GameObject), true);
                if (objectToPlace != null) 
                {
                    //replacementScale = EditorGUILayout.FloatField("Object Relative Scale", replacementScale);
                }
            }
            replacementScale = EditorGUILayout.FloatField("Object Relative Scale", replacementScale);
            positionBlend = EditorGUILayout.Slider("BlendValue", positionBlend, 0, 1);


        }

        if (objectTypes != ObjectTypes.multiple)
        {
            gridRes = EditorGUILayout.IntSlider("Grid Resolution", gridRes, 2, 100);
        }
       
        gridSize = EditorGUILayout.Slider("Grid Size/Radius", gridSize, 1,400);
       
        randomRotation = EditorGUILayout.Toggle("Enable Random Rotation", randomRotation);

        gridBase.gridRes = gridRes;
        gridBase.size = gridSize;
        gridBase.objectToPlace = objectToPlace;
        gridBase.gridParent = gridParent;
        gridBase.positionBlend = positionBlend;
        gridBase.enableRandomRotation = randomRotation;
        if (((objectToPlace != null && objectTypes == ObjectTypes.single)||
            (objectsContainer != null && objectTypes == ObjectTypes.multiple)) &&
            gridParent != null) 
        {
            if (GUILayout.Button("Spawn")) 
            {
                hasGenerated = true;
                gridBase.Clear();
                GenerateMode();
               
            }

            
        }
        if (hasGenerated) 
        {
            if (GUI.changed) 
            {
                gridBase.Clear();
                GenerateMode();
            }
            if (GUILayout.Button("Clear")) 
            {
                hasGenerated = false;
                gridBase.Clear();
            }
        }

    }

    void GenerateMode() 
    {
      
        switch (spawnMode)
        {
            case SpawnMode.grid:

                if (objectToPlace != null) 
                {
                gridBase.SpawnInGrid(objectToPlace);

                }

                break;
            case SpawnMode.unitCircle:
                GameObject[] objects;
                GameObject[] replacementObjects;
                switch (objectTypes)
                {
                    case ObjectTypes.single:
                        objects = new GameObject[gridRes * gridRes];
                        replacementObjects = new GameObject[gridRes * gridRes];

                        for (int i = 0; i < gridRes * gridRes; i++)
                        {
                            if (objectToPlace != null)
                            {
                               
                                replacementObjects[i] = Instantiate(objectToPlace);
                                objects[i] = replacementObjects[i];
 
                            }

                        }
                        gridBase.SpawnInUnitCircle(gridRes*gridRes, objects,true);
                        for (int i = 0; i < gridRes * gridRes; i++)
                        {
                            DestroyImmediate(replacementObjects[i]);
                        }
                        break;

                    case ObjectTypes.multiple:
                        if (objectsContainer != null) 
                        {
                            objects = new GameObject[objectsContainer.transform.childCount];
                            replacementObjects = new GameObject[objectsContainer.transform.childCount];
                            Vector3 parentScale = objectsContainer.transform.localScale;
                            Vector3[] originalScales = new Vector3[objectsContainer.transform.childCount];

                            for (int i = 0; i < objects.Length; i++)
                            {
                                objects[i] = objectsContainer.transform.GetChild(i).gameObject;
                                
                                Vector3 originalPosition = objects[i].transform.localPosition;
                                originalScales[i] = objects[i].transform.localScale;
                                
                                
                                if (objectToPlace != null)
                                {
                                    replacementObjects[i] = Instantiate(objectToPlace);
                                    objects[i] = !allowReplacementObject ? objects[i] : replacementObjects[i];
                                }
           
                                objects[i].transform.localScale = originalScales[i] * replacementScale;
                                objects[i].transform.localPosition = originalPosition;
                                
                            }
                            gridBase.SpawnInUnitCircle(objects.Length, objects,false);
                            for (int i = 0; i < objects.Length; i++) 
                            {
                                DestroyImmediate(replacementObjects[i]);
                                objects[i].transform.localScale = originalScales[i];
                            }

                        }
                        
                        break;

                }
                break;

            default:
                //gridBase.SpawnInGrid();
                break;
        }
    }
}
#endif