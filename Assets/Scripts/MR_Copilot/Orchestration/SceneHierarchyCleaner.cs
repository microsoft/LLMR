using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

public class SceneHierarchyCleaner : MonoBehaviour
{

    public GameObject[] roots;
    Dictionary<GameObject, bool> root_dict;
    Dictionary<GameObject, GameObject> child_parent_dict = new Dictionary<GameObject, GameObject>();
    public bool conservative_cleanup;

    void Start()
    {
        // check if URP is enabled
        //bool USING_URP = Type.GetType("UnityEngine.Rendering.DebugManager, Unity.RenderPipelines.Core.Runtime") != null;

        // Disable certain behaviors in URP, which creates a debugging GO that breaks the scene clean up
        //UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
    }

    void ConstructRootDict()
    {
        root_dict = new Dictionary<GameObject, bool>();
        foreach (var root in roots)
        {
            root_dict[root] = true;
        }
    }

    public void CleanUpSceneHierarchy()
    {
        // do CleanUpSceneHierarchy_helper until it has no effect on the scene
        bool continue_cleanup = true;
        while (continue_cleanup)
        {
            continue_cleanup = CleanUpSceneHierarchy_helper();
        }
    }

    public bool CleanUpSceneHierarchy_helper()
    {
        // Note: cannot do this in Start() since we spawn new objects into the scene at runtime
        roots = SceneManager.GetActiveScene().GetRootGameObjects();
        ConstructRootDict();
        // reassign each child to an appropriate parent
        ResetHierarchy();
        // delete useless gameObjects (not a root object and contains only the transform component)
        //bool continue_deletion = true;
        //while (continue_deletion)
        //{   // as long as we deleted something, continue this process. 
        //    // if conservative_cleanup = false, this will only execute twice
        //    continue_deletion = DeleteExtraneousObjects();
        //}
        return DeleteExtraneousObjects();
    }

    public bool DeleteExtraneousObjects()
    {
        bool deleted_sth = false;
        object[] obj = GameObject.FindSceneObjectsOfType(typeof(GameObject));
        foreach (object o in obj)
        {
            GameObject g = (GameObject)o;
            // if in conservative mode, make sure all deleted game objects have no children
            bool conservative_check = true;
            if (conservative_cleanup) 
            {
                conservative_check = g.transform.childCount == 0;
            }
            if (!root_dict.ContainsKey(g) && g.GetComponents<Component>().Length == 1 && conservative_check)
            {
                //print("will delete");
                //Debug.Assert(g.transform.childCount == 0);
                DestroyImmediate(g);
                deleted_sth = true;
            }
        }

        return deleted_sth;
    }

    // Call this method to clean up the hierarchy of the current scene
    public void ResetHierarchy()
    {
        child_parent_dict = new Dictionary<GameObject, GameObject>();
        object[] obj = GameObject.FindSceneObjectsOfType(typeof(GameObject));
        foreach (object o in obj)
        {
            GameObject g = (GameObject)o;
            if (!root_dict.ContainsKey(g))
            {
                var valid_parent = FindValidAncestor(g);
                child_parent_dict[g] = valid_parent;
            }
        }

        ResetAllParents();
    }

    void ResetAllParents()
    {
        foreach (KeyValuePair<GameObject, GameObject> child_parent in child_parent_dict)
        {
            child_parent.Key.transform.SetParent(child_parent.Value.transform, true);
        }
    }

    // Find the nearest valid ancestor of a given game object
    private GameObject FindValidAncestor(GameObject child)
    {
        //print(child.gameObject.name);
        // Get the parent of the child
        var parent = child.transform.parent.gameObject;

        // if in conservative mode, a valid ancestor can also be one that has more than one child object.
        bool conservative_check = false;
        if (conservative_cleanup)
        {
            conservative_check = parent.transform.childCount > 1;
        }
        // Check if the parent has more than one component or if it is a root node
        if (root_dict.ContainsKey(parent) || parent.GetComponents<Component>().Length > 1 || conservative_check)
        {
            // If yes, then the parent is a valid ancestor, so return it
            return parent;
        }
        else
        {
            // If no, then the parent is not a valid ancestor, so recursively find the next valid ancestor
            return FindValidAncestor(parent);
        }
    }

    //private GameObject FindValidAncestor(GameObject child)
    //{
    //    print (child.gameObject.name);
    //    // Get the parent of the child
    //    var parent = child.transform.parent?.gameObject; // Use the null-conditional operator to avoid null dereference

    //    // Check if the parent is null or has more than one component
    //    if (parent == null || parent.GetComponents<Component>().Length > 1)
    //    {
    //        // If yes, then the parent is a valid ancestor, or the child has no ancestor, so return it
    //        return parent ?? child; // Use the null-coalescing operator to return the child if the parent is null
    //    }
    //    else
    //    {
    //        // If no, then the parent is not a valid ancestor, so recursively find the next valid ancestor
    //        return FindValidAncestor(parent);
    //    }
    //}

    //private void LocateAncestor(GameObject obj)
    //{
    //    var ancestor = FindValidAncestor(obj);


    //    // If the ancestor is not the same as the parent, then reparent the child to the ancestor
    //    if (ancestor != obj.transform.parent.gameObject)
    //    {
    //        //obj.transform.SetParent(ancestor.transform, true);
    //        child_parent_dict[obj] = ancestor;
    //    }

    //    foreach (Transform child in obj.transform)
    //    {
    //        LocateAncestor(child.gameObject);
    //    }
    //}
}