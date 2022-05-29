using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "My Assets/AudioContainer")]
public class Audios_A : ScriptableObject
{
    [SerializeField]
    public AudioItem_A[] audioItems;
}
[System.Serializable]
public class AudioItem_A
{
    public string name;
    public AudioClip audioClip;
    [Range (0,2)]
    public float initialVolume = 1;
    [HideInInspector]
    public float adjustedVolume = 0;
    public AnimationCurve audioEaseCurve;
    [Range(0,3)]
    public float pitch = 1;
    [HideInInspector]
    public AudioSource audioSource;
}