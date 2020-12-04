using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGrabable
{
    //Unity inspector doesn't handle interface...
    // int size {get; set;} //Size (1 or 2 hands) of the object
    void use();
    void take();
    void drop(Vector2 position); //Drop to position
}
