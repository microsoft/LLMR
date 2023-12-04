using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectPlacement2 : Skills
{
    // Start is called before the first frame update
    void Awake()
    {
        string className = this.GetType().Name;
        textToBuilder = "The following methods are available: \n" +
            "public static void PlaceNextTo(GameObject targetObject, GameObject objectToPlace, Vector3 offset): place objectToPlace next to targetObject in the direction of offset. The placement will be resolved such that the object's mesh bounding boxes do not overlap."
            + "#Example \n" + "using UnityEngine;\r\npublic class CreateObjects : Widgets\r\n{\r\n    public GameObject cube;\r\n    public GameObject sphere;\r\n    void Start()\r\n    {\r\n        summary = \"This script creates a cube and places a sphere on top of it\";\r\n        // Create the cube\r\n        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);\r\n        cube.name = \"Cube\";\r\n        // Create the sphere\r\n        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);\r\n        sphere.name = \"Sphere\";\r\n        // Place the sphere on top of the cube\r\n        ObjectPlacement.PlaceNextTo(cube, sphere, new Vector3(0, 1, 0));\r\n    }\r\n}";
        
        textToArchitect = "If the user's request involves spatial placement of objects, this file has a method that places one object next to another.";
    }

    // Actual APIs start here ---------------------------------------------------


    public static void PlaceNextTo(GameObject targetObject, GameObject objectToPlace, Vector3 offset)
    {
        // Note: this demonstrates building skills on top of skills
        Bounds objectBounds = GetMeshSize.GetRenderBounds(objectToPlace);
        Bounds targetBounds = GetMeshSize.GetRenderBounds(targetObject);

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

    static Vector3 ComputeExitPointEfficient(Bounds box, Vector3 dir)
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

}
