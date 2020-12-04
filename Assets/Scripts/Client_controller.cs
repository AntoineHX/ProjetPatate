using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Client_controller : MonoBehaviour
{
    public float consumeTime = 3.0f; //Time to consume currentMug
    public float waitingTime = 10.0f; //Patience after ordering
    GameObject currentMug;

    public bool use(GameObject new_mug)
    {
        if(new_mug != null && new_mug.tag=="Mug")
        {
            Debug.Log(gameObject.name+" take "+new_mug.name);
            currentMug = new_mug;
            return true; //Object taken from tavernkeeper
        }
        else
        {
            Debug.Log("Display order (or something else) of "+gameObject.name);
            return false; //Object not taken from tavernkeeper
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //Needs to be on Interactions layer and have the Client tag to work properly
        gameObject.tag = "Client"; //Force gameobject tag
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
