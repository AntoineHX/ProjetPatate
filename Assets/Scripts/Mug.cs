using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Mug : MonoBehaviour, IGrabable
{
    public int size = 1; //Size (1 or 2 hands) of the object
    bool dirty = false;
    int content = 1;

    public void use()
    {
        //Do nothing
    }
    public void take() //Object taken
    {
        gameObject.SetActive(false);
    }
    public void drop(Vector2 position) //Drop to position
    {
        gameObject.SetActive(true);
        gameObject.transform.position = position;
    }

    public void fill(int new_content)
    {
        content = new_content;
    }
    public void consume()
    {
        content=0;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Needs to be on Interactions layer and have the Mug tag to work properly
        gameObject.tag = "Mug"; //Force gameobject tag
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
