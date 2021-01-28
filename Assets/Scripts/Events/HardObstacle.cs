using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Define the behavior of a hard obstacle (Block movement)
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(NavMeshObstacle))]
public class HardObstacle : MonoBehaviour, IUsable
{
    [SerializeField]
    float lifeTime= -1.0f, waitTime= 30.0f; //Time active/waiting before self-destruct (Negative value to prevent self-destruct)
    float lifeTimer;

    [SerializeField]
    UITimer UIStopTimer = null; //Script of the UI display
    [SerializeField]
    float stopTime=1.0f; //Time to stop fight by tavernkeeper
    float stopTimer=0.0f;
    bool playerInteracting = false; //Wether the player is interacting w/ the workshop
    float interactionCd = 0.0f; //Time to consider the interaction stopped
    [SerializeField]
    float interactionSmoothing = 0.0f; //% of action_cd added to the interaction CD for smooth continued interaction
    protected float cdTimer = 0.0f;

    SpriteRenderer user_renderer = null; //Sprite renderer of the user (to turn insvisible)
    

    List<Client_controller> angryClients = new List<Client_controller>(); //Clients in the fight

    Collider2D ObsCollider;
    NavMeshObstacle Obstacle;
    // SpriteRenderer ObsRenderer;
    Animator animator;

    bool gameRunning = true; //Wehter the game is running (for clean-up purpose)

    //Handle objects interactions w/ obstacle
    //Return wether the object is taken from tavernkeeper
    public bool use(GameObject userObject)
    {
        //Handle continuous interaction w/ tavernkeeper
        if(userObject.tag=="Player")
        {
            if(stopTimer<stopTime) //Continue to stop fight
            {
                Debug.Log(gameObject.name+" still used by "+userObject.name);
                if(interactionCd<0.01f) //No interaction CD ?
                {
                    user_renderer = userObject.GetComponent<SpriteRenderer>(); //Renderer for visual effects

                    //Reset interaction CD
                    Tavernkeeper_controller player = userObject.GetComponent<Tavernkeeper_controller>();
                    if(player != null)
                        interactionCd=player.action_cd*(1.0f+interactionSmoothing); //=action_cd+ interactionSmoothing(%) action_cd (for smooth continued interaction)
                    else
                        Debug.LogWarning(userObject.name+" cannot have a continuous interaction on "+gameObject.name);
                }
                playerInteracting = true; //Set interaction indicator
                cdTimer = interactionCd; //Reset Interaction CD

                //Visual effect (user disappear)
                if(user_renderer != null)
                        user_renderer.color=Color.clear;
            }
            else //Fight stopped
                Destroy(gameObject);
        }
        return false; //No object taken
    }
        
    // Start is called before the first frame update
    void Start()
    {
        if(gameObject.layer != LayerMask.NameToLayer("Interactions"))
            Debug.LogWarning(gameObject.name+" layer should be set to 'Interactions' to work properly");
        if(gameObject.tag != "Usable")
            Debug.LogWarning(gameObject.name+" tag should be set to 'Usable' to work properly");
        if(UIStopTimer is null)
            Debug.LogWarning(gameObject.name+" doesn't have a UIStopTimer set");


        lifeTimer=waitTime; //Start by waiting client
        
        ObsCollider = GetComponent<Collider2D>();

        Obstacle = GetComponent<NavMeshObstacle>();
        Obstacle.enabled=false; //Don't block movement before client entounter

        animator = GetComponent<Animator>();
        animator.SetBool("active fight", false); //Disable animation before client encounter
    }

    // Update is called once per frame
    void Update()
    {
        //Life time update
        lifeTimer -= Time.deltaTime;
        if(lifeTimer<0)
            Destroy(gameObject);

        //Player interactions update
        if(user_renderer != null && !playerInteracting)
            user_renderer.color=Color.white; //User reappear if there's one
        if(playerInteracting)
        {
            //Continue stopping fight
            if(stopTimer<stopTime)
                stopTimer+=Time.deltaTime;

            //Update interaction CD
            cdTimer-=Time.deltaTime;
            if (cdTimer<0)
                playerInteracting=false; //Reset interaction indicator 
        }
        else if(stopTimer>0) //Decrease fight stopping jauge if not interacting
            stopTimer-=Time.deltaTime*0.5f;
        
        //UI update
        if(UIStopTimer != null)
        {
            UIStopTimer.SetValue(stopTimer/stopTime);
            UIStopTimer.gameObject.SetActive(stopTimer>0); //Set active if timer>0
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Client_controller newClient = other.GetComponent<Client_controller>();
        if(newClient!=null && !angryClients.Contains(newClient))
        {
            angryClients.RemoveAll(item => item == null); //In case clients have been destroyed before event start, remove them

            if(newClient.status!="event") //Make sure to set right status 
                newClient.assignToEvent(gameObject.transform.position);
            angryClients.Add(newClient);
            
            // Debug.Log("New fighting client. Current nb : "+angryClients.Count);
            if(angryClients.Count>1)//Start fight
            {
                foreach(Client_controller client in angryClients) //Turn off client (to merge for a fight)
                        client.gameObject.SetActive(false);

                animator.SetBool("active fight", true);
    
                //Block movement
                Obstacle.enabled=true; 
                ObsCollider.isTrigger=false; //Trigger becoming solid

                lifeTimer=lifeTime; //Time before end of the fight
            }
        }
    }

    void OnDestroy()
    {
        if(gameRunning) //Only apply if game is still running
        {
            foreach(Client_controller client in angryClients) //Clients return to their previous behavior
            {
                client.gameObject.SetActive(true);
                client.assignToEvent(); //Restore previous behavior
            }

            if(user_renderer != null) //User reappear if there's one
                    user_renderer.color=Color.white;
        }
            
        EventManager.Instance.removeEvent(gameObject);
    }

    void OnApplicationQuit()
    {
        gameRunning=false;
    }
}
