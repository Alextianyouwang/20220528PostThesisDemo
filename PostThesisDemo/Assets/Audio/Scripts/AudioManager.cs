using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public Audios_A audios;
    private AudioItem_A[] audioItems;
    private AudioItem_A currentAudio;
    private AudioItem_A previousAudio;
    private Coroutine easeInCo,easeOutCo;
    private float randomPitch;


    private MarchManager marchManager;

    private void Awake()
    {
        if (audios != null)
        {
            audioItems = audios.audioItems;
        }
        else 
        {
            Debug.LogWarning("Please assign Audio Container");
            return;
        }
        foreach (AudioItem_A au in audioItems) 
        {
            au.audioSource = gameObject.AddComponent<AudioSource>();
            au.audioSource.clip = au.audioClip;
            au.audioSource.volume = au.initialVolume;
            au.audioSource.pitch = au.pitch;
            au.adjustedVolume = 0;
        }
       
        marchManager = FindObjectOfType<MarchManager>();
    }
    private void OnEnable()
    {

        EventExecuter.OnBGM += PlayBGM_EaseInNew;
        EventExecuter.OnBGMStop += StopBGM_EaseOutCurrent;
        EventExecuter.OnPlaySoundSpacial += PlaySFX_Spacial;

        for (int i = 0; i < marchManager.marchObjectGroups.Count; i++) 
        {
            MarchManager.MarchObjectGroup MOG = marchManager.marchObjectGroups[i];
            MOG.OnCompletionSound += PlaySFX_Spacial;
            for (int j = 0; j < MOG.marchObjects.Count;j++) 
            {
                MarchManager.MarchObject MO = MOG.marchObjects[j];
                MO.OnPlaySound += PlaySFX_Spacial;
            }
        }

        VisualManager.OnScanSound += PlaySFX_Spacial_DetailControl;
    }
    private void OnDisable()
    {
        VisualManager.OnScanSound -= PlaySFX_Spacial_DetailControl;
        EventExecuter.OnPlaySoundSpacial -= PlaySFX_Spacial;
        EventExecuter.OnBGM -= PlayBGM_EaseInNew;
        EventExecuter.OnBGMStop -= StopBGM_EaseOutCurrent;

    }
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.L))
        {
            PlaySFX_Overlap("Whoosh0");
        }

    }
    public void PlayBGM_EaseInNew(string name, float time)
    {
        if (currentAudio != GetAudioByName(name) || currentAudio == null) 
        {
            if (currentAudio != null)
            {
                previousAudio = currentAudio;
                if (easeOutCo != null)
                    StopCoroutine(easeOutCo);
                easeOutCo = StartCoroutine(Ease(false, time, previousAudio));
            }

            currentAudio = GetAudioByName(name);
            if (easeInCo != null)
                StopCoroutine(easeInCo);
            easeInCo = StartCoroutine(Ease(true, time, currentAudio));
        }
    }
    public void StopBGM_EaseOutCurrent(float time)
    {
        if (currentAudio != null) 
        {
           if (easeInCo != null)
               StopCoroutine(easeInCo);
           easeInCo = StartCoroutine(Ease(false, time, currentAudio));
           currentAudio = null;
        }
    }
    public void PlaySFX_Simple(string name) 
    {
        Array.Find(audioItems, x => x.name == name).audioSource.Play();
    }
    public void PlaySFX_Overlap(string name) 
    {
        randomPitch = UnityEngine.Random.Range(-0.2f, 0.2f);
        AudioItem_A sfx = Array.Find(audioItems, x => x.name == name);
        if (sfx == null)
            return;
        AudioSource sfxAS = gameObject.AddComponent<AudioSource>();
        sfxAS.clip = sfx.audioClip;
        sfxAS.volume = sfx.initialVolume;
        sfxAS.pitch = sfx.pitch + randomPitch;
        sfxAS.Play();
        StartCoroutine(LerpAudioSource(sfxAS, sfx,0));
        StartCoroutine(RecycleIndividualSFX(sfxAS,sfx,false));
    }
    public void PlaySFX_Spacial(string name,Vector3 pos,AudioRolloffMode mode,float spacialBlend,int priority) 
    {
        randomPitch = UnityEngine.Random.Range(-0.25f, 0.25f);
        AudioItem_A sfx = Array.Find(audioItems, x => x.name == name);
        if (sfx == null)
            return;
        GameObject audioObject = new GameObject();
        audioObject.name = "SFX";
        audioObject.transform.parent = transform;
        audioObject.transform.position = pos;
        AudioSource sfxAS = audioObject.AddComponent<AudioSource>();
        sfxAS.clip = sfx.audioClip;
        sfxAS.volume = sfx.initialVolume;
        sfxAS.pitch = sfx.pitch + randomPitch;
        sfxAS.spatialize = true;
        sfxAS.spatialBlend = spacialBlend;
        sfxAS.rolloffMode = mode;
        sfxAS.maxDistance = 100f;
        sfxAS.priority = priority;
        sfxAS.Play();
        StartCoroutine(LerpAudioSource(sfxAS, sfx,0));
        StartCoroutine(RecycleIndividualSFX(sfxAS, sfx,true));
    }
    public void PlaySFX_Spacial_DetailControl(string name, Vector3 pos, AudioRolloffMode mode,float volume,float pitch)
    {
        randomPitch = UnityEngine.Random.Range(-0.25f, 0.25f);
        AudioItem_A sfx = Array.Find(audioItems, x => x.name == name);
        if (sfx == null)
            return;
        GameObject audioObject = new GameObject();
        audioObject.name = "SFX";
        audioObject.transform.parent = transform;
        audioObject.transform.position = pos;
        AudioSource sfxAS = audioObject.AddComponent<AudioSource>();
        sfxAS.clip = sfx.audioClip;
        sfxAS.volume = sfx.initialVolume;
        sfxAS.pitch = sfx.pitch + randomPitch + pitch;
        sfxAS.spatialize = true;
        sfxAS.spatialBlend = 1f;
        sfxAS.rolloffMode = mode;
        sfxAS.maxDistance = 100f;
        sfxAS.priority = 0;
        sfxAS.Play();
        StartCoroutine(LerpAudioSource(sfxAS, sfx,volume));
        StartCoroutine(RecycleIndividualSFX(sfxAS, sfx, true));
    }
    public void StopSFX(string name)
    {
        Array.Find(audioItems, x => x.name == name).audioSource.Stop();
    }

    public AudioItem_A GetAudioByName(string name) 
    {
       return Array.Find(audioItems, x => x.name == name) ;
    }
    IEnumerator RecycleIndividualSFX(AudioSource sfxAS, AudioItem_A sfx,bool destroySelf)
    {
        yield return new WaitForSeconds(sfxAS.clip.length);
        if (destroySelf) 
            Destroy(sfxAS.gameObject);    
        else
            Destroy(sfxAS);
    }
    IEnumerator LerpAudioSource(AudioSource audioSource,AudioItem_A audioItem,float addtionalVolume) 
    {
        float percentage = 0;
        float start = 0;
        float target = audioItem.initialVolume + addtionalVolume;
        float interpolate;
        while (percentage < 1) 
        {
            percentage += Time.deltaTime/ (audioSource.clip.length-0.1f);
            interpolate = Mathf.Lerp(start,target,audioItem.audioEaseCurve.Evaluate(percentage)) ;
            audioSource.volume = interpolate;
            yield return null;
        }
    }
    IEnumerator Ease(bool easeIn,float time,AudioItem_A audio) 
    {
        float start = audio.adjustedVolume;
        float percentage = start;
        float target = easeIn ? audio.initialVolume : 0;
        
        if (easeIn)
        {
            audio.audioSource.Play();
            audio.audioSource.priority = 0;
        }
        while (easeIn? percentage < target : percentage > target) 
        {
            percentage += easeIn ?
                Time.deltaTime / time * audio.initialVolume:
                -Time.deltaTime / time * audio.initialVolume;
            audio.audioSource.volume = percentage;
            audio.adjustedVolume = percentage;
            yield return null;
        }
        if (!easeIn)
        {
            audio.audioSource.Stop();
            audio.audioSource.priority = 0;
        }
    }
}
