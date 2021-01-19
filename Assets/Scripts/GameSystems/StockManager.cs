using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Define the system managing the stock of goods. (Singleton)
//TODO : Update check stock of registered workshops
public sealed class StockManager : MonoBehaviour
{
    public static string StockManager_path="/GameSystem/StockManager";
    //Singleton
    private static StockManager _instance=null;
    public static StockManager Instance { get 
        { 
        if(_instance is null) //Force Awakening if needed
            GameObject.Find(StockManager_path).GetComponent<StockManager>().Awake();
        return _instance; 
        } 
    }

    [HideInInspector]
    public bool ready = false; //Wether the StockManager is initialized

    //Consumable
    Dictionary<string, Sprite> consumableSprites = new Dictionary<string, Sprite>(); //Sprite associated w/ types of consumable
    HashSet<string> avail_consumable = new HashSet<string>(); //Available consumable
    Dictionary<string, int> global_stock; //Stocks of all the active production workshops

    List<Component> workshop_register = new List<Component>();

    //Register a production workshop
    public void registerWorkshop(Component workshop)
    {
        if(workshop!=null && !workshop_register.Contains(workshop))
        {
            string prod_name = ((Production_workshop)workshop).product_name;
            Sprite prod_sprite = ((Production_workshop)workshop).product_sprite;
            int stock = ((Production_workshop)workshop).Stock;

            //Check if type is allowed for Consumables
            if(!Consumable.allowed_types.Contains(prod_name))
                Debug.LogError(workshop.gameObject.name+" :  "+prod_name+" isn't an allowed type of consumable.");
            //Check if there's a different sprite registered for the same product
            if(consumableSprites.ContainsKey(prod_name) && consumableSprites[prod_name]!=prod_sprite)
                Debug.LogWarning("StockManager: Different sprites registered for "+prod_name+". Only one will be kept.");

            consumableSprites[prod_name]=prod_sprite;
            updateStock(prod_name, stock);
            workshop_register.Add(workshop);
            // Debug.Log(workshop.gameObject.name+" registered by StockManager");
        }
    }

    //Remove a workshop from the register
    public void removeWorkshop(Component workshop)
    {
        if(workshop!=null && workshop_register.Contains(workshop))
        {
            //Update global stock
            string prod_name = ((Production_workshop)workshop).product_name;
            int stock = ((Production_workshop)workshop).Stock;
            updateStock(prod_name, -stock);

            //Remove workshop
            workshop_register.Remove(workshop);
            // Debug.Log(workshop.gameObject.name+" unregistered by StockManager");
        }
    }

    public Sprite consumableSprite(string consumable)
    {
        if(!consumableSprites.ContainsKey(consumable))
            Debug.LogError("Stock Manager : no sprite registered for : "+consumable);
        return consumableSprites[consumable];
    }

    public void updateStock(string consumable, int valueModif)
    {
        global_stock[consumable]= Mathf.Abs(global_stock[consumable]+valueModif);
        if(global_stock[consumable]==0)
            avail_consumable.Remove(consumable);
        else
            avail_consumable.Add(consumable);
    }

    //Awake is called when the script instance is being loaded.
    void Awake()
    {
        //Singleton
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;

        if(!ready)
        {
            //Initialize global stock
            global_stock= new Dictionary<string, int>();
            foreach(string cons in Consumable.allowed_types)
            {
                global_stock[cons]=0;
            }

            ready = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //// Singleton Implementation (https://jlambert.developpez.com/tutoriels/dotnet/implementation-pattern-singleton-csharp/#LIII) ////
    // private StockManager()
    // {
    // }

    // public static StockManager Instance { get { return Nested.instance; } }
        
    // private class Nested
    // {
    //     // Explicit static constructor to tell C# compiler
    //     // not to mark type as beforefieldinit
    //     static Nested()
    //     {
    //     }

    //     internal static readonly StockManager instance = new GameObject("StockManager").AddComponent<StockManager>();
    // }
    ////
}
