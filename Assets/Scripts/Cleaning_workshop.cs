using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Define the behavior of a cleaning workshop
public class Cleaning_workshop : Workshop
{
    // List<GameObject> stock = new List<GameObject>(); //List of mug in workshop
    
    //Handle objects interactions w/ Workshop
    //Return wether the object is taken from tavernkeeper
    public override bool use(GameObject object_used)
    {
        if(object_used != null && object_used.tag=="Grabable")
        {
            Mug mug = object_used.GetComponent<Mug>();
            if (mug!= null && mug.content is null && mug.dirty) //Mug dirty & empty
            {
                Debug.Log(object_used.name+ "cleaned by"+gameObject.name);
                mug.dirty=false;
                return false;
            }
        }
        return false;
    }
}
