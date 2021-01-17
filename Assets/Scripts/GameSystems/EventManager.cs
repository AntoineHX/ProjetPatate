using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Define the system managing the events. (Singleton)
//TODO: Switch to a registering approach for events
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
    float SpawnRange = 1.0f; //Range of an event spawn from its origin (real max distance = 2*range)
    [SerializeField]
    int nbMaxEvents = 1; //Maximum active clients
    [SerializeField]
    float spawnChance = 100.0f; //Probability of an event to spawn
    // [SerializeField]
    // float eventSpawnTimer = 0.5f; //Intial time before first spawn (pseudo-random after that)
    // [SerializeField]
    // float maxTimeNewEvents = 2.0f; //Longest waiting time for new clients
    // bool eventSpawnReady = false;

    [SerializeField]
    string EventRessourceFolder = "Events"; //Ressource folder w/ events prefabs
    private Object[] events;
    GameObject EventContainer=null;
    List<int> eventIDs = new List<int>(); //List of active event ID

    private Dictionary<int,IEnumerator> coroutines= new Dictionary<int,IEnumerator>(); //Dict of EventManager coroutines associated to each client ID

    //Spawn an event near position with a probability of spawnChance%
    public void spawnEvent(Vector2 position, float spawnChance = 100.0f)
    {
        Vector3 spawnPoint;
        if (Random.Range(0.0f, 99.9f)<spawnChance && eventIDs.Count<nbMaxEvents && RandomPoint(position, SpawnRange, out spawnPoint))
        {
            // Debug.DrawRay(spawnPoint, Vector3.up, Color.blue, 2.0f);
            int prefabChoice = Random.Range(0, events.Length);
            GameObject newEvent = Instantiate((GameObject)events[prefabChoice], spawnPoint, Quaternion.identity, EventContainer.transform); //Instantiate new event inside ClientManager
            eventIDs.Add(newEvent.GetInstanceID()); //Save ID
            newEvent.name = newEvent.name.Split('(')[0]+eventIDs[eventIDs.Count-1]; //Rename new client

            // eventSpawnTimer=Random.Range(1.0f, maxTimeNewEvents); //Need more random ?
            // eventSpawnReady=false;
        }
    }

    //Remove an event from the EventManager
    public void destroyEvent(GameObject eventObj)
    {
        eventIDs.Remove(eventObj.GetInstanceID());
    }

    //Start an event coroutine for client
    public void startCoroutine(GameObject client)
    {
        Debug.Log("EventManager: Start coroutine "+client.name);
        int clientID = client.GetInstanceID();

        if(coroutines.ContainsKey(clientID)) //Stop previous coroutine of the client
            StopCoroutine(coroutines[clientID]);

        IEnumerator newCoroutine = clientCoroutine(client.transform.position);
        coroutines[clientID]=newCoroutine;
        StartCoroutine(newCoroutine);
    }

    //Stop the event coroutine for client
    public void stopCoroutine(GameObject client)
    {
        int clientID = client.GetInstanceID();

        if(coroutines.ContainsKey(clientID))
        {
            Debug.Log("EventManager: Stop coroutine "+client.name);
            StopCoroutine(coroutines[clientID]);
        }
    }

    //InvokeRepeating() is another option
    //Coroutine to be started in a parallel process.
    private IEnumerator clientCoroutine(Vector2 position) 
    {
        while(EventManager.Instance!=null){
            if(GameSystem.Instance.serviceOpen)
                EventManager.Instance.spawnEvent(position, spawnChance);
            yield return new WaitForSeconds(1.0f); //Called every second
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
        // if(!eventSpawnReady)
        // {
        //     eventSpawnTimer-= Time.deltaTime;
        //     if(eventSpawnTimer<=0)
        //         eventSpawnReady=true;
        // }
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }

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
}
