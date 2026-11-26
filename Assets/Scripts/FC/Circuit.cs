using UnityEngine;

public class Circuit : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private bool hasItem = false;
    

    public FCChief.HeldItem itemType = FCChief.HeldItem.None;

    public void PlaceItem(FCChief.HeldItem item)
    {
        hasItem = true;
        itemType = item;
    }

    public bool Occupied()
    {
        return hasItem;
    }

    public FCChief.HeldItem TakeItem()
    {
        hasItem = false;
        var temp = itemType;
        itemType = FCChief.HeldItem.None;
        return temp;
    }
}
