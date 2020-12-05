﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; //Exceptions

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class Tavernkeeper_controller : MonoBehaviour
{
    public float mvt_speed = 5.0f; //Movement speed
    public float action_dist = 1.5f; //Action distance

    IDictionary<string, GameObject> hand_container; //Objects in hand

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
        hands = Input.GetAxis("Hand");

        //Movement action
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

        //Hands actions
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

    //Handle action with hands ("left" or "right")
    void handAction(string hand)
    {
        //Test
        if(!hand_container.ContainsKey(hand))
        {
            throw new Exception("Invalid key for hands :"+hand);
        }
        
        // Test collision of ray from tavernkeeper center (A verifier) at action_dist unit distance on Interactions layer
        RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, action_dist, LayerMask.GetMask("Interactions"));
        if (hit.collider != null)
        {
            GameObject hit_object = hit.collider.gameObject;
            // Debug.Log("Raycast has hit the object " + hit_object.name+ hit_object.tag);
            if (hit_object != null)
            {
                //Handle objects interactions by tags
                //TODO : Factoriser actions des IGrabable et actions des Clients/Workshop
                if(hit_object.tag == "Mug")
                {
                    if(hand_container[hand] is null) //Empty hand : try grab mug
                    {
                        // hit_object.transform.SetParent(transform);
                        // hit_object.transform.localPosition = new Vector2(-0.2f,0.2f);

                        Mug obj = hit_object.GetComponent<Mug>();
                        if(obj!=null && obj.size == 1) //Only take obj of size 1 hand
                        {
                            obj.take();
                            hand_container[hand]=hit_object;
                        }
                    }
                }
                else if(hit_object.tag == "Client")
                {
                    // Debug.Log("Give "+ hand_container[hand].name+" to "+hit_object.name);
                    Client_controller client = hit_object.GetComponent<Client_controller>();
                    if(client!=null)
                    {
                        if(client.use(hand_container[hand])) //Interactions w/ object in hands
                        {
                            //Object taken by client
                            hand_container[hand]=null;
                        }
                    }
                }
                else if(hit_object.tag == "Workshop")
                {
                    // Debug.Log("Give "+ hand_container[hand].name+" to "+hit_object.name);
                    Workshop workshop = hit_object.GetComponent<Workshop>();
                    if(workshop!=null)
                    {
                        if(workshop.use(hand_container[hand])) //Interactions w/ object in hands
                        {
                            //Object taken by workshop
                            hand_container[hand]=null;
                        }
                    }
                }
                else
                {
                    Debug.Log(hit_object.tag+" tag not handled by "+gameObject.name);
                }
            }  
        }
        else if (hand_container[hand] != null) //Hand full and no hits
        {
            if (hand_container[hand].tag == "Mug") //Drop mug on player position
            {
                Mug obj = hand_container[hand].GetComponent<Mug>();
                if(obj !=null)
                {
                    obj.drop(rigidbody2d.position);
                    hand_container[hand]=null;
                }
            }
            else
            {
                Debug.Log(gameObject+" doesn't handle Hand full with "+ hand_container[hand]);
            }
        }
    }

    //Returns set of free hands (keys)
    // ISet<string> freeHands()
    // {
    //     HashSet<string> res = new HashSet<string>();
    //     foreach ( KeyValuePair<string, GameObject> kvp in hand_container)
    //     {
    //         if(kvp.Value is null)
    //         {
    //             res.Add(kvp.Key);
    //         }
    //     }
    //     return res;
    // }
}
