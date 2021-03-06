﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


//Define the system managing the clients. (Singleton)
//TODO: Switch to a registering approach for clients
public sealed class ClientManager : MonoBehaviour
{
    public static string ClientManager_path="/GameSystem/ClientManager";
    //Singleton
    private static ClientManager _instance=null;
    public static ClientManager Instance { get 
        { 
        if(_instance is null) //Force Awakening if needed
            GameObject.Find(ClientManager_path).GetComponent<ClientManager>().Awake();
        return _instance; 
        } 
    }

    [HideInInspector]
    public bool ready = false; //Wether the ClientManager is initialized

    [SerializeField]
    int nbMaxClients= 1; //Maximum active clients
    public int MaxClients{ get{return nbMaxClients;} private set{nbMaxClients=value;}} //Accessor nbMaxClients
    [SerializeField]
    float clientSpawnChance = 100.0f; //Chance of new client every request
    [SerializeField]
    float clientFrequency = 1.0f; //Time (s) between clientRequest
    // [SerializeField]
    // float clientSpawnTimer = 0.5f; //Intial time before first spawn (pseudo-random after that)
    // [SerializeField]
    // float maxTimeNewClients = 2.0f; //Longest waiting time for new clients
    // bool clientSpawnReady = false;

    [SerializeField]
    string ClientRessourceFolder = "Clients";
    private Object[] clientsPrefab;
    GameObject ClientContainer = null;

    private List<GameObject> _clientList = new List<GameObject>();
    public List<GameObject> clientList
    {
        get{return _clientList;}
        private set{_clientList=value;}
    }

    Vector2 spawnPoint;
    Dictionary<Vector2, bool> targets_dict; //Dict with target and wether they're taken by a client

    private List<IEnumerator> coroutines= new List<IEnumerator>(); //List of ClientManager coroutines

    //Request new client
    //Return wether a new client was created
    public bool clientRequest(float SpawnChance=100.0f)
    {
        if(Random.Range(0.0f, 99.9f)<SpawnChance && clientList.Count<nbMaxClients && targets_dict.ContainsValue(false))
        {
            int prefabChoice = Random.Range(0, clientsPrefab.Length);
            GameObject newClient = Instantiate((GameObject)clientsPrefab[prefabChoice], spawnPoint, Quaternion.identity, ClientContainer.transform); //Instantiate new client inside ClientManager
            // Debug.Log(newClient.GetInstanceID());
            newClient.name = newClient.name.Split('(')[0]+newClient.GetInstanceID(); //Rename new client

            clientList.Add(newClient); //Save client ref

            // clientSpawnTimer=Random.Range(1.0f, maxTimeNewClients); //Need more random ?
            // clientSpawnReady=false;

            // Debug.Log("Spawning "+clientPrefab.name+" at "+spawnPosition);
            return true; //New client instantiated
        }
        return false; //No new client
    }

    //TODO: Reputation
    public void clientReward(int money)
    {
        GameSystem.Instance.Gold+=money;
    }

    //Destroy a client
    public void clientLeave(GameObject client)
    {
        clientList.Remove(client);
        // Debug.Log(client.name+" destroyed"+clientIDs.Count);
        
        //Prevent immediate spawn of a new client after one leaving
        // if(clientSpawnReady)
        // {
        //     clientSpawnReady=false;
        //     clientSpawnTimer+=0.5f;
        // }
    }

    //Return a random available target. Or the Exit if Exit=True.
    //prevTarget : Previous target of the client which will be available for other clients.
    public Vector2 assignTarget(Vector2? prevTarget=null, bool exit=false)
    {
        if(prevTarget != null)
        {
            targets_dict[(Vector2)prevTarget]=false; //Free prevTarget
        }
        if(exit) //Assign Exit target
            return spawnPoint;
        else
        {
            List<Vector2> avail_tgt = new List<Vector2>();
            foreach(KeyValuePair<Vector2, bool> tgt in targets_dict)
                if(tgt.Value is false)
                    avail_tgt.Add(tgt.Key);
            
            Vector2 target = avail_tgt[Random.Range(0, avail_tgt.Count)];
            targets_dict[target]=true;
            return target;
        }
    }

    //Return a random order from available consummable
    //TODO : Check stock before assignement ?
    public string assignOrder()
    {
        List<string> available_types = new List<string>(Consumable.allowed_types);

        string order_type = available_types[Random.Range(0, available_types.Count)];
        return order_type;
    }

    //InvokeRepeating() is another option
    //Coroutine to be started in a parallel process. It'll repeatidly request new client.
    private IEnumerator requestCoroutine() 
    {
        while(ClientManager.Instance!=null){
            if(GameSystem.Instance.serviceOpen)
                ClientManager.Instance.clientRequest(clientSpawnChance);
            yield return new WaitForSeconds(clientFrequency);
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
            ClientContainer = GameObject.Find(ClientManager_path);
            if (ClientContainer is null)
                throw new System.Exception("No ClientManager found under GameSystem");

            // Load clients prefabs //

            // Find all assets labelled with 'usable' :
            // string[] guids = AssetDatabase.FindAssets("", new string[] {"Assets/Prefabs/Characters/Clients"});

            // foreach (string guid in guids)
            // {
            //     Debug.Log(AssetDatabase.GUIDToAssetPath(guid));
            //     Instantiate(guid, spawnPosition, Quaternion.identity);
            // }

            clientsPrefab = Resources.LoadAll(ClientRessourceFolder);

            // foreach (var c in clients)
            // {
            //     Debug.Log(gameObject.name+" : "+c.name + " loaded");
            // }
            if (clientsPrefab.Length<nbMaxClients)
            {
                Debug.LogWarning("ClientManager doesn't have enough client prefab to manage unique MaxClients : "+clientsPrefab.Length+"/"+nbMaxClients);
            }

            // Load Client spawn point //
            GameObject spawnObj = GameObject.Find("/GameSystem/ClientSpawn");
            if (spawnObj is null)
                throw new System.Exception("No ClientSpawn GameObject found under GameSystem");
            spawnPoint = spawnObj.transform.position;

            // Load Client targets //
            targets_dict = new Dictionary<Vector2, bool>();
            GameObject targetsObj = GameObject.Find("/GameSystem/Targets");
            if (targetsObj is null)
                throw new System.Exception("No Targets GameObject found under GameSystem");

            Component[] targets = targetsObj.GetComponentsInChildren<Transform>();
            if(targets != null)
            {
                foreach(Transform target in targets)
                {
                    if(target.gameObject.name != "Targets")
                    {
                        targets_dict.Add(target.position, false);
                        // Debug.Log("Client target : "+ target.gameObject.name + target.position);
                    }
                }
            }
            if (targets_dict.Count<nbMaxClients)
            {
                Debug.LogWarning("ClientManager doesn't have enough target to manage MaxClients : "+targets_dict.Count+"/"+nbMaxClients);
            }

            ready = true;
        }
    }

    void Start()
    {
        //Start coroutines in parallel
        coroutines.Add(requestCoroutine());
        foreach(IEnumerator c in coroutines)
            StartCoroutine(c);
    }

    // Update is called once per frame
    void Update()
    {
        // if(!clientSpawnReady)
        // {
        //     clientSpawnTimer-= Time.deltaTime;
        //     if(clientSpawnTimer<=0)
        //         clientSpawnReady=true;
        // }
        // Debug.Log("Client Spawn : "+clientSpawnTimer+" / Seat available: "+targets_dict.ContainsValue(false));
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }

    //// Singleton Implementation (https://jlambert.developpez.com/tutoriels/dotnet/implementation-pattern-singleton-csharp/#LIII) ////
    // private ClientManager()
    // {
    // }

    // public static ClientManager Instance { get { return Nested.instance; } }
        
    // private class Nested
    // {
    //     // Explicit static constructor to tell C# compiler
    //     // not to mark type as beforefieldinit
    //     static Nested()
    //     {
    //     }

    //     internal static readonly ClientManager instance = new GameObject("ClientManager").AddComponent<ClientManager>();
    // }
    ////
}
