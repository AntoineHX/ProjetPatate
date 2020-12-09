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
    GameObject currentMug = null; //Mug currently stocked in workshop

    //Handle objects interactions w/ Workshop
    //Return wether the object is taken from tavernkeeper
    public bool use(GameObject userObject)
    {
        if(userObject != null)
        {
            if(userObject.tag=="Mug")
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
