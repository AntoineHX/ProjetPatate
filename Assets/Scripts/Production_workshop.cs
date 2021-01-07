﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Define the behavior of a production workshop
public class Production_workshop : Workshop
{
    // public float prepTime = 2.0f; //Time for preparation of product

    public string product_name;
    public int product_value;
    public Sprite product_sprite;
    public int stock = 5; //Stock of product

    //Handle objects interactions w/ Workshop
    //Return wether the object is taken from tavernkeeper
    public override bool use(GameObject userObject)
    {
        if(userObject.tag=="Player")
        {
            playerInteracting = true; //Set interaction indicator
            Debug.Log("Player interacting");
        }
            

        if(userObject != null)
        {
            // Debug.Log(userObject.tag);
            //TODO : Gérer Grabable qui ne sont pas des mugs ?
            if(userObject.tag=="Grabable" && currentMug is null)
            {
                Mug mug = userObject.GetComponent<Mug>();
                if (mug!= null && mug.content is null && !mug.dirty && stock>0) //Mug clean & empty + remaining stock in workshop
                {
                    Debug.Log(userObject.name+ " stocked in "+gameObject.name);
                    currentMug=userObject;

                    if(UIPrepTimer != null) //Display UI prep timer
                    {
                        prepTimer=0.0f;
                        UIPrepTimer.SetValue(prepTimer/prepTime);
                        UIPrepTimer.DisplayIcon(product_sprite);
                        UIPrepTimer.gameObject.SetActive(true);
                    }

                    return true; //Object taken
                }
                else
                {
                    Debug.Log(userObject.name+" cannot be filled with "+product_name+ " -stock:"+stock);
                }
            }
            else if(prepTimer>=prepTime && userObject.tag=="Player" && currentMug != null) //Give tavernkeeper currentMug if finished preparation
            {
                Tavernkeeper_controller player = userObject.GetComponent<Tavernkeeper_controller>();
                Mug mug = currentMug.GetComponent<Mug>();
                if(player!=null && mug !=null)
                {
                    Debug.Log(gameObject.name+" give "+currentMug.name+" filled with "+product_name+" to "+userObject.name);
                    //Fill mug
                    mug.fill(new Consumable(product_name,product_value,product_sprite));
                    stock--;
                    UIPrepTimer.gameObject.SetActive(false); //Turn off UI prep timer

                    //Give mug
                    player.grab(currentMug);
                    currentMug=null;
                }
            }
        }
        else
            Debug.LogWarning(gameObject.name+" doesn't handle : "+userObject);

        return false; //Object not taken
    }
}
