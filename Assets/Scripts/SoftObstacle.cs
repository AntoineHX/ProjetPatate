using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Define the behavior of a soft obstacle
//TODO : Effect on clients ? (Lower Tips/Repu ?)
[RequireComponent(typeof(Collider2D))]
public class SoftObstacle : MonoBehaviour
{
    [SerializeField]
    float effectChance = 100.0f; //Probabily of fall on entering.
    [SerializeField]
    float mvtSlow = 1.0f; //Scale of movement slowdown (1.0f = no change).

    [SerializeField]
    float lifeTime = 3.0f; //Time to consume currentMug
    float lifeTimer;

    Tavernkeeper_controller player;

    // Start is called before the first frame update
    void Start()
    {
        lifeTimer=lifeTime;
    }

    // Update is called once per frame
    void Update()
    {
        lifeTimer -= Time.deltaTime;
        if(lifeTimer<0)
        {
            Destroy(gameObject);
        }
    }
    //TODO : Trigger falling animation on tavernkeeper
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag=="Player")
        {
            player=other.GetComponent<Tavernkeeper_controller>();
            player.mvt_speed=player.mvt_speed/mvtSlow; //Slow movement speed

            if(Random.Range(0.0f, 99.9f)<effectChance)
            {
                Debug.Log("Woops");
                player.emptyHands();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.tag=="Player")
        {
            player.mvt_speed=player.mvt_speed*mvtSlow; //Restore movement speed
        }
    }

    void OnDestroy()
    {
        EventManager.Instance.destroyEvent(gameObject);
    }
}
