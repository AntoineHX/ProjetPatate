using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Define the behavior of a hard obstacle (Block movement)
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(NavMeshObstacle))]
public class HardObstacle : MonoBehaviour
{
    [SerializeField]
    float lifeTime = -1.0f; //Time before self-destruct (Negative value to prevent self-destruct)
    float lifeTimer;

    List<Client_controller> angryClients = new List<Client_controller>(); //Clients in the fight

    Collider2D ObsCollider;
    NavMeshObstacle Obstacle;
    // SpriteRenderer ObsRenderer;
    Animator animator;

    bool gameRunning = true;

    // Start is called before the first frame update
    void Start()
    {
        lifeTimer=lifeTime;
        
        ObsCollider = GetComponent<Collider2D>();

        Obstacle = GetComponent<NavMeshObstacle>();
        Obstacle.enabled=false; //Don't block movement before client entounter

        animator = GetComponent<Animator>();
        animator.SetBool("active fight", false); //Disable animation before client encounter
    }

    // Update is called once per frame
    void Update()
    {
        if(animator.GetBool("active fight") && lifeTimer>0)
        {
            lifeTimer -= Time.deltaTime;
            if(lifeTimer<0)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Client_controller newClient = other.GetComponent<Client_controller>();
        if(newClient!=null && !angryClients.Contains(newClient))
        {
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
            }
        }
    }

    void OnDestroy()
    {
        if(gameRunning) //Only apply if game is still running
            foreach(Client_controller client in angryClients) //Clients return to their previous behavior
            {
                client.gameObject.SetActive(true);
                client.assignToEvent(); //Restore previous behavior
            }
            
        EventManager.Instance.destroyEvent(gameObject);
    }

    void OnApplicationQuit()
    {
        gameRunning=false;
    }
}
