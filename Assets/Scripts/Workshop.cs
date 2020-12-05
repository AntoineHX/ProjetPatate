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
    protected int _stock = 5; //Stock of prodcut

    //Handle objects interactions w/ Workshop
    //Return wether the object is taken from tavernkeeper
    public bool use(GameObject object_used)
    {
        if(object_used != null && object_used.tag=="Mug")
        {
            Mug mug = object_used.GetComponent<Mug>();
            if (mug!= null && mug.content is null && _stock>0)
            {
                Debug.Log(gameObject.name+" fill "+object_used.name+ " with "+product_name);
                mug.fill(new Consumable(product_name,product_value,product_sprite));
                _stock--;
                return false;
            }
            else
            {
                Debug.Log(object_used.name+" cannot be filled with "+product_name+ " -stock:"+_stock);
                return false;
            }
        }
        else
        {
            Debug.Log("Display order (or something else) of "+gameObject.name);
            return false;
        }
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
