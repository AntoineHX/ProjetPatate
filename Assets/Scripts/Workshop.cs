using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Define the behavior of a workshop
[RequireComponent(typeof(Collider2D))]
public abstract class Workshop : MonoBehaviour, IUsable
{
    protected GameObject currentMug = null; //Mug currently stocked in workshop

    public float prepTime = 2.0f; //Time for preparation of product
    protected float prepTimer= 0.0f;
    public UITimer UIPrepTimer = null; //Script of the UI display
    protected bool playerInteracting = false; //Wether the player is interacting w/ the workshop
    // bool playerInteractFrame = false; //Wether the player used the workshop in the current frame

    //Handle objects interactions w/ Workshop
    //Return wether the object is taken from tavernkeeper
    public abstract bool use(GameObject userObject);

    // Start is called before the first frame update
    void Start()
    {
        if(gameObject.layer != LayerMask.NameToLayer("Interactions"))
            Debug.LogWarning(gameObject.name+" layer should be set to 'Interactions' to work properly");
        if(gameObject.tag != "Usable")
            Debug.LogWarning(gameObject.name+" tag should be set to 'Usable' to work properly");
        if(UIPrepTimer is null)
            Debug.LogWarning(gameObject.name+" doesn't have a UIPrepTimer set");
        else
            UIPrepTimer.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if(playerInteracting)
        {
            if(prepTimer<prepTime) //Update UI Prep timer
            {
                prepTimer+=Time.deltaTime;
                UIPrepTimer.SetValue(prepTimer/prepTime);
            }

            playerInteracting=false; //Reset interaction indicator for next frame
        }
    }
}
