using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Define the behavior of a cleaning workshop
public class Cleaning_workshop : Workshop
{
    List<GameObject> stock = new List<GameObject>(); //List of mug in workshop
    
    //Handle objects interactions w/ Workshop
    //Return wether the object is taken from tavernkeeper
    public override bool use(GameObject object_used)
    {
        if(object_used != null)
        {
            //TODO : Gérer grabable autre que Mug
            if(object_used.tag=="Grabable")
            {
                //Stock and empty mug
                Mug mug = object_used.GetComponent<Mug>();
                if (mug!= null)
                {
                    Debug.Log(object_used.name+ " stocked in "+gameObject.name);
                    if (mug.content != null)//Empty mug
                        mug.consume();
                    stock.Add(object_used);
                    return true; //Object taken
                }
            }
            else if(object_used.tag=="Player")
            {
                Tavernkeeper_controller player = object_used.GetComponent<Tavernkeeper_controller>();
                if(player!=null && currentMug!=null)
                {
                    Mug mug = currentMug.GetComponent<Mug>();
                    mug.dirty=false;
                    player.grab(currentMug);
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
