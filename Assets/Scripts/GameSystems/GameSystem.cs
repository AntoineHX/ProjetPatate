using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Define the global game system of the service. (Singleton)
public sealed class GameSystem : MonoBehaviour
{
    public static string GameSystem_path="/GameSystem";
    //Singleton
    private static GameSystem _instance=null;
    public static GameSystem Instance { get 
        { 
        if(_instance is null) //Force Awakening if needed
            GameObject.Find(GameSystem_path).GetComponent<GameSystem>().Awake();
        return _instance; 
        } 
    }

    [HideInInspector]
    public bool ready = false; //Wether the GameSystems are initialized

    //Time
    [HideInInspector]
    public bool serviceOpen = false;
    [SerializeField]
    float serviceTime = 30.0f;
    float serviceTimer = 0.0f;
    UITimer UIServiceTimer;
    [SerializeField]
    float slowScale = 0.5f; //Default scale for slow mode
    private float fixedDeltaTime;

    //TODO : Effect on gold change
    //Money
    private int gold;
    public int Gold
    {
        get{return gold;}
        set{
            gold = Mathf.Abs(value);
            Debug.Log("Gold : "+gold);
        }
    }

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
                Debug.LogError("Trying to set time scale to negative value (rewinding time...) :"+newTimeScale);
            
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
        //Singleton
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;

        if(!ready)
        {
            // Make a copy of the fixedDeltaTime, it defaults to 0.02f, but it can be changed in the editor
            this.fixedDeltaTime = Time.fixedDeltaTime;
            GameObject timerObj = GameObject.Find("/UI/Canvas/ServiceTimer");
            if(timerObj is null)
            {
                Debug.LogWarning("No service timer found");
                UIServiceTimer=null;
            }
            else
            {
                UIServiceTimer=timerObj.GetComponent<UITimer>();
            }

            if(ClientManager.Instance.ready && StockManager.Instance.ready)
            {
                ready=true;
                Debug.Log("All GameSystems are ready");
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        startService();
        gold=0;
    }

    // Update is called once per frame
    void Update()
    {
        if(serviceOpen)
        {
            //Update service timer
            serviceTimer-= Time.deltaTime;
            if(UIServiceTimer != null)
                UIServiceTimer.SetValue(serviceTimer/serviceTime);
            if (serviceTimer < 0)
            {
                serviceOpen = false;
                Debug.Log("Service closed");
            }
        }

        //Temporary manual slowmode toggle
        if (Input.GetButtonDown("Fire2"))
        {
            toggleSlowMode();
            Debug.Log("Time scale: "+Time.timeScale);
        }
        if (Input.GetButtonDown("Fire3"))
        {
            toggleSlowMode(2.0f);
            Debug.Log("Time scale: "+Time.timeScale);
        }
        // Debug.Log("Service timer : "+(int)serviceTimer);
    }

    // simple Singleton implementation
    //public static GameSystem instance { get; private set; } //Give public access to the instance. But only set from this class

    //// Singleton Implementation (https://jlambert.developpez.com/tutoriels/dotnet/implementation-pattern-singleton-csharp/#LIII) ////
    // private GameSystem()
    // {
    // }

    // public static GameSystem Instance { get { return Nested.instance; } }
        
    // private class Nested
    // {
    //     // Explicit static constructor to tell C# compiler
    //     // not to mark type as beforefieldinit
    //     static Nested()
    //     {
    //     }

    //     internal static readonly GameSystem instance = new GameObject("GameSystem").AddComponent<GameSystem>();
    // }
    ////
}