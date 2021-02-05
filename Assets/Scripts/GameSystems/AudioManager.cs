using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class AudioManager : MonoBehaviour
{
    public static string AudioManager_path="/GameSystem/AudioManager";
    //Singleton
    private static AudioManager _instance=null;
    public static AudioManager Instance { get 
        { 
        if(_instance is null) //Force Awakening if needed
            GameObject.Find(AudioManager_path).GetComponent<AudioManager>().Awake();
        return _instance; 
        } 
    }

    [HideInInspector]
    public bool ready = false; //Wether the AudioManager is initialized

    [SerializeField]
    BGMusic BGMusic = null; //Background music script

    [SerializeField]
    float clientWeight=1.0f, HardObsWeight=1.0f, SoftObsWeight=1.0f; //Importance of each entity

    float maxEntity; //Maximum entities (client + events) weighted by importance 
    float currentNbEntity; //Current number of entities (Client + events) weighted by importance 

    //Update audio intensity
    void updateIntensity()
    {
        //Intensity change thresholds
        float upThreshold = ((BGMusic.Intensity+1)*maxEntity)/BGMusic.Levels;
        float downTreshold = ((BGMusic.Intensity)*maxEntity)/BGMusic.Levels; //add -1 or % to prevent constant change ?
        // Debug.Log(gameObject.name+" - treshold : (+)"+upThreshold+"/(-)"+downTreshold);

        //Update intensity
        if(BGMusic.Intensity<(BGMusic.Levels-1) && currentNbEntity> upThreshold) //Increase Intensity if needed
            BGMusic.Intensity++;
        else if(BGMusic.Intensity>0 && currentNbEntity< downTreshold) //Decrease Intensity if needed
            BGMusic.Intensity--;
    }

    // Start is called before the first frame update
    void Awake()
    {
        //Singleton
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;
        
        if(!ready)
        {
            if(BGMusic is null)
                Debug.LogError(gameObject.name+" doesnt have a background music script");

            //Maximum entity weighted by importance 
            maxEntity = clientWeight*ClientManager.Instance.MaxClients+HardObsWeight*EventManager.Instance.MaxEvents("Hard")+SoftObsWeight*EventManager.Instance.MaxEvents("Soft");

            ready = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Update current number of entity (weighted)
        currentNbEntity=clientWeight*ClientManager.Instance.clientList.Count+HardObsWeight*EventManager.Instance.eventCount("Hard")+SoftObsWeight*EventManager.Instance.eventCount("Soft");
        updateIntensity();
    }
}
