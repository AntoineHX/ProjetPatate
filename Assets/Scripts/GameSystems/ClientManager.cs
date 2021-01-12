﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//Define the system managing the clients. (Singleton)
public sealed class ClientManager : MonoBehaviour
{
    int nbMaxClients = 1;
    bool clientSpawnReady = false;
    float clientSpawnTimer = 0.5f; //Intial time before first spawn (pseudo-random after that)
    float maxTimeNewClients = 2.0f;

    string ClientRessourceFolder = "Clients";
    private Object[] clients;
    GameObject ClientContainer = null;
    List<int> clientIDs = new List<int>();

    Vector2 spawnPoint;
    Dictionary<Vector2, bool> targets_dict; //Dict with target and wether they're taken by a client

    //Request new client
    //Return wether a new client was created
    public bool clientRequest()
    {
        if(clientSpawnReady && clientIDs.Count<nbMaxClients && targets_dict.ContainsValue(false))
        {
            int prefabChoice = Random.Range(0, clients.Length);
            GameObject newClient = Instantiate((GameObject)clients[prefabChoice], spawnPoint, Quaternion.identity, ClientContainer.transform); //Instantiate new client inside ClientManager
            clientIDs.Add(newClient.GetInstanceID()); //Save ID
            // Debug.Log(newClient.GetInstanceID());
            newClient.name = newClient.name.Split('(')[0]+clientIDs[clientIDs.Count-1]; //Rename new client

            clientSpawnTimer=Random.Range(1.0f, maxTimeNewClients); //Need more random ?
            clientSpawnReady=false;

            // Debug.Log("Spawning "+clientPrefab.name+" at "+spawnPosition);
            return true; //New client instantiated
        }
        return false; //No new client
    }

    //TODO: Reputation
    public void clientReward(int money)
    {
        GameSystem.Instance.gold+=money;
    }

    //Destroy a client
    public void clientLeave(GameObject client)
    {
        clientIDs.Remove(-int.Parse(client.name.Split('-')[1]));
        Destroy(client);
        // Debug.Log(client.name+" destroyed"+clientIDs.Count);
        
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
    public string assignOrder()
    {
        return "beer";
    }

    // Start is called before the first frame update
    void Start()
    {
        ClientContainer = GameObject.Find("/GameSystem/ClientManager");
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

        clients = Resources.LoadAll(ClientRessourceFolder);

        // foreach (var c in clients)
        // {
        //     Debug.Log(gameObject.name+" : "+c.name + " loaded");
        // }
        if (clients.Length<nbMaxClients)
        {
            Debug.LogWarning("ClientManager doesn't have enough client prefab to manage unique MaxClients : "+clients.Length+"/"+nbMaxClients);
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
    }

    // Update is called once per frame
    void Update()
    {
        if(!clientSpawnReady)
        {
            clientSpawnTimer-= Time.deltaTime;
            if(clientSpawnTimer<=0)
                clientSpawnReady=true;
        }
        // Debug.Log("Client Spawn : "+clientSpawnTimer+" / Seat available: "+targets_dict.ContainsValue(false));
    }

    //// Singleton Implementation (https://jlambert.developpez.com/tutoriels/dotnet/implementation-pattern-singleton-csharp/#LIII) ////
    private ClientManager()
    {
    }

    public static ClientManager Instance { get { return Nested.instance; } }
        
    private class Nested
    {
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Nested()
        {
        }

        internal static readonly ClientManager instance = new GameObject("ClientManager").AddComponent<ClientManager>();
    }
    ////
}
