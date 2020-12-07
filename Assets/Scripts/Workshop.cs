using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Define the behavior of a workshop (producer of Consumable)
[RequireComponent(typeof(Collider2D))]
public class Workshop : MonoBehaviour
{
    public string product_name;
    public int product_value;
    public Sprite product_sprite;
    public float prepTime = 2.0f; //Time for preparation of product
    public int stock = 5; //Stock of product
    // GameObject currentMug = null; //Mug currently stocked in workshop

    //Handle objects interactions w/ Workshop
    //Return wether the object is taken from tavernkeeper
    public bool use(GameObject object_used)
    {
        if(object_used != null && object_used.tag=="Mug")
        {
            Mug mug = object_used.GetComponent<Mug>();
            if (mug!= null && mug.content is null && !mug.dirty && stock>0) //Mug clean & empty + remaining stock in workshop
            {
                Debug.Log(gameObject.name+" fill "+object_used.name+ " with "+product_name);
                mug.fill(new Consumable(product_name,product_value,product_sprite));
                stock--;
                return false;
            }
            else
            {
                Debug.Log(object_used.name+" cannot be filled with "+product_name+ " -stock:"+stock);
                return false;
            }
        }
        else
        {
            Debug.Log(gameObject.name+" doesn't handle : "+object_used);
            return false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if(gameObject.layer != LayerMask.NameToLayer("Interactions"))
            Debug.LogWarning(gameObject.name+" layer should be set to 'Interactions' to work properly");
        if(gameObject.tag != "Workshop")
            Debug.LogWarning(gameObject.name+" tag should be set to 'Workshop' to work properly");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
