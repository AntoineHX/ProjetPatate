using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Define the background music behavior.
public class BGMusic : MonoBehaviour
{
    public int intensity =0; //Current intensity level
    int intensityLevels = 2; //Number of intensity levels (differents tracks)
    [SerializeField]
    AudioClip AudioIntensity0, AudioIntensity1; //Must have enough clip foreach intensity levels

    List<AudioSource> audioSources; //Sources used for each intensity clips

    //Change the intensity level of the music
    //TODO: Smooth transition
    public void changeIntensity(int level)
    {
        if(level>=intensityLevels)
        {
            Debug.LogWarning(gameObject.name+" doesn't have such high intensity available :"+ level+ " /"+intensityLevels);
            level = intensityLevels-1; //Max level
        }
            
        for(int i=0; i<intensityLevels; i++)
        {
            if(i!=level)
                audioSources[i].volume=0.0f;
            else
                audioSources[i].volume=1.0f;
        }

        intensity = level;
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSources = new List<AudioSource>(GetComponents<AudioSource>());
        if(audioSources.Count != intensityLevels )
            Debug.LogWarning(gameObject.name+" missing audio sources too play all intensity levels : "+ intensityLevels);

        //Load clip
        audioSources[0].clip = AudioIntensity0;
        audioSources[1].clip = AudioIntensity1;

        //Set initial intensity
        changeIntensity(intensity);
        
        //Start sources
        foreach(AudioSource s in audioSources)
            s.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
