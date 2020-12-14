using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; //Exceptions

//Define the global game system of the service. (Singleton)
//TODO : Split gamesystem into subsystem ?
public sealed class GameSystem : MonoBehaviour
{
    //Time
    bool serviceOpen = false;
    float serviceTime = 10.0f;
    float serviceTimer = 0.0f;
    float slowScale = 0.5f; //Default scale for slow mode
    private float fixedDeltaTime;

    //Clients
    // int nbMaxClients = 2;
    // float freqNewClients = 3.0f;

    public void startService()
    {
        serviceTimer=serviceTime;
        serviceOpen=true;
    }

    //Change time scale
    //Return wether game time follow real-time (not scaled)
    public bool toggleSlowMode(float? newTimeScale=null)
    {
        if(newTimeScale is null) //Toggle between default values
        {
            if(Mathf.Approximately(Time.timeScale, 1.0f))
                Time.timeScale = slowScale;
            else
                Time.timeScale = 1.0f;
        }
        else //Set to specific scale
        {
            if(newTimeScale<0.0f)
                throw new Exception("Trying to set time scale to negative value (rewinding time...) :"+newTimeScale);
            
            Time.timeScale = (float)newTimeScale;
        }

        // Adjust fixed delta time according to timescale
        // The fixed delta time will now be 0.02 frames per real-time second
        Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;

        //Return wether game time follow real-time (not scaled)
        if(Mathf.Approximately(Time.timeScale, 1.0f))
            return true;
        else
            return false;
    }
    
    //Awake is called when the script instance is being loaded.
    void Awake()
    {
        // Make a copy of the fixedDeltaTime, it defaults to 0.02f, but it can be changed in the editor
        this.fixedDeltaTime = Time.fixedDeltaTime;
    }

    // Start is called before the first frame update
    void Start()
    {
        startService();
    }

    // Update is called once per frame
    void Update()
    {
        if(serviceOpen)
        {
            serviceTimer-= Time.deltaTime;
            if (serviceTimer < 0)
                serviceOpen = false;
        }

        //Temporary manual slowmode toggle
        if (Input.GetButtonDown("Fire2"))
        {
            toggleSlowMode();
            Debug.Log("Time scale: "+Time.timeScale);
        }
        // Debug.Log("Service timer : "+(int)serviceTimer);
    }

    //// Singleton Implementation (https://jlambert.developpez.com/tutoriels/dotnet/implementation-pattern-singleton-csharp/#LIII) ////
    private GameSystem()
    {
    }

    public static GameSystem Instance { get { return Nested.instance; } }
        
    private class Nested
    {
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Nested()
        {
        }

        internal static readonly GameSystem instance = new GameObject("GameSystem").AddComponent<GameSystem>();
    }
    ////
}