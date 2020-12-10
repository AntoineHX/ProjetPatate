using System.Collections;
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
        if(userObject != null)
        {
            // Debug.Log(userObject.tag);
            //TODO : Gérer Grabable qui ne sont pas des mugs ?
            if(userObject.tag=="Grabable")
            {
                Mug mug = userObject.GetComponent<Mug>();
                if (mug!= null && mug.content is null && !mug.dirty && stock>0) //Mug clean & empty + remaining stock in workshop
                {
                    Debug.Log(gameObject.name+" fill "+userObject.name+ " with "+product_name);
                    mug.fill(new Consumable(product_name,product_value,product_sprite));
                    stock--;
                    return false;
                }
                else
                {
                    Debug.Log(userObject.name+" cannot be filled with "+product_name+ " -stock:"+stock);
                    return false;
                }
            }
            else if(userObject.tag=="Player" && currentMug != null) //Give tavernkeeper currentMug
            {
                Tavernkeeper_controller player = userObject.GetComponent<Tavernkeeper_controller>();
                return false;
            }
            else
            {
                return false;
            }
        }
        else
        {
            Debug.Log(gameObject.name+" doesn't handle : "+userObject);
            return false;
        }
    }
}
