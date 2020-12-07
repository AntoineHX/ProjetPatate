using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Define the behavior of a cleaning workshop
public class Cleaning_workshop : MonoBehaviour
{
    public float cleanTime=2.0f; //Time to clean a mug
    // List<GameObject> stock = new List<GameObject>(); //List of mug in workshop
    
    //Handle objects interactions w/ Workshop
    //Return wether the object is taken from tavernkeeper
    public bool use(GameObject object_used)
    {
        if(object_used != null && object_used.tag=="Mug")
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
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
