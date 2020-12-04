using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mug : MonoBehaviour, IGrabable
{
    public int size = 1; //Size (1 or 2 hands) of the object

    public void use()
    {

    }
    public void take()
    {
        gameObject.SetActive(false);
    }
    public void drop(Vector2 position) //Drop to position
    {
        gameObject.SetActive(true);
        gameObject.transform.position = position;
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
