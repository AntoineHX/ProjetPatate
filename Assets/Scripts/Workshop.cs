using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Define the behavior of a workshop
[RequireComponent(typeof(Collider2D))]
public abstract class Workshop : MonoBehaviour, IUsable
{
    public float prepTime = 2.0f; //Time for preparation of product
    protected GameObject currentMug = null; //Mug currently stocked in workshop

    //Handle objects interactions w/ Workshop
    //Return wether the object is taken from tavernkeeper
    public abstract  bool use(GameObject userObject);

    // Start is called before the first frame update
    void Start()
    {
        if(gameObject.layer != LayerMask.NameToLayer("Interactions"))
            Debug.LogWarning(gameObject.name+" layer should be set to 'Interactions' to work properly");
        if(gameObject.tag != "Usable")
            Debug.LogWarning(gameObject.name+" tag should be set to 'Usable' to work properly");
    }
}
