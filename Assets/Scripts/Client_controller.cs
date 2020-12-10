using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Define the behavior of a client
[RequireComponent(typeof(Collider2D))]
public class Client_controller : MonoBehaviour, IUsable
{
    public float consumeTime = 3.0f; //Time to consume currentMug
    public float waitingTime = 10.0f; //Patience after ordering
    
    float consumeTimer;
    GameObject currentMug = null; //Mug currently held by the client

    //Handle objects interactions w/ Workshop
    //Return wether the object is taken from tavernkeeper
    public bool use(GameObject object_used)
    {
        if(currentMug is null) //No mug in hand
        {
            //TODO : Gérer Grabale qui ne sont pas des Mugs ?
            if(object_used != null && object_used.tag=="Grabable")
            {
                Mug mug = object_used.GetComponent<Mug>();
                if (mug!= null && mug.content != null)
                {
                    Debug.Log(gameObject.name+" take "+object_used.name+ " of "+mug.content.Type);
                    currentMug = object_used;
                    consumeTimer=consumeTime;
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
        if(gameObject.layer != LayerMask.NameToLayer("Interactions"))
            Debug.LogWarning(gameObject.name+" layer should be set to 'Interactions' to work properly");
        if(gameObject.tag != "Usable")
            Debug.LogWarning(gameObject.name+" tag should be set to 'Usable' to work properly");
    }

    // Update is called once per frame
    void Update()
    {
        //Timer
        if (currentMug!= null) //Consuming mug if there's one
        {
            consumeTimer -= Time.deltaTime;
            if (consumeTimer < 0) //Finished consuming mug ?
            {
                Mug obj = currentMug.GetComponent<Mug>();
                if(obj !=null)
                {
                    obj.consume();
                    //Drop mug
                    obj.drop(gameObject.transform.position+ (Vector3)Vector2.down * 0.2f);
                    currentMug=null;
                }
            }
        }
    }
}
