﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tavernkeeper_controller : MonoBehaviour
{
    public float mvt_speed = 5.0f; //Movement speed

    Rigidbody2D rigidbody2d;
    // Last user inputs
    float horizontal;
    float vertical;
    float hands;

    Animator animator;
    Vector2 lookDirection = new Vector2(1,0);

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
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
                Debug.Log("Left hand");
            }
            else
            {
                Debug.Log("Right hand");
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
}
