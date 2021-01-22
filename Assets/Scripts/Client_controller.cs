using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Define the behavior of a client
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NavMeshObstacle))]
public class Client_controller : MonoBehaviour, IUsable
{
    public float consumeTime = 3.0f; //Time to consume currentMug
    public float waitingTime = 10.0f; //Patience after reaching seat
    public UITimer UIWaitingTimer = null;

    Animator animator;
    string _status;
    string _prevStatus;
    HashSet<string> _availStatus = new HashSet<string>(){"entering", "waiting", "consuming", "leaving", "event"};
    public string status
    { 
        get{ return _status;}
        set{
            if (_availStatus.Contains(value))
                _prevStatus=_status;
                _status = value;
                animator.SetTrigger(_status); //Update status in animator
                // Debug.Log(gameObject.name+" "+_status);
                switch (value)
                {
                    case "entering":
                        navObstacle.enabled = false;
                        agent.enabled = true;
                        if(UIWaitingTimer != null)
                            UIWaitingTimer.gameObject.SetActive(false);
                        break;
                    case "waiting": 
                        EventManager.Instance.startCoroutine(gameObject);
                        //Switch Agent to obstacle if waiting
                        agent.Warp(assigedPos); //Make sure agent become static at right position
                        agent.enabled = false;
                        navObstacle.enabled = true;

                        if(UIWaitingTimer != null)
                        {
                            UIWaitingTimer.DisplayIcon(true);
                            UIWaitingTimer.SetValue(1.0f);
                            UIWaitingTimer.gameObject.SetActive(true);
                        }
                        break;
                    case "consuming":
                        EventManager.Instance.startCoroutine(gameObject);
                        if(UIWaitingTimer != null)
                            UIWaitingTimer.gameObject.SetActive(false);
                        break;
                    case "event":
                    case "leaving":
                        EventManager.Instance.stopCoroutine(gameObject);
                        navObstacle.enabled = false;
                        agent.enabled = true;
                        if(UIWaitingTimer != null)
                            UIWaitingTimer.gameObject.SetActive(false);
                        break;
                    default:
                        break;
                }
                // navObstacle.enabled = value=="waiting";
                // agent.enabled = value!="waiting";
        }
    }
    
    string order = "none"; //Order requested by the client
    float consumeTimer;
    float waitTimer;
    GameObject currentMug = null; //Mug currently held by the client

    //Navigation
    Vector2 assigedPos; //Chair to sit or destination to stay (leave)
    NavMeshAgent agent;
    NavMeshObstacle navObstacle; //Obstacle for other agents

    //Handle objects interactions w/ Workshop
    //Return wether the object is taken from tavernkeeper
    public bool use(GameObject object_used)
    {
        if(status == "waiting" && currentMug is null) //No mug in hand
        {
            //TODO : Gérer Grabale qui ne sont pas des Mugs ?
            if(object_used != null && object_used.tag=="Grabable")
            {
                Mug mug = object_used.GetComponent<Mug>();
                if (mug!= null && mug.content != null && mug.content.Type==order)
                {
                    status = "consuming";
                    Debug.Log(gameObject.name+" "+status+" "+object_used.name+ " of "+mug.content.Type);
                    currentMug = object_used;
                    consumeTimer=consumeTime;
                    mug.take();
                    return true;
                }
                else
                {
                    Debug.Log(gameObject.name+" doesn't want that "+object_used.name+" - Request : "+order);
                    return false;
                }
            }
            else
            {
                Debug.Log(gameObject.name+" doesn't want that "+object_used.name+" - Request : "+order);
                return false;
            }
        }
        else
        {
            Debug.Log(gameObject.name+" already consumming "+currentMug.name);
            return false;
        }
    }

    //Assign client to an event/destination. Restore its previous status if no event/destination is given.
    public void assignToEvent(Vector2? destination=null)
    {
        if(destination is null)
        {
            status=_prevStatus;
            // NavMeshHit hit;
            // NavMesh.SamplePosition(gameObject.transform.position, out hit, agent.height*2, NavMesh.AllAreas);
            // agent.Warp(hit.position);
            if(agent.enabled)
                agent.SetDestination(assigedPos);
            else
                gameObject.transform.position=assigedPos;
        }
        else
        {
            status="event";
            agent.SetDestination((Vector2)destination);
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

        animator = GetComponent<Animator>();

        // Navigation //
        agent = GetComponent<NavMeshAgent>();
        navObstacle = GetComponent<NavMeshObstacle>();

        //Prevent rotation of the ground at movement
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        //Get target
        assigedPos = ClientManager.Instance.assignTarget(); //Chair to go
        agent.SetDestination(assigedPos);
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
            order = ClientManager.Instance.assignOrder();
            if(UIWaitingTimer != null) //Update UI Waiting timer Icon
                UIWaitingTimer.DisplayIcon(StockManager.Instance.consumableSprite(order));
        }

        else if(status=="waiting")
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer < 0) //Waited too long
            {
                //Leave tavern
                status = "leaving";
                assigedPos = ClientManager.Instance.assignTarget(assigedPos, true); //Request leaving target
                agent.SetDestination(assigedPos); 
            }
            else if(UIWaitingTimer != null) //Update UI Waiting timer
                UIWaitingTimer.SetValue(waitTimer/waitingTime);
        }

        //Consume Timer
        else if(status=="consuming") //Consuming mug if there's one and reached destination
        {
            consumeTimer -= Time.deltaTime;
            if(consumeTimer < 0) //Finished consuming mug ?
            {
                Mug obj = currentMug.GetComponent<Mug>();
                if(obj !=null)
                {
                    //Reward
                    Consumable content = obj.consume();
                    int money = (int)(content.Value*(1.0f+waitTimer/waitingTime)); //Reward = value order +  Tips (value * waitTime) 
                    ClientManager.Instance.clientReward(money);

                    //Drop mug
                    Transform dropPos = gameObject.transform;
                    dropPos.position += (Vector3)Vector2.down * 0.2f;
                    obj.drop(dropPos);
                    currentMug=null;
                }

                //Leave tavern
                status = "leaving";
                assigedPos = ClientManager.Instance.assignTarget(assigedPos, true); //Request leaving target
                agent.SetDestination(assigedPos); 
            }
        }

        else if(status=="leaving" && !agent.pathPending && agent.remainingDistance==0)
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        ClientManager.Instance.clientLeave(gameObject);
    }
}
