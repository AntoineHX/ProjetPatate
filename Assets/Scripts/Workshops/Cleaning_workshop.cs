using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Define the behavior of a cleaning workshop
//BUG w/ consecutive mugs washed ?
public class Cleaning_workshop : Workshop
{
    List<GameObject> stock = new List<GameObject>(); //List of mug in workshop
    
    //Handle objects interactions w/ Workshop
    //Return wether the object is taken from tavernkeeper
    public override bool use(GameObject userObject)
    {
        if(userObject != null)
        {
            //TODO : Gérer grabable autre que Mug
            if(userObject.tag=="Grabable")
            {
                //Stock mug
                Mug mug = userObject.GetComponent<Mug>();
                if (mug!= null)
                {
                    Debug.Log(userObject.name+ " stocked in "+gameObject.name);
                    mug.take();
                    stock.Add(userObject);

                    return true; //Object taken
                }
            }
            else if(userObject.tag=="Player" && prepTimer<prepTime && currentMug != null) //Prepare currentMug
            {
                continueUse(userObject);
            }
            else if(userObject.tag=="Player" && prepTimer>=prepTime && currentMug != null) //Give tavernkeeper currentMug if cleaned
            {
                Tavernkeeper_controller player = userObject.GetComponent<Tavernkeeper_controller>();
                Mug mug = currentMug.GetComponent<Mug>();
                if(player!=null && mug !=null)
                {
                    // Debug.Log(gameObject.name+" give "+currentMug.name+" to "+userObject.name);
                    //Clean mug
                    mug.dirty=false;
                    //Give mug
                    player.grab(currentMug);
                    currentMug=null;

                    UIPrepTimer.gameObject.SetActive(false); //Turn off UI prep timer
                }
            }
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        // if(currentMug!=null)
        //     Debug.Log(gameObject.name+" stock: "+stock.Count+ " - CurrentMug: "+currentMug.name+" (Dirty: "+currentMug.GetComponent<Mug>().dirty+")");
        //Set current mug if there's stock
        if(currentMug is null && stock.Count>0)
        {
            currentMug=stock[0];
            stock.RemoveAt(0);

            Mug mug = currentMug.GetComponent<Mug>();
            if(mug.content != null)//Empty mug
            {
                mug.consume();
                prepTimer=0.0f;
            }
            else if(!mug.dirty)//Mug already clean
                prepTimer=prepTime;

            if(UIPrepTimer != null) //Display UI prep timer
            {
                UIPrepTimer.SetValue(prepTimer/prepTime);
                // UIPrepTimer.DisplayIcon(false);
                UIPrepTimer.gameObject.SetActive(true);
            }
        }
    }
}
