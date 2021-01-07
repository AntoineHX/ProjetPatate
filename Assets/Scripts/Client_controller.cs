using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Define the behavior of a client
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NavMeshObstacle))]
public class Client_controller : MonoBehaviour, IUsable
{
    public float consumeTime = 3.0f; //Time to consume currentMug
    public float waitingTime = 10.0f; //Patience after reaching seat
    public UITimer UIWaitingTimer = null;

    string _status;
    HashSet<string> _availStatus = new HashSet<string>(){"entering", "waiting", "consuming", "leaving"};
    public string status
    { 
        get{ return _status;}
        set{
            if (_availStatus.Contains(value))
                _status = value;
                Debug.Log(gameObject.name+" "+_status);

                //Switch Agent to obstacle if waiting
                if(value=="waiting")
                {
                    agent.Warp(agent.destination); //Make sure agent become static at right position
                    agent.enabled = false;
                    navObstacle.enabled = true;
                }
                else
                {
                    navObstacle.enabled = false;
                    agent.enabled = true;
                }
                // navObstacle.enabled = value=="waiting";
                // agent.enabled = value!="waiting";
        }
    }
    
    float consumeTimer;
    float waitTimer;
    GameObject currentMug = null; //Mug currently held by the client

    //Navigation
    Vector2 destination;
    NavMeshAgent agent;
    NavMeshObstacle navObstacle; //Obstacle for other agents

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
                    status = "consuming";
                    Debug.Log(gameObject.name+" "+status+" "+object_used.name+ " of "+mug.content.Type);
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

        if(UIWaitingTimer is null)
            Debug.LogWarning(gameObject.name+" doesn't have a UIWaitingTimer set");
        else
            UIWaitingTimer.gameObject.SetActive(false);

        // Navigation //
        agent = GetComponent<NavMeshAgent>();
        navObstacle = GetComponent<NavMeshObstacle>();

        //Prevent rotation of the ground at movement
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        //Get target
        agent.SetDestination(ClientManager.Instance.assignTarget());
        //Assign Random priority to prevent two agent blocking each other
        agent.avoidancePriority=Random.Range(0, 99);

        status = "entering";
    }

    // Update is called once per frame
    void Update()
    {
        //Navigation
        // Debug.Log(gameObject.name + " navigation : "+ agent.isStopped + " " + agent.remainingDistance);

        if(status=="entering" && !agent.pathPending && agent.remainingDistance==0) //Reached seat ?
        {
            status="waiting";
            waitTimer=waitingTime;
            if(UIWaitingTimer != null)
            {
                UIWaitingTimer.DisplayIcon(true);
                UIWaitingTimer.SetValue(1.0f);
                UIWaitingTimer.gameObject.SetActive(true);
            }
        }

        if(status=="waiting")
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer < 0) //Waited too long
            {
                //Disable UI Waiting timer
                if(UIWaitingTimer != null)
                    UIWaitingTimer.gameObject.SetActive(false);
                //Leave tavern
                status = "leaving";
                agent.SetDestination(ClientManager.Instance.assignTarget(agent.destination)); //Request next target
            }
            else if(UIWaitingTimer != null) //Update UI Waiting timer
                UIWaitingTimer.SetValue(waitTimer/waitingTime);
        }

        //Consume Timer
        //TODO : Make Client Obstacle when consumming
        if(status=="consuming" && !agent.pathPending && agent.remainingDistance==0) //Consuming mug if there's one and reached destination
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

                //Leave tavern
                status = "leaving";
                agent.SetDestination(ClientManager.Instance.assignTarget(agent.destination)); //Request next target
            }
        }

        if(status=="leaving" && !agent.pathPending && agent.remainingDistance==0)
        {
            ClientManager.Instance.clientLeave(gameObject);
        }
    }
}
