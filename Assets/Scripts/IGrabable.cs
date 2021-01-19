using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Represent object that can be grabed.
public interface IGrabable: IUsable
{
    //Unity inspector doesn't handle well interface...
    int size {get; set;} //Size (1 or 2 hands) of the object
    void take(Transform taker_tf=null);
    void drop(Transform tf); //Drop at transform position
}
