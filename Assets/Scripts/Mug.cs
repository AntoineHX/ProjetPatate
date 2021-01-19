using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Define the behavior of a mug (movable container of Consumable)
[RequireComponent(typeof(Collider2D))]
public class Mug : MonoBehaviour, IGrabable
{
    //Redfine attributes of IGrabable to allow display in Unity Inspector
    public int size{get; set;} = 1; //Size (1 or 2 hands) of the object
    public bool dirty = false;
    public Consumable content{get; protected set;} = null; //new Consumable("beer",1,null);

    public UITimer UIContent = null;

    Collider2D triggerCollider;

    //TODO: Gérer objets tavernier (drop) et autres
    public bool use(GameObject userObject)
    {
        if(userObject.tag=="Player")
        {
            // Debug.Log(gameObject.name+" dropped by "+userObject.name);
            drop(userObject.transform);
            return true; //Object taken (on the floor)
        }
        return false; //Return wether the object is taken from tavernkeeper
    }
    public void take(Transform taker_tf=null) //Object taken
    {
        if(taker_tf is null)
            gameObject.SetActive(false);
        else
        {
            gameObject.SetActive(true);
            gameObject.transform.SetParent(taker_tf);
            gameObject.transform.localPosition = new Vector2(0.0f,0.0f);
            triggerCollider.enabled=false;
        }
    }
    public void drop(Transform tf) //Drop to position
    {
        gameObject.SetActive(true);
        gameObject.transform.SetParent(tf.parent);
        gameObject.transform.position = tf.position;
        triggerCollider.enabled=true;
    }

    public void fill(Consumable new_content) //Fill Mug w/ new Consumable
    {
        if(content is null)
        {
            content = new_content;
            if(UIContent!=null)
                UIContent.DisplayIcon(content.Sprite);
                UIContent.gameObject.SetActive(true);
        }
        else
            Debug.Log(gameObject.name+" cannot be filled (already full) with "+new_content.Type);
    }
    public Consumable consume() //Return Mug content and empty it
    {
        Consumable output = content;
        content=null;
        dirty = true; //Used and dirty

        //Turn off UI display
        if(UIContent!=null)
            UIContent.gameObject.SetActive(false);

        return output;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(gameObject.layer != LayerMask.NameToLayer("Interactions"))
            Debug.LogWarning(gameObject.name+" layer should be set to 'Interactions' to work properly");
        if(gameObject.tag != "Grabable")
            Debug.LogWarning(gameObject.name+" tag should be set to 'Grabable' to work properly");
        
        if(UIContent is null)
            Debug.LogWarning(gameObject.name+" doesn't have a UIContent set");
        else
            UIContent.gameObject.SetActive(false);
        
        triggerCollider = gameObject.GetComponent<Collider2D>();
        if(!triggerCollider.isTrigger)
            Debug.LogWarning(gameObject.name+" collider found isn't a trigger");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
