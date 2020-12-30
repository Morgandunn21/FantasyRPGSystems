using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pod : MonoBehaviour
{
    [HideInInspector]
    public List<TargetableObject> objectList;

    //This is for use on the server
    //[HideInInspector]
    public int ID;

    public bool Initialized { get; protected set; }

    protected virtual void Awake()
    {
        objectList = new List<TargetableObject>();
    }

    protected abstract void Start();

    protected void AddObject(TargetableObject obj)
    {
        Debug.Assert(objectList != null, "invalid objectList");

        if (objectList.Contains(obj) == false)
        {
            //Add the object to the overall list of objects
            objectList.Add(obj);
            //Set the ID to its index in the list of objects of its type
            obj.ID = objectList.Count - 1;
        }
        else
        {
            obj.ID = objectList.IndexOf(obj);
        }

        obj.SetPod(this);
    }

    protected void RemoveObject(TargetableObject obj)
    {
        if(objectList.Contains(obj))
        {
            int index = objectList.IndexOf(obj);

            objectList[index] = null;
        }
    }
}
