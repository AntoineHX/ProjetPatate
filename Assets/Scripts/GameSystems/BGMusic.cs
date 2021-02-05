using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Define the background music behavior.
public class BGMusic : MonoBehaviour
{
    //Intensity levels
    [SerializeField]
    int intensity= 0; //Current intensity level
    public int Intensity{get{return intensity;} set{changeIntensity(value);}} //Accessors intensity
    public static int Levels= 3; //Number of intensity levels (differents tracks)

    //Clips
    [SerializeField]
    AudioClip AudioIntensity0, AudioIntensity1, AudioIntensity2; //Must have enough clip foreach intensity levels

    List<AudioSource> audioSources; //Sources used for each intensity clips
    List<float> targetVolumes = new List<float>(Levels);
    [SerializeField]
    float changeTime = 1.0f; //Time to smoothly change intensity level
    List<float> changeVelocity = new List<float>(Levels); //Current velocity for smooth change

    //Change the intensity level of the music. If instantChange is false, do it smoothly.
    public void changeIntensity(int level, bool instantChange=false)
    {
        if(level>=Levels)
        {
            Debug.LogWarning(gameObject.name+" doesn't have such high intensity available :"+ level+ " /"+Levels);
            level = Levels-1; //Max level
        }
        else if(level<0)
        {
            Debug.LogWarning(gameObject.name+" doesn't have such low intensity available :"+ level+ " /"+Levels);
            level = 0; //Min level
        }
            
        for(int i=0; i<Levels; i++)
        {
            if(i!=level)
            {
                targetVolumes[i]=0.0f;
                if(instantChange)
                    audioSources[i].volume=0.0f;
            }
            else
            {
                targetVolumes[i]=1.0f;
                if(instantChange)
                    audioSources[i].volume=1.0f;
            }
        }

        intensity = level;
        Debug.Log(gameObject.name+" - Intensity : "+intensity);
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSources = new List<AudioSource>(GetComponents<AudioSource>());
        if(audioSources.Count != Levels )
            Debug.LogWarning(gameObject.name+" needs "+Levels+" audio sources. Found : "+audioSources.Count);

        //Load clip
        audioSources[0].clip = AudioIntensity0;
        audioSources[1].clip = AudioIntensity1;
        audioSources[2].clip = AudioIntensity2;

        //Initialize target & velocities
        for(int i=0; i<Levels; i++)
        {
            targetVolumes.Add(0.0f);
            changeVelocity.Add(0.0f);
        }

        //Set initial intensity
        changeIntensity(intensity, true);
            

        //Start sources
        foreach(AudioSource s in audioSources)
            s.Play();
    }

    // Update is called once per frame
    void Update()
    {
        float currVel;
        for(int i=0; i<Levels; i++)
        {
            currVel=changeVelocity[i];
            audioSources[i].volume=Mathf.SmoothDamp(audioSources[i].volume, targetVolumes[i], ref currVel, changeTime);
            changeVelocity[i]=currVel;
        }
    }
}
