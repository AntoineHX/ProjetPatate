﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Define the behavior of a mug (movable container of Consumable)
[RequireComponent(typeof(Collider2D))]
public class Mug : MonoBehaviour, IGrabable
{
    public int size = 1; //Size (1 or 2 hands) of the object
    public bool dirty = false;
    public Consumable content= null; //new Consumable("beer",1,null);

    public void use()
    {
        //Do nothing
    }
    public void take() //Object taken
    {
        gameObject.SetActive(false);
    }
    public void drop(Vector2 position) //Drop to position
    {
        gameObject.SetActive(true);
        gameObject.transform.position = position;
    }

    public void fill(Consumable new_content) //Fill Mug w/ new Consumable
    {
        if(content is null)
        {
            content = new_content;
        }
        else
        {
            Debug.Log(gameObject.name+" cannot be filled (already full) with "+new_content.Type);
        }
    }
    public void consume() //Empty Mug of its Consumable
    {
        content=null;
        dirty = true; //Used and dirty
    }

    // Start is called before the first frame update
    void Start()
    {
        //Needs to be on Interactions layer and have the Mug tag to work properly
        gameObject.tag = "Mug"; //Force gameobject tag
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
