using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//Define the system managing the clients. (Singleton)
public sealed class ClientManager : MonoBehaviour
{
    int currentNbClient = 0;
    int nbMaxClients = 1;
    bool clientSpawnReady = false;
    float clientSpawnTimer = 2.0f; //Intial time before first spawn (pseudo-random after that)
    float maxTimeNewClients = 10.0f;

    string ClientRessourceFolder = "Clients";
    Vector3 spawnPosition = new Vector3(0, 0, 0);
    private Object[] clients;

    //Request new client
    //Return wether a new client was created
    public bool clientRequest()
    {
        if(clientSpawnReady && currentNbClient<nbMaxClients)
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

    // Start is called before the first frame update
    void Start()
    {
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
