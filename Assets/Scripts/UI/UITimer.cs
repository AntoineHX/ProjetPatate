using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITimer : MonoBehaviour
{
    // public Image mask;
    public Image time;
    // float originalSize;

    void Start()
    {
        // originalSize = mask.rectTransform.rect.height;
        time.color = Color.green;
    }

    //Value : [0,1]
    public void SetValue(float value)
    {	
        if(value>1||value<0)
            Debug.LogWarning(gameObject.name+" - Timer value out of range [0,1]: "+value);
        //Change time color		      
        if(value>0.66)
            time.color = Color.green;
        else if(value>0.33)
            time.color = Color.yellow;
        else
            time.color = Color.red;

        time.fillAmount = value;

        //Change mask size
        // mask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSize * value);
    }
}
