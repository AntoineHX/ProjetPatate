using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Define the system managing the events. (Singleton)
public sealed class EventManager : MonoBehaviour
{
    //Singleton
    private static EventManager _instance=null;
    public static EventManager Instance { get 
        { 
        if(_instance is null)
            Debug.LogError("Missing EventManager instance");
        return _instance; 
        } 
    }

    [HideInInspector]
    public bool ready = false; //Wether the ClientManager is initialized

    [SerializeField]
    float SpawnRange = 1.0f;
    [SerializeField]
    int nbMaxEvents = 1; //Maximum active clients
    [SerializeField]
    float eventSpawnTimer = 0.5f; //Intial time before first spawn (pseudo-random after that)
    [SerializeField]
    float maxTimeNewEvents = 2.0f; //Longest waiting time for new clients
    bool eventSpawnReady = false;

    [SerializeField]
    string EventRessourceFolder = "Events";
    private Object[] events;
    GameObject EventContainer=null;
    List<int> eventIDs = new List<int>();

    //Try to find a random point on NavMesh inside a range circle. A result would at a maximum distance of 2*range. 
    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 50; i++)
        {
            Vector3 randomPoint = center + (Vector3)Random.insideUnitCircle * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, range, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }

    public void spawnEvent(Vector3 position)
    {
        Vector3 spawnPoint;
        if (eventSpawnReady && eventIDs.Count<nbMaxEvents && RandomPoint(position, SpawnRange, out spawnPoint))
        {
            Debug.DrawRay(spawnPoint, Vector3.up, Color.blue, 2.0f);
            int prefabChoice = Random.Range(0, events.Length);
            GameObject newEvent = Instantiate((GameObject)events[prefabChoice], spawnPoint, Quaternion.identity, EventContainer.transform); //Instantiate new event inside ClientManager
            eventIDs.Add(newEvent.GetInstanceID()); //Save ID
            newEvent.name = newEvent.name.Split('(')[0]+eventIDs[eventIDs.Count-1]; //Rename new client

            eventSpawnTimer=Random.Range(1.0f, maxTimeNewEvents); //Need more random ?
            eventSpawnReady=false;
        }
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
            EventContainer = GameObject.Find("/GameSystem/EventManager");
            if (EventContainer is null)
                throw new System.Exception("No EventManager found under GameSystem");

            events = Resources.LoadAll(EventRessourceFolder);
            if (events.Length==0)
            {
                Debug.LogWarning("EventManager didn't find events prefab in ressource folder : "+EventRessourceFolder);
            }

            ready = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!eventSpawnReady)
        {
            eventSpawnTimer-= Time.deltaTime;
            if(eventSpawnTimer<=0)
                eventSpawnReady=true;
        }
    }
}
