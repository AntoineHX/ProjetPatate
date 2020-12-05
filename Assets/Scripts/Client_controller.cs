using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Define the behavior of a client
[RequireComponent(typeof(Collider2D))]
public class Client_controller : MonoBehaviour
{
    public float consumeTime = 3.0f; //Time to consume currentMug
    public float waitingTime = 10.0f; //Patience after ordering
    GameObject currentMug = null; //Mug currently held by the client

    //Handle objects interactions w/ Workshop
    //Return wether the object is taken from tavernkeeper
    public bool use(GameObject object_used)
    {
        if(currentMug is null) //No mug in hand
        {
            if(object_used != null && object_used.tag=="Mug")
            {
                Mug mug = object_used.GetComponent<Mug>();
                if (mug!= null && mug.content != null)
                {
                    Debug.Log(gameObject.name+" take "+object_used.name+ " of "+mug.content.Type);
                    currentMug = object_used;
                    return true;
                }
                else
                {
                    Debug.Log("Display order (or something else) of "+gameObject.name);
                    return false;
                }
            }
            else
            {
                Debug.Log("Display order (or something else) of "+gameObject.name);
                return false;
            }
        }
        else
        {
            Debug.Log(gameObject.name+" already consumming "+currentMug.name);
            return false;
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
