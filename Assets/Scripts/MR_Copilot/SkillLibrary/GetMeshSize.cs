using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetMeshSize : Skills
{
    // Start is called before the first frame update
    void Awake()
    {
        string className = this.GetType().Name;
        textToBuilder = "To get the meshsize of the object, call " + className + ".GetRenderBounds(GameObject obj), which returns the total mesh bound of the parent and child gameobject.";
        textToArchitect = "If the user requests the size of the object, this is file has the function to get the renderer bounds.";
    }

    // Actual APIs start here ---------------------------------------------------

    //public static Bounds GetRenderBounds(GameObject obj)
    ////get the renderer of parent and child gameobject because the mesh of the parent can be a child gameobject
    //{
    //    var r = obj.GetComponent<Renderer>();
    //    var renderers = obj.GetComponentsInChildren<Renderer>();
    //    if (r == null)
    //        r = renderers[0];
    //    var bounds = r.bounds;
    //    foreach (Renderer render in renderers)
    //    {
    //        if (render != r)
    //            bounds.Encapsulate(render.bounds);
    //    }
    //    return bounds;
    //}

    public static Bounds GetRenderBounds(GameObject obj)
    {
        var r = obj.GetComponent<Renderer>();

        var renderers = obj.GetComponentsInChildren<Renderer>();
        if (r == null)
        {
            if (renderers.Length != 0)
            {
                r = renderers[0];
            }
            else
            {
                // if we reached this point, the object has no renders on it or its children
                // so just create a dummy bound with its position as center and zero size
                return new Bounds(obj.transform.position, new Vector3(0, 0, 0));
            }

        }

        var bounds = r.bounds;
        foreach (Renderer render in renderers)
        {
            if (render != r)
                bounds.Encapsulate(render.bounds);
        }
        return bounds;
    }

}
