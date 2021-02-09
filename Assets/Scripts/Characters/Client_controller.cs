using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Define the behavior of a client
[RequireComponent(typeof(SpriteRenderer))]
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

    SpriteRenderer client_renderer;
    Animator animator;
    string _status;
    string _prevStatus;
    string _lastStatusRequest=null;
    // private readonly object balanceLock = new object();
    // bool updatingStatus=false;
    HashSet<string> _availStatus = new HashSet<string>(){"entering", "waiting", "consuming", "leaving", "event"};
    public string status
    { 
        get{ return _status;}
        //BEWARE : Set is only a request. The status is only really set in update.
        set{
            if (_availStatus.Contains(value))
            {
                    if(value==_status)
                        Debug.LogWarning(gameObject.name+" status is set twice to:"+value);
                    else //Request change of status
                    {
                        // if(_lastStatusRequest!=null)
                        //     Debug.LogWarning(gameObject.name+" status request("+_lastStatusRequest+") is overriden by : "+value);
                        _lastStatusRequest = value;
                    }
            }
        }
    }
    
    string order = "none"; //Order requested by the client
    float consumeTimer;
    float waitTimer;
    GameObject currentMug = null; //Mug currently held by the client

    //Navigation
    Vector2 assigedPos; //Chair to sit or destination to stay (leave)
    Vector2 currentObjective; //Current destination to reach
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
            currentObjective=assigedPos;
        }
        else
        {
            status="event";
            currentObjective=(Vector2)destination;
        }
    }

    //Update client attributes in fonction of the newStatus. Should only be called once by Update.
    protected void updateStatus(string newStatus)
    {
        switch (newStatus)
        {
            case "entering":
                client_renderer.color = Color.white;
                toggleAgent(true);
                if(UIWaitingTimer != null)
                    UIWaitingTimer.gameObject.SetActive(false);
                break;
            case "waiting": 
                client_renderer.color = Color.white;
                toggleAgent(false);
                if(UIWaitingTimer != null)
                {
                    UIWaitingTimer.DisplayIcon(true);
                    UIWaitingTimer.gameObject.SetActive(true);
                }
                break;
            case "consuming":
                client_renderer.color = Color.white;
                toggleAgent(false);
                if(UIWaitingTimer != null)
                    UIWaitingTimer.gameObject.SetActive(false);
                break;
            case "event":
                client_renderer.color = Color.red;
                toggleAgent(true);
                if(UIWaitingTimer != null)
                    UIWaitingTimer.gameObject.SetActive(false);
                break;
            case "leaving":
                client_renderer.color = Color.white;
                toggleAgent(true);
                if(UIWaitingTimer != null)
                    UIWaitingTimer.gameObject.SetActive(false);
                break;
            default:
                break;
        }

        //Navigation 
        if(agent.enabled) //Assign destination
            agent.SetDestination(currentObjective);
        else //Warp to destination
            gameObject.transform.position=currentObjective;

        if(status=="event"&&!agent.enabled)
            Debug.LogWarning("Wrong status update : "+ gameObject.name + _prevStatus + status +" "+ _lastStatusRequest);
    }

    //Switch between agent or obstacle. And enable Event coroutine if obstacle.
    protected void toggleAgent(bool isAgent)
    {
        if(isAgent)
        {
            navObstacle.enabled = false;
            agent.enabled = true;
            EventManager.Instance.stopCoroutine(gameObject);
        }
        else
        {
            agent.enabled = false;
            navObstacle.enabled = true;
            EventManager.Instance.startCoroutine(gameObject);
        }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        if(gameObject.layer != LayerMask.NameToLayer("Interactions"))
            Debug.LogWarning(gameObject.name+" layer should be set to 'Interactions' to work properly");
        if(gameObject.tag != "Usable")
            Debug.LogWarning(gameObject.name+" tag should be set to 'Usable' to work properly");

        if(UIWaitingTimer is null)
            Debug.LogWarning(gameObject.name+" doesn't have a UIWaitingTimer set");
        else
            UIWaitingTimer.gameObject.SetActive(false);

        client_renderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Navigation //
        agent = GetComponent<NavMeshAgent>();
        navObstacle = GetComponent<NavMeshObstacle>();

        //Prevent rotation of the ground at movement
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        //Get target
        currentObjective = assigedPos = ClientManager.Instance.assignTarget(); //Chair to go
        // agent.SetDestination(assigedPos);
        //Assign Random priority to prevent two agent blocking each other
        agent.avoidancePriority=Random.Range(0, 99);

        status = "entering";
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //Update status if it was requested
        if(_lastStatusRequest !=null)
        {
            _prevStatus=_status;
            _status = _lastStatusRequest;

            animator.SetTrigger(_status); //Update status in animator
            updateStatus(_status);

            _lastStatusRequest = null;

            // Debug.Log(gameObject.name+" "+_status);
        }

        //Navigation
        // Debug.Log(gameObject.name + " navigation : "+ agent.isStopped + " " + agent.remainingDistance);
        Debug.DrawLine(gameObject.transform.position, agent.destination, Color.blue, 0.0f);

        
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
                currentObjective = assigedPos = ClientManager.Instance.assignTarget(assigedPos, true); //Assign leaving target and set it as current objective
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
                currentObjective = assigedPos = ClientManager.Instance.assignTarget(assigedPos, true); //Assign leaving target and set it as current objective 
            }
        }

        else if(status=="leaving" && !agent.pathPending && agent.remainingDistance<0.5) //Reached exit ?
        {
            Destroy(gameObject);
        }
            
        else if(status=="event" && !agent.pathPending && agent.remainingDistance==0) //Reached event ?
        {
            assignToEvent(); //In case events already finished, come back to normal
        }
    }

    protected void OnDestroy()
    {
        ClientManager.Instance.clientLeave(gameObject);
    }
}
