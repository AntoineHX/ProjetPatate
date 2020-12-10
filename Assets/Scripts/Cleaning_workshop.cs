﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Define the behavior of a cleaning workshop
public class Cleaning_workshop : Workshop
{
    List<GameObject> stock = new List<GameObject>(); //List of mug in workshop
    
    //Handle objects interactions w/ Workshop
    //Return wether the object is taken from tavernkeeper
    public override bool use(GameObject userObject)
    {
        if(userObject != null)
        {
            //TODO : Gérer grabable autre que Mug
            if(userObject.tag=="Grabable")
            {
                //Stock and empty mug
                Mug mug = userObject.GetComponent<Mug>();
                if (mug!= null)
                {
                    Debug.Log(userObject.name+ " stocked in "+gameObject.name);
                    if (mug.content != null)//Empty mug
                        mug.consume();
                    stock.Add(userObject);
                    return true; //Object taken
                }
            }
            else if(userObject.tag=="Player" && currentMug!=null) //Give clean mug
            {
                Tavernkeeper_controller player = userObject.GetComponent<Tavernkeeper_controller>();
                Mug mug = currentMug.GetComponent<Mug>();
                if(player!=null && mug !=null)
                {
                    Debug.Log(gameObject.name+" give "+currentMug.name+" to "+userObject.name);
                    //Clean mug
                    mug.dirty=false;
                    //Give mug
                    player.grab(currentMug);
                    currentMug=null;
                }
            }
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        //Set current mug if there's stock
        if(currentMug is null && stock.Count>0)
        {
            currentMug=stock[0];
            stock.RemoveAt(0);
        }
    }
}
