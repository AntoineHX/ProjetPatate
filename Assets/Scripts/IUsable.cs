using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Represent object that can be used by tavernkeeper
public interface IUsable
{
    //Return wether the object is taken from tavernkeeper
    bool use(GameObject userObject);
}
