using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tavernkeeper_controller : MonoBehaviour
{
    public float mvt_speed = 5.0f; //Movement speed
    public float action_dist = 1.5f; //Action distance

    IDictionary<string, GameObject> hand_container;

    // Last user inputs
    float horizontal;
    float vertical;
    float hands;

    Vector2 lookDirection = new Vector2(1,0);
    Rigidbody2D rigidbody2d;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        hand_container = new Dictionary<string, GameObject>(){
            {"left", null},
            {"right", null}
        };
    }

    // Update is called once per frame
    void Update()
    {
        //Read inputs
        horizontal = Input.GetAxis("Horizontal"); //See Edit/Project setting / Input Manager
        vertical = Input.GetAxis("Vertical");
        // Debug.Log(horizontal);

        Vector2 move = new Vector2(horizontal, vertical);
        
        //Update animation direction
        if(!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f)) //Movement requested ?
        {
            lookDirection.Set(move.x, move.y); //== lookDirection=move
            lookDirection.Normalize();
        }       
        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", move.magnitude);


        hands = Input.GetAxis("Hand");
        // Debug.Log(hands);
        if(!Mathf.Approximately(hands, 0.0f))
        {
            if(hands>0)
            {
                // Debug.Log("Left hand");
                handAction("left");
            }
            else
            {
                // Debug.Log("Right hand");
                handAction("right");
            }
        }
    }

    // Update used by the Physics engine
    void FixedUpdate()
    {
        //Movement of a physic object
        Vector2 position = rigidbody2d.position;
        position.x = position.x + mvt_speed * horizontal * Time.deltaTime;
        position.y = position.y + mvt_speed * vertical * Time.deltaTime;

        rigidbody2d.MovePosition(position); //Movement processed by the phyisc engine for Collision, etc.
    }

    void handAction(string hand)
    {
        // Test collision of ray from tavernkeeper center (A verifier) at action_dist unit distance on Interactions layer
        RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, action_dist, LayerMask.GetMask("Interactions"));
        if (hit.collider != null)
        {
            GameObject hit_object = hit.collider.gameObject;
            // Debug.Log("Raycast has hit the object " + hit_object+ hit_object.tag);
            if (hit_object != null)
            {
                //Empty hand : try grab grabable object
                if(hand_container[hand] is null)
                {
                    if(hit_object.tag == "Grabable") //by tag, layer-mask or with parent-name ?
                    {
                        // hit_object.transform.SetParent(transform);
                        // hit_object.transform.localPosition = new Vector2(-0.2f,0.2f);
                        hit_object.SetActive(false);
                        hand_container[hand]=hit_object;
                    }
                }
                //Full hand : try give to client
                else if(hit_object.tag == "Client") //by tag or with parent-name ?
                {
                    Debug.Log("Give "+ hand_container[hand]+" to "+hit_object);
                    Destroy(hand_container[hand]);
                }
            }  
        }
        //Full hand : drop
        else if (hand_container[hand] != null)
        {
            // Debug.Log("Hand full with "+ hand_container[hand]);
            // hand_container[hand].transform.SetParent(null);
            hand_container[hand].SetActive(true);
            hand_container[hand].transform.position = rigidbody2d.position;
            hand_container[hand]=null;
        }
    }
}
