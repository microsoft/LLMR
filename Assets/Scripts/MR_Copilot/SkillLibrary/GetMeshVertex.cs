using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshVertex : Skills
{
    
    void Awake()
    {
        string className = this.GetType().Name;
        textToBuilder = "To get the meshsize of the object, call " + className + ".GetRenderBounds(GameObject obj), which returns the total mesh bound of the parent and child gameobject.";
        textToArchitect = "This allows the user to access the vertex of the 3D object.";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
