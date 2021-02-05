using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Define the system managing the events. (Singleton)
//TODO: Switch to a registering approach for events
public sealed class EventManager : MonoBehaviour
{
    public static string EventManager_path="/GameSystem/EventManager";
    //Singleton
    private static EventManager _instance=null;
    public static EventManager Instance { get 
        { 
        if(_instance is null) //Force Awakening if needed
            GameObject.Find(EventManager_path).GetComponent<EventManager>().Awake();
        return _instance; 
        } 
    }

    [HideInInspector]
    public bool ready = false; //Wether the EventManager is initialized

    [SerializeField]
    float SpawnRange = 1.0f; //Range of an event spawn from its origin (real max distance = 2*range)
    [SerializeField]
    float spawnChanceSoft = 100.0f, spawnChanceHard = 100.0f; //Probability of an event to spawn (%)
    [SerializeField]
    int maxSoftObs = 1, maxHardObs = 1; //Maximum active events
    
    // [SerializeField]
    // float eventSpawnTimer = 0.5f; //Intial time before first spawn (pseudo-random after that)
    // [SerializeField]
    // float maxTimeNewEvents = 2.0f; //Longest waiting time for new clients
    // bool eventSpawnReady = false;

    [SerializeField]
    string SoftObsRessourceFolder = "Events/SoftObstacles", HardObsRessourceFolder = "Events/HardObstacles"; //Ressource folder w/ events prefabs
    private Dictionary<string,Object[]> eventPrefabs = new Dictionary<string,Object[]>();
    GameObject EventContainer=null;

    //List of active event
    List<GameObject> softObsList = new List<GameObject>(); 
    List<GameObject> hardObsList = new List<GameObject>();

    [SerializeField]
    float coroutineRefreshRate = 1.0f; //Time (s) before refreshing a coroutine
    private Dictionary<int,IEnumerator> coroutines= new Dictionary<int,IEnumerator>(); //Dict of EventManager coroutines associated to each client ID

    //Return the maximum number of events ("Soft"; "Hard")
    public int MaxEvents(string types="SoftHard")
    {
        int res=0;
        if(types.Contains("Soft"))
            res+=maxSoftObs;
        if(types.Contains("Hard"))
            res+=maxHardObs;
        
        return res;
    }
    //Return the current number of events ("Soft"; "Hard")
    public int eventCount(string types="SoftHard")
    {
        int res=0;
        if(types.Contains("Soft"))
            res+=softObsList.Count;
        if(types.Contains("Hard"))
            res+=hardObsList.Count;
        
        return res;
    }

    //Spawn a soft obstacle near position with a probability of spawnChance%
    public void spawnSoftObs(Vector2 position, float spawnChance = 100.0f)
    {
        Vector3 spawnPoint;
        if (Random.Range(0.0f, 99.9f)<spawnChance && softObsList.Count<maxSoftObs && RandomPoint(position, SpawnRange, out spawnPoint))
        {
            // Debug.DrawRay(spawnPoint, Vector3.up, Color.blue, 2.0f);
            int prefabChoice = Random.Range(0, eventPrefabs["soft"].Length);
            GameObject newEvent = Instantiate((GameObject)eventPrefabs["soft"][prefabChoice], spawnPoint, Quaternion.identity, EventContainer.transform); //Instantiate new event inside ClientManager
            softObsList.Add(newEvent); //Save event to list
            newEvent.name = newEvent.name.Split('(')[0]+newEvent.GetInstanceID(); //Rename new event

            // eventSpawnTimer=Random.Range(1.0f, maxTimeNewEvents); //Need more random ?
            // eventSpawnReady=false;
        }
    }
    //Spawn a hard obstacle near position with a probability of spawnChance%, and assign targetClients to it
    public void spawnHardObs(List<Client_controller> targetClients, Vector2 position, float spawnChance = 100.0f)
    {
        //TODO: Orienté client vers event + prefab 
        Vector3 spawnPoint;
        if (Random.Range(0.0f, 99.9f)<spawnChance && hardObsList.Count<maxHardObs && RandomPoint(position, SpawnRange, out spawnPoint))
        {
            // Debug.DrawRay(spawnPoint, Vector3.up, Color.blue, 2.0f);
            int prefabChoice = Random.Range(0, eventPrefabs["hard"].Length);
            GameObject newEvent = Instantiate((GameObject)eventPrefabs["hard"][prefabChoice], spawnPoint, Quaternion.identity, EventContainer.transform); //Instantiate new event inside ClientManager
            hardObsList.Add(newEvent); //Save event to list
            newEvent.name = newEvent.name.Split('(')[0]+newEvent.GetInstanceID(); //Rename new event

            foreach(Client_controller client in targetClients)
                client.assignToEvent(spawnPoint);
        }
    }

    //Remove an event from the EventManager
    public void removeEvent(GameObject eventObj)
    {
        softObsList.Remove(eventObj);
        hardObsList.Remove(eventObj);
    }

    //Destroy registered events. Levels : 2 = All events, 1 = Hard Obstacles
    public void cleanUp(int level=2)
    {
        if(level<1)
            Debug.Log("EventManager : Called cleanup w/ level inferior to 1. Nothing was done.");
        if(level>0) //Clean HardObstacles
        {
            foreach(GameObject obs in hardObsList)
                Destroy(obs);
            hardObsList.Clear();
        }
        if(level>1) //Clean SoftObstacles
        {
            foreach(GameObject obs in softObsList)
                Destroy(obs);
            softObsList.Clear();
        }
    }

    //Start an event coroutine for client
    public void startCoroutine(GameObject client)
    {
        // Debug.Log("EventManager: Start coroutine "+client.name);
        int clientID = client.GetInstanceID();

        if(coroutines.ContainsKey(clientID)) //Stop previous coroutine of the client
            StopCoroutine(coroutines[clientID]);

        IEnumerator newCoroutine = clientCoroutine(client);
        coroutines[clientID]=newCoroutine;
        StartCoroutine(newCoroutine);
    }

    //Stop the event coroutine for client
    public void stopCoroutine(GameObject client)
    {
        int clientID = client.GetInstanceID();

        if(coroutines.ContainsKey(clientID))
        {
            // Debug.Log("EventManager: Stop coroutine "+client.name);
            StopCoroutine(coroutines[clientID]);
        }
    }

    //InvokeRepeating() is another option
    //Coroutine to be started in a parallel process.
    private IEnumerator clientCoroutine(GameObject clientObj) 
    {
        Client_controller client = clientObj.GetComponent<Client_controller>();
        while(EventManager.Instance!=null){
            if(GameSystem.Instance.serviceOpen)
            {
                //Try to spawn softObs or hardObs randomly
                if(Random.value<0.5f)
                    if(client.status=="consuming") //Only spawn soft obs while consuming
                        EventManager.Instance.spawnSoftObs(clientObj.transform.position, spawnChanceSoft);
                else
                {
                    List<GameObject> otherClients = findNearClients(clientObj, 1.0f);
                    if(otherClients.Count>0)
                    {
                        foreach(GameObject ocl in otherClients)
                            Debug.DrawLine(clientObj.transform.position, ocl.transform.position, Color.red, coroutineRefreshRate);
                        // Debug.Log("Clients near");

                        GameObject tgtClient = otherClients[Random.Range(0, otherClients.Count)];
                        //TODO : Compute spawnChance w/ clients happiness
                        Vector2 eventPos=(clientObj.transform.position+tgtClient.transform.position)/2; //Event pos between clients
                        List<Client_controller> targetClients = new List<Client_controller>(){client, tgtClient.GetComponent<Client_controller>()};
                        EventManager.Instance.spawnHardObs(targetClients, eventPos, spawnChanceHard);
                    }
                }
            }
            yield return new WaitForSeconds(coroutineRefreshRate); //Called every coroutineRefreshRate second
        }
    }

    //Return the list of other clients in range of client. The client in event status are ignored.
    List<GameObject> findNearClients(GameObject client, float range)
    {
        List<GameObject> res = new List<GameObject>();
        List<GameObject> clientList = ClientManager.Instance.clientList;
        Vector3 originPos = client.transform.position;
        Vector3 otherPos;
        foreach(GameObject cl in clientList)
        {
            otherPos=cl.transform.position;
            if(Vector2.Distance(originPos, otherPos)<range && originPos!=otherPos)
                if(cl.GetComponent<Client_controller>().status!="event")//Ignore clients already in event status
                    res.Add(cl);
        }
        return res;
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
            EventContainer = GameObject.Find(EventManager_path);
            if (EventContainer is null)
                throw new System.Exception("No EventManager found under GameSystem");

            eventPrefabs["soft"] = Resources.LoadAll(SoftObsRessourceFolder);
            eventPrefabs["hard"] = Resources.LoadAll(HardObsRessourceFolder);
            if (eventPrefabs["soft"].Length==0)
            {
                Debug.LogWarning("EventManager didn't find events prefab in ressource folder : "+SoftObsRessourceFolder);
            }
            if (eventPrefabs["hard"].Length==0)
            {
                Debug.LogWarning("EventManager didn't find events prefab in ressource folder : "+HardObsRessourceFolder);
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
        cleanUp();
    }
}
