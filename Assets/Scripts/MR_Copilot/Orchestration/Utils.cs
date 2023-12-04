using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;

public class Utils
{

    private static Bounds GetRenderBounds(GameObject obj)
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
                return new Bounds(obj.transform.position, new Vector3(0,0,0));
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

    public static void PlaceNextTo(GameObject targetObject, GameObject objectToPlace, Vector3 offset)
    {
        //NOTE: this function assumes the objects are axis-aligned

        //// Get the renderers of both objects
        //Renderer objectRenderer = objectToPlace.GetComponent<Renderer>();
        //Renderer targetRenderer = targetObject.GetComponent<Renderer>();

        //// Check if both renderers are valid
        //if (objectRenderer == null || targetRenderer == null)
        //{
        //    Debug.LogError("One or both objects do not have a renderer component");
        //    return;
        //}

        //// Get the bounding boxes of both renderers
        //Bounds objectBounds = objectRenderer.bounds;
        //Bounds targetBounds = targetRenderer.bounds;

        Bounds objectBounds = GetRenderBounds(objectToPlace);
        Bounds targetBounds = GetRenderBounds(targetObject);

        float distance = offset.magnitude;
        Vector3 direction = offset.normalized;


        Vector3 exit_point_target = ComputeExitPointEfficient(targetBounds, direction);
        Vector3 exit_point_object = ComputeExitPointEfficient(objectBounds, -direction);

        // see if the minimal separation is enough
        float dist_target = Vector3.Distance(targetBounds.center, exit_point_target);
        float dist_object = Vector3.Distance(objectBounds.center, exit_point_object);

        // Calculate the minimum distance needed to avoid overlapping the bounding boxes
        float minDistance = dist_target + dist_object;

        // Adjust the distance if it is smaller than the minimum distance
        if (distance < minDistance)
        {
            distance = minDistance;
        }

        // Calculate the new position for the object to place
        Vector3 newPosition = targetObject.transform.position + direction * distance;

        // Set the position of the object to place
        objectToPlace.transform.position = newPosition;
    }

    public static Vector3 ComputeExitPointEfficient(Bounds box, Vector3 dir)
    {
        float[] factors = new float[] { box.extents.x / dir.x, box.extents.y / dir.y, box.extents.z / dir.z };
        //for (int i=0; i< factors.Length; i++)
        //{
        //    print(factors[i]);
        //}
        float[] factors_abs = factors.Select(a => Mathf.Abs(a)).ToArray();

        var (min_factor_abs, min_index) = factors_abs.Select((n, i) => (n, i)).Min();
        float min_factor = factors[min_index];

        // determining the plane of exit. Six cases
        Vector3[] normals = new Vector3[]
        {
        Vector3.right, // Positive x face
        Vector3.up, // Positive y face
        Vector3.forward, // Positive z face
        Vector3.left, // Negative x face
        Vector3.down, // Negative y face
        Vector3.back // Negative z face
        };

        float[] distances = new float[]
        {
        box.extents.x, // Positive x face
        box.extents.y, // Positive y face
        box.extents.z, // Positive z face
        box.extents.x, // Negative x face
        box.extents.y, // Negative y face
        box.extents.z // Negative z face
        };

        int plane_idx = min_index;
        if (min_factor < 0) { plane_idx += 3; } // should be the negative of that plane.

        //print("dir: " + dir);
        //print("plane idx: " + plane_idx);


        //Vector3? exit_point = GetPlaneIntersection(box.center, dir, normals[plane_idx], distances[plane_idx]);
        //return exit_point.Value;

        Vector3 exit_point = box.center + min_factor_abs * dir;
        return exit_point;
    }

    public static Vector2 get_bb_center(Vector2 corner, float w, float h, float W, float H, string mode)
    {
        float x_hat = 0f;
        float y_hat = 0f;

        if (mode == "denseCaptioning")
        {
            x_hat = (corner.x + 0.5f * w) / W;
            y_hat = (corner.y - 0.5f * h) / H;
        }
        else if (mode == "objects")
        {
            x_hat = (corner.x + 0.5f * w) / W;
            y_hat = (corner.y + 0.5f * h) / H;
        }


        return new Vector2(x_hat, y_hat);
    }

    public static Vector2 image_to_screen_space(Vector2 p_img)
    {
        float x_s = Screen.width * p_img.x;
        float y_s = Screen.height * (1f - p_img.y); //y is flipped between image coordinates and screen coordinates

        return new Vector2(x_s, y_s);
    }

    public static Vector3 image_to_world_space(Vector2 p_img, float z_s)
    {
        Vector2 p_s = image_to_screen_space(p_img);
        Vector3 p_w = Camera.main.ScreenToWorldPoint(new Vector3(p_s.x, p_s.y, z_s));

        return p_w;
    }

    public static void DeleteScriptOnGameObject(GameObject target, string scriptName)
    {
        // Try to find a component of the given script name on the target gameObject using reflection
        System.Type scriptType = System.Type.GetType(scriptName + ",Assembly-CSharp");
        if (scriptType != null)
        {
            Component script = target.GetComponent(scriptType);
            // If the component exists, destroy it
            if (script != null)
            {
                UnityEngine.Object.Destroy(script);
            }
            else
            {
                Debug.Log("No script of name " + scriptName + " found on " + target.name);
            }
        }
        else
        {
            Debug.Log("Invalid script name " + scriptName);
        }
    }

    public static void DeleteComponentByName(GameObject target, string componentName)
    {
        // Try to find the component on the target gameobject using reflection
        System.Type componentType = System.Type.GetType(componentName);

        Component component = target.GetComponent(componentType);

        // If the component instance is not null, destroy it
        if (component != null)
        {
            UnityEngine.Object.Destroy(component);
        }
        else
        {
            Debug.LogWarning("No component of type " + componentName + " found on " + target.name);
        }
    }

    // deletes scripts on the compiler
    public static void DeleteScript(string scriptName)
    {
        GameObject compiler = GameObject.Find("Compiler");
        //DeleteScriptOnGameObject(compiler, scriptName);
        DeleteComponentByName(compiler, scriptName);
    }

    public static void AddPhysics(GameObject obj, float mass)
    {
        // set collider
        foreach (Transform child in obj.transform.GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.GetComponent<MeshCollider>() == null)
            {
                MeshCollider col = child.gameObject.AddComponent<MeshCollider>();
                // non-cvx collider is incompatible with rigidbody unless it is kinematic
                col.convex = true;
            }
        }

        // set rigidbody
        if (obj.GetComponent<Rigidbody>() == null)
        {
           obj.AddComponent<Rigidbody>();
        }
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        rb.mass = mass;
    }

    // Source: https://github.com/yasirkula/UnityAdjustPivot/blob/master/Plugins/AdjustPivot/Editor/AdjustPivot.cs
    public static void SetParentPivot(Transform pivot)
    {
        Transform pivotParent = pivot.parent;
        //if (IsPrefab(pivotParent))
        //{
        //    Debug.LogWarning("Modifying prefabs directly is not allowed, create an instance in the scene instead!");
        //    return;
        //}

        if (pivot.localPosition == Vector3.zero && pivot.localEulerAngles == Vector3.zero)
        {
            Debug.LogWarning("Pivot hasn't changed!");
            return;
        }

        //// deals with the case where the pivot doesn't have 0 rotation
        //if (pivot.localEulerAngles != Vector3.zero)
        //{
        //    Vector3 parentScale = pivotParent.localScale;
        //    if (!Mathf.Approximately(parentScale.x, parentScale.y) || !Mathf.Approximately(parentScale.x, parentScale.z))
        //    {
        //        // This is an edge case (object has non-uniform scale and pivot is rotated). We must create an empty parent GameObject in this scenario
        //        GameObject emptyParentObject = new GameObject(GENERATED_EMPTY_PARENT_NAME);
        //        if (!IsNull(pivotParent.parent))
        //            emptyParentObject.transform.SetParent(pivotParent.parent, false);
        //        else
        //            SceneManager.MoveGameObjectToScene(emptyParentObject, pivotParent.gameObject.scene);

        //        emptyParentObject.transform.localPosition = pivotParent.localPosition;
        //        emptyParentObject.transform.localRotation = pivotParent.localRotation;
        //        emptyParentObject.transform.localScale = pivotParent.localScale;

        //        Undo.RegisterCreatedObjectUndo(emptyParentObject, UNDO_ADJUST_PIVOT);
        //        Undo.SetTransformParent(pivotParent, emptyParentObject.transform, UNDO_ADJUST_PIVOT);

        //        // Automatically expand the newly created empty parent GameObject in Hierarchy
        //        EditorGUIUtility.PingObject(pivot.gameObject);
        //    }
        //}

        MeshFilter meshFilter = pivotParent.GetComponent<MeshFilter>();
        Mesh originalMesh = null;
        if (!Utils.IsNull(meshFilter) && !Utils.IsNull(meshFilter.sharedMesh))
        {
            //Undo.RecordObject(meshFilter, UNDO_ADJUST_PIVOT);

            originalMesh = meshFilter.sharedMesh;
            //Mesh mesh = Instantiate(meshFilter.sharedMesh);
            Mesh mesh = Mesh.Instantiate(meshFilter.sharedMesh);
            meshFilter.sharedMesh = mesh;

            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            Vector4[] tangents = mesh.tangents;

            Debug.Log(vertices.Length);
            Debug.Log(tangents.Length);

            if (pivot.localPosition != Vector3.zero)
            {
                Vector3 deltaPosition = -pivot.localPosition;
                for (int i = 0; i < vertices.Length; i++)
                    vertices[i] += deltaPosition;
            }

            if (pivot.localEulerAngles != Vector3.zero) // this is a strange-ish case. Usually your pivot should not be rotated.
            {
                Quaternion deltaRotation = Quaternion.Inverse(pivot.localRotation);
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = deltaRotation * vertices[i];
                    normals[i] = deltaRotation * normals[i];

                    if (tangents.Length > i) //sometimes this happens, not sure why, may need to figure it out someday.
                    {
                        Vector3 tangentDir = deltaRotation * tangents[i];
                        tangents[i] = new Vector4(tangentDir.x, tangentDir.y, tangentDir.z, tangents[i].w);
                    }
                }
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.tangents = tangents;

            mesh.RecalculateBounds();
        }

        // gets custom options from editor window
        //GetPrefs();

        Collider[] colliders = pivotParent.GetComponents<Collider>();
        foreach (Collider collider in colliders)
        {
            MeshCollider meshCollider = collider as MeshCollider;
            if (!Utils.IsNull(meshCollider) && !Utils.IsNull(originalMesh) && meshCollider.sharedMesh == originalMesh)
            {
                //Undo.RecordObject(meshCollider, UNDO_ADJUST_PIVOT);
                meshCollider.sharedMesh = meshFilter.sharedMesh;
            }
        }

        //if (createColliderObjectOnPivotChange && IsNull(pivotParent.Find(GENERATED_COLLIDER_NAME)))
        //{
        //    GameObject colliderObj = null;
        //    foreach (Collider collider in colliders)
        //    {
        //        if (IsNull(collider))
        //            continue;

        //        MeshCollider meshCollider = collider as MeshCollider;
        //        if (IsNull(meshCollider) || meshCollider.sharedMesh != meshFilter.sharedMesh)
        //        {
        //            if (colliderObj == null)
        //            {
        //                colliderObj = new GameObject(GENERATED_COLLIDER_NAME);
        //                colliderObj.transform.SetParent(pivotParent, false);
        //            }

        //            EditorUtility.CopySerialized(collider, colliderObj.AddComponent(collider.GetType()));
        //        }
        //    }

        //    if (colliderObj != null)
        //        Undo.RegisterCreatedObjectUndo(colliderObj, UNDO_ADJUST_PIVOT);
        //}

        //if (createNavMeshObstacleObjectOnPivotChange && IsNull(pivotParent.Find(GENERATED_NAVMESH_OBSTACLE_NAME)))
        //{
        //    NavMeshObstacle obstacle = pivotParent.GetComponent<NavMeshObstacle>();
        //    if (!IsNull(obstacle))
        //    {
        //        GameObject obstacleObj = new GameObject(GENERATED_NAVMESH_OBSTACLE_NAME);
        //        obstacleObj.transform.SetParent(pivotParent, false);
        //        EditorUtility.CopySerialized(obstacle, obstacleObj.AddComponent(obstacle.GetType()));
        //        Undo.RegisterCreatedObjectUndo(obstacleObj, UNDO_ADJUST_PIVOT);
        //    }
        //}

        Transform[] children = new Transform[pivotParent.childCount];
        Vector3[] childrenPositions = new Vector3[children.Length];
        Quaternion[] childrenRotations = new Quaternion[children.Length];
        for (int i = children.Length - 1; i >= 0; i--)
        {
            children[i] = pivotParent.GetChild(i);
            childrenPositions[i] = children[i].position;
            childrenRotations[i] = children[i].rotation;

            //Undo.RecordObject(children[i], UNDO_ADJUST_PIVOT);
        }

        //Undo.RecordObject(pivotParent, UNDO_ADJUST_PIVOT);
        pivotParent.position = pivot.position;
        pivotParent.rotation = pivot.rotation;

        for (int i = 0; i < children.Length; i++)
        {
            children[i].position = childrenPositions[i];
            children[i].rotation = childrenRotations[i];
        }

        pivot.localPosition = Vector3.zero;
        pivot.localRotation = Quaternion.identity;
    }

    private static bool IsNull(UnityEngine.Object obj)
    {
        return obj == null || obj.Equals(null);
    }

    public static void DisplayMessage(string msg)
    {
        string output_console_name = "CopilotOutput";
        GameObject output_console = GameObject.Find(output_console_name);
        output_console.GetComponent<TMP_InputField>().text = msg;
    }

}
