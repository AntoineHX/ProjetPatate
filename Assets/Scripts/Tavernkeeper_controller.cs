using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tavernkeeper_controller : MonoBehaviour
{
    public float mvt_speed = 5.0f; //Movement speed


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
        //Empty hand : try grab
        if(hand_container[hand] is null)
        {
            // Test collision of ray from tavernkeeper center at 1.5 unit distance
            RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f);
            if (hit.collider != null)
            {
                // Debug.Log("Raycast has hit the object " + hit.collider.gameObject);
                GameObject hit_object = hit.collider.gameObject;
                if (hit_object != null)
                {
                    // hit_object.transform.SetParent(transform);
                    // hit_object.transform.localPosition = new Vector2(-0.2f,0.2f);
                    hit_object.SetActive(false);
                    hand_container[hand]=hit_object;
                }  
            }
        }
        else //Full hand : drop
        {
            // Debug.Log("Hand full with "+ hand_container[hand]);
            // hand_container[hand].transform.SetParent(null);
            hand_container[hand].SetActive(true);
            hand_container[hand].transform.position = rigidbody2d.position;
            hand_container[hand]=null;
        }
    }
}
