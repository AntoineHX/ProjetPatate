﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//Define the system managing the clients. (Singleton)
public sealed class ClientManager : MonoBehaviour
{
    int currentNbClient = 0;
    int nbMaxClients = 1;
    bool clientSpawnReady = false;
    float clientSpawnTimer = 0.5f; //Intial time before first spawn (pseudo-random after that)
    float maxTimeNewClients = 10.0f;

    string ClientRessourceFolder = "Clients";
    private Object[] clients;

    Vector3 spawnPosition = new Vector3(0, 0, 0); //TODO : Use gameObject 
    Dictionary<Transform, bool> targets_dict; //Dict with target and wether they're taken by a client

    //Request new client
    //Return wether a new client was created
    public bool clientRequest()
    {
        if(clientSpawnReady && currentNbClient<nbMaxClients && targets_dict.ContainsValue(false))
        {
            GameObject newClient = (GameObject)clients[Random.Range(0, clients.Length)];
            // Debug.Log("Spawning "+clientPrefab.name+" at "+spawnPosition);
            Instantiate(newClient, spawnPosition, Quaternion.identity);
            currentNbClient+=1;
            clientSpawnTimer=Random.Range(1.0f, maxTimeNewClients); //Need more random ?
            clientSpawnReady=false;

            return true; //New client instantiated
        }
        return false; //No new client
    }

    //Assign a random available target
    public Transform assignTarget()
    {
        List<Transform> avail_tgt = new List<Transform>();
        foreach(KeyValuePair<Transform, bool> tgt in targets_dict)
            if(tgt.Value is false)
                avail_tgt.Add(tgt.Key);
        
        Transform target = avail_tgt[Random.Range(0, avail_tgt.Count)];
        targets_dict[target]=true;
        return target;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Load clients prefabs //

        // Find all assets labelled with 'usable' :
        // string[] guids = AssetDatabase.FindAssets("", new string[] {"Assets/Prefabs/Characters/Clients"});

        // foreach (string guid in guids)
        // {
        //     Debug.Log(AssetDatabase.GUIDToAssetPath(guid));
        //     Instantiate(guid, spawnPosition, Quaternion.identity);
        // }

        clients = Resources.LoadAll(ClientRessourceFolder);

        foreach (var c in clients)
        {
            Debug.Log(gameObject.name+" : "+c.name + " loaded");
        }

        // Load Client spawn point //
        GameObject spawnObj = GameObject.Find("/GameSystem/ClientSpawn");
        if (spawnObj is null)
            throw new System.Exception("No ClientSpawn GameObject found under GameSystem");
        spawnPosition = spawnObj.transform.position;

        // Load Client targets //
        targets_dict = new Dictionary<Transform, bool>();
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
                    targets_dict.Add(target, false);
                    Debug.Log("Client target : "+ target.gameObject.name + target.position);
                }
            }
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
