using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Define the behavior of a workshop
//TODO : Only stock Component instead of GameObject in currentMug ?
[RequireComponent(typeof(Collider2D))]
public abstract class Workshop : MonoBehaviour, IUsable
{
    protected GameObject currentMug = null; //Mug currently stocked in workshop

    [SerializeField]
    protected float prepTime = 2.0f; //Time for preparation of product
    protected float prepTimer= 0.0f;
    [SerializeField]
    protected UITimer UIPrepTimer = null; //Script of the UI display
    protected bool playerInteracting = false; //Wether the player is interacting w/ the workshop
    protected float interactionCd = 0.0f; //Time to consider the interaction stopped
    [SerializeField]
    protected float interactionSmoothing = 0.0f; //% of action_cd added to the interaction CD for smooth continued interaction
    protected float cdTimer = 0.0f;

    //Handle objects interactions w/ Workshop
    //Return wether the object is taken from tavernkeeper
    public abstract bool use(GameObject userObject);

    //Handle continuous interaction w/ Workshop
    protected void continueUse(GameObject userObject)
    {
        Debug.Log(gameObject.name+" still used by "+userObject.name);
        if(interactionCd<0.01f) //No interaction CD => Try to set it up
        {
            Tavernkeeper_controller player = userObject.GetComponent<Tavernkeeper_controller>();
            if(player != null)
                interactionCd=player.action_cd*(1.0f+interactionSmoothing); //=action_cd+ interactionSmoothing(%) action_cd (for smooth continued interaction)
            else
                Debug.LogWarning(userObject.name+" cannot have a continuous interaction on "+gameObject.name);
        }
        playerInteracting = true; //Set interaction indicator
        cdTimer = interactionCd; //Reset Interaction CD
    }

    // Start is called before the first frame update
    protected virtual void Start()
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

    //LateUpdate is called after classic Updates
    protected virtual void LateUpdate()
    {
        if(playerInteracting)
        {
            //Continue Preparation
            if(prepTimer<prepTime) //Update UI Prep timer
            {
                prepTimer+=Time.deltaTime;
                if(UIPrepTimer != null)
                    UIPrepTimer.SetValue(prepTimer/prepTime);
            }

            //Update interaction CD
            cdTimer-=Time.deltaTime;
            if (cdTimer<0)
                playerInteracting=false; //Reset interaction indicator
        }
    }
}
