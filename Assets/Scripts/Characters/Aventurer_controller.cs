using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Define the behavior of an adventurer
//TODO : Add talking behavior, etc.
public class Aventurer_controller : Client_controller
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start(); //Call Client_controller Start()
    }

    // Update is called once per frame
    //TODO : Request from client manager seat to wait when leaving to talk.
    protected override void Update()
    {
        base.Update(); //Call Client_controller Update
    }
}
