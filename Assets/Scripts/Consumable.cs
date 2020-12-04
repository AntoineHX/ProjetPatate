using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


//Represents consumable : sprite and informations
public class Consumable //: MonoBehaviour
{
    private HashSet<string> allowed_types = new HashSet<string>(new [] {"beer", "pression", "vodka"});
    private string _type;
    private int _value;
    private Sprite _sprite;

    //Read-only accessors
    public string Type
    {
        get{ return _type;}
        // set{}
    }
    public int Value
    {
        get{ return _value;}
        // set{}
    }
    public Sprite Sprite
    {
        get{ return _sprite;}
        // set{}
    }

    //TODO : Handle sprite = null 
    public Consumable(string type, int value, Sprite sprite)
    {
        if(!allowed_types.Contains(type))
        {
            throw new Exception("Invalid consumable type :"+type);
        }
        _type=type;
        _value=value;
        _sprite=sprite;
    }
}
