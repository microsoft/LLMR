////using UnityEngine;

////[RequireComponent(typeof(MeshFilter))]
////[RequireComponent(typeof(MeshCollider))]
////public class TestGenCode : MonoBehaviour
////{
////    // The mesh to sculpt
////    private Mesh mesh;

////    // The mesh collider to update
////    private MeshCollider meshCollider;

////    // The sculpting radius
////    public float radius = 0.5f;

////    // The sculpting strength
////    public float strength = 0.01f;

////    // The sculpting mode (add or subtract)
////    public bool add = true;

////    // The layer mask to filter the raycast
////    public LayerMask layerMask;

////    private void Start()
////    {
////        // Get the mesh and the mesh collider components
////        mesh = GetComponent<MeshFilter>().mesh;
////        meshCollider = GetComponent<MeshCollider>();
////    }

////    private void SmoothMesh(Vector3[] vertices, Vector3[] normals)
////    {
////        // Define a smoothing factor that controls the strength of the smoothing
////        float smoothingFactor = 0.5f;

////        // Define a smoothing radius that controls the range of the smoothing
////        float smoothingRadius = 0.2f;

////        // Loop through the vertices
////        for (int i = 0; i < vertices.Length; i++)
////        {
////            // Convert the vertex to world space
////            Vector3 vertex = transform.TransformPoint(vertices[i]);

////            // Initialize a sum vector and a weight sum
////            Vector3 sum = Vector3.zero;
////            float weightSum = 0f;

////            // Loop through the neighboring vertices
////            for (int j = 0; j < vertices.Length; j++)
////            {
////                // Skip the current vertex
////                if (i == j) continue;

////                // Convert the neighbor vertex to world space
////                Vector3 neighbor = transform.TransformPoint(vertices[j]);

////                // Calculate the distance and direction from the current vertex
////                float distance = Vector3.Distance(vertex, neighbor);
////                Vector3 direction = (neighbor - vertex).normalized;

////                // If the distance is within the smoothing radius
////                if (distance < smoothingRadius)
////                {
////                    // Calculate a Gaussian weight based on the distance
////                    float weight = Mathf.Exp(-distance * distance / (2f * smoothingRadius * smoothingRadius));

////                    // Add the weighted neighbor vertex and normal to the sum vector
////                    sum += weight * neighbor;
////                    sum += weight * direction;

////                    // Add the weight to the weight sum
////                    weightSum += weight;
////                }
////            }

////            // If the weight sum is not zero
////            if (weightSum > 0f)
////            {
////                // Calculate the smoothed vertex and normal by dividing the sum vector by the weight sum
////                Vector3 smoothedVertex = sum / weightSum;
////                Vector3 smoothedNormal = sum / weightSum - smoothedVertex;

////                // Lerp the original vertex and normal with the smoothed ones based on the smoothing factor
////                vertex = Vector3.Lerp(vertex, smoothedVertex, smoothingFactor);
////                normals[i] = Vector3.Lerp(normals[i], smoothedNormal, smoothingFactor);

////                // Convert the vertex back to local space
////                vertices[i] = transform.InverseTransformPoint(vertex);
////            }
////        }
////    }

////    private void Update()
////    {
////        // If the left mouse button is pressed
////        if (Input.GetMouseButton(0))
////        {
////            // Cast a ray from the mouse position to the scene
////            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
////            RaycastHit hit;

////            // If the ray hits the mesh
////            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
////            {
////                // Get the hit point and normal
////                Vector3 hitPoint = hit.point;
////                Vector3 hitNormal = hit.normal;

////                // Get the mesh vertices and normals
////                Vector3[] vertices = mesh.vertices;
////                Vector3[] normals = mesh.normals;

////                // Loop through the vertices
////                for (int i = 0; i < vertices.Length; i++)
////                {
////                    // Convert the vertex to world space
////                    Vector3 vertex = transform.TransformPoint(vertices[i]);

////                    // Calculate the distance and direction from the hit point
////                    float distance = Vector3.Distance(vertex, hitPoint);
////                    Vector3 direction = (vertex - hitPoint).normalized;

////                    // If the distance is within the radius
////                    if (distance < radius)
////                    {
////                        // Calculate a falloff factor based on the distance
////                        float falloff = 1f - distance / radius;

////                        // Modify the vertex position and normal based on the sculpting mode, strength and falloff
////                        if (add)
////                        {
////                            vertex += hitNormal * strength * falloff;
////                            normals[i] += hitNormal * strength * falloff;
////                        }
////                        else
////                        {
////                            vertex -= hitNormal * strength * falloff;
////                            normals[i] -= hitNormal * strength * falloff;
////                        }

////                        // Convert the vertex back to local space
////                        vertices[i] = transform.InverseTransformPoint(vertex);
////                    }
////                }

////                // Update the mesh vertices and normals
////                mesh.vertices = vertices;
////                mesh.normals = normals;

////                // Recalculate the mesh bounds and tangents
////                mesh.RecalculateBounds();
////                mesh.RecalculateTangents();

////                // Update the mesh collider
////                meshCollider.sharedMesh = null;
////                meshCollider.sharedMesh = mesh;


////            }
////        }
////    }
////}


//using UnityEngine;

//[RequireComponent(typeof(MeshFilter))]
//[RequireComponent(typeof(MeshCollider))]
//public class TestGenCode : MonoBehaviour
//{
//    // The mesh to sculpt
//    private Mesh mesh;

//    // The mesh collider to update
//    private MeshCollider meshCollider;

//    // The sculpting radius
//    public float radius = 0.3f;

//    // The sculpting strength
//    public float strength = 0.01f;

//    // The sculpting mode (add or subtract)
//    public bool add = true;

//    // The layer mask to filter the raycast
//    public LayerMask layerMask;

//    private void Start()
//    {
//        // Get the mesh and the mesh collider components
//        mesh = GetComponent<MeshFilter>().mesh;
//        meshCollider = GetComponent<MeshCollider>();
//    }

//    private void Update()
//    {
//        // If the left mouse button is pressed
//        if (Input.GetMouseButton(0))
//        {
//            // Cast a ray from the mouse position to the scene
//            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//            RaycastHit hit;

//            // If the ray hits the mesh
//            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
//            {
//                // Get the hit point and normal
//                Vector3 hitPoint = hit.point;
//                Vector3 hitNormal = hit.normal;

//                // Get the mesh vertices and normals
//                Vector3[] vertices = mesh.vertices;
//                Vector3[] normals = mesh.normals;

//                // Loop through the vertices
//                for (int i = 0; i < vertices.Length; i++)
//                {
//                    // Convert the vertex to world space
//                    Vector3 vertex = transform.TransformPoint(vertices[i]);

//                    // Calculate the distance and direction from the hit point
//                    float distance = Vector3.Distance(vertex, hitPoint);
//                    Vector3 direction = (vertex - hitPoint).normalized;

//                    // If the distance is within the radius
//                    if (distance < radius)
//                    {
//                        // Calculate a falloff factor based on the distance using a cosine function
//                        float falloff = Mathf.Cos(Mathf.PI * 0.5f * distance / radius);

//                        // Modify the vertex position based on the sculpting mode, strength and falloff
//                        if (add)
//                        {
//                            vertex += hitNormal * strength * falloff;
//                        }
//                        else
//                        {
//                            vertex -= hitNormal * strength * falloff;
//                        }

//                        // Convert the vertex back to local space
//                        vertices[i] = transform.InverseTransformPoint(vertex);
//                    }
//                }

//                // Loop through the vertices again
//                for (int i = 0; i < vertices.Length; i++)
//                {
//                    // Convert the vertex to world space
//                    Vector3 vertex = transform.TransformPoint(vertices[i]);

//                    // Initialize a variable to store the average normal
//                    Vector3 averageNormal = Vector3.zero;

//                    // Initialize a variable to count the number of neighbors
//                    int neighborCount = 0;

//                    // Loop through the other vertices
//                    for (int j = 0; j < vertices.Length; j++)
//                    {
//                        // Skip the same vertex
//                        if (i == j) continue;

//                        // Convert the other vertex to world space
//                        Vector3 otherVertex = transform.TransformPoint(vertices[j]);

//                        // Calculate the distance from the other vertex
//                        float distance = Vector3.Distance(vertex, otherVertex);

//                        // If the distance is within the radius
//                        if (distance < radius)
//                        {
//                            // Add the other vertex normal to the average normal
//                            averageNormal += normals[j];

//                            // Increment the neighbor count
//                            neighborCount++;
//                        }
//                    }

//                    // If there are any neighbors
//                    if (neighborCount > 0)
//                    {
//                        // Divide the average normal by the neighbor count
//                        averageNormal /= neighborCount;

//                        // Normalize the average normal
//                        averageNormal.Normalize();

//                        // Assign the average normal to the vertex normal
//                        normals[i] = averageNormal;
//                    }
//                }

//                // Update the mesh vertices and normals
//                mesh.vertices = vertices;
//                mesh.normals = normals;

//                // Recalculate the mesh bounds and tangents
//                mesh.RecalculateBounds();
//                mesh.RecalculateTangents();

//                // Update the mesh collider
//                meshCollider.sharedMesh = null;
//                meshCollider.sharedMesh = mesh;
//            }
//        }
//    }
//}


//using UnityEngine;

//public class TestGenCode : Widgets
//{
//    private Camera mainCamera;
//    private GameObject selectedObject;
//    private Vector3 lastMousePosition;
//    private float sculptingStrength = 0.1f;

//    void Start()
//    {
//        summary = "This script allows you to click and drag to sculpt meshes on objects in the scene";
//        mainCamera = Camera.main;
//    }

//    void Update()
//    {
//        if (Input.GetMouseButtonDown(0))
//        {
//            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
//            RaycastHit hit;

//            if (Physics.Raycast(ray, out hit))
//            {
//                selectedObject = hit.collider.gameObject;
//                print (selectedObject.name);
//                lastMousePosition = Input.mousePosition;
//            }
//        }

//        if (Input.GetMouseButton(0) && selectedObject != null)
//        {
//            SculptMesh();
//        }

//        if (Input.GetMouseButtonUp(0))
//        {
//            selectedObject = null;
//        }
//    }

//    private void SculptMesh()
//    {
//        Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
//        //print(mouseDelta);
//        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
//        RaycastHit hit;

//        if (Physics.Raycast(ray, out hit))
//        {
//            Mesh mesh = selectedObject.GetComponent<MeshFilter>().mesh;
//            Vector3[] vertices = mesh.vertices;
//            int[] triangles = mesh.triangles;
//            Vector3 localHitPoint = selectedObject.transform.InverseTransformPoint(hit.point);

//            for (int i = 0; i < vertices.Length; i++)
//            {
//                float distance = Vector3.Distance(localHitPoint, vertices[i]);
//                print("distance: " + distance.ToString());

//                if (distance < sculptingStrength)
//                {
//                    Vector3 direction = (vertices[i] - localHitPoint).normalized;
//                    vertices[i] += direction * mouseDelta.magnitude * sculptingStrength;
//                }
//            }

//            mesh.vertices = vertices;
//            mesh.RecalculateNormals();
//            mesh.RecalculateBounds();
//            selectedObject.GetComponent<MeshCollider>().sharedMesh = mesh;
//        }

//        lastMousePosition = Input.mousePosition;
//    }
//}



using UnityEngine;

public class TestGenCode : Widgets
{
    private Camera mainCamera;
    private Vector3 previousMousePosition;
    public float sculptRadius = 1f;
    public float sculptStrength = 0.1f;

    void Start()
    {
        summary = "This script allows you to click and drag to sculpt meshes on objects in the scene";
        mainCamera = Camera.main;
    }

    //void Update()
    //{
    //    //print("previous loc: " + previousMousePosition.ToString());
    //    //print("curr loc: " + Input.mousePosition.ToString());

    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        //print("mousedown");
    //        previousMousePosition = Input.mousePosition;
    //    }

    //    if (Input.GetMouseButton(0))
    //    {
    //        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
    //        RaycastHit hit;

    //        if (Physics.Raycast(ray, out hit))
    //        {
    //            MeshCollider meshCollider = hit.collider as MeshCollider;
    //            if (meshCollider != null)
    //            {
    //                Mesh mesh = meshCollider.sharedMesh;
    //                Vector3[] vertices = mesh.vertices;
    //                int[] triangles = mesh.triangles;
    //                Vector3 localHitPoint = meshCollider.transform.InverseTransformPoint(hit.point);
    //                int closestVertexIndex = -1;
    //                float closestDistance = Mathf.Infinity;

    //                for (int i = 0; i < vertices.Length; i++)
    //                {
    //                    float distance = Vector3.Distance(localHitPoint, vertices[i]);
    //                    if (distance < closestDistance)
    //                    {
    //                        closestDistance = distance;
    //                        closestVertexIndex = i;
    //                    }
    //                }

    //                Vector3 dragDirection = mainCamera.ScreenToWorldPoint(Input.mousePosition) - mainCamera.ScreenToWorldPoint(previousMousePosition);
    //                print(dragDirection);
    //                dragDirection = meshCollider.transform.InverseTransformDirection(dragDirection);

    //                //print(dragDirection);

    //                for (int i = 0; i < vertices.Length; i++)
    //                {
    //                    float distance = Vector3.Distance(vertices[i], vertices[closestVertexIndex]);
    //                    //print("Distance: " + distance.ToString());
    //                    if (distance < sculptRadius)
    //                    {
    //                        //print("Sculpting");
    //                        float influence = Mathf.Clamp01(1f - (distance / sculptRadius));
    //                        vertices[i] += dragDirection * sculptStrength * influence;
    //                    }
    //                }

    //                mesh.vertices = vertices;
    //                mesh.RecalculateNormals();
    //                mesh.RecalculateBounds();
    //            }
    //        }

    //        previousMousePosition = Input.mousePosition;
    //    }
    //}

    void Update()
    {
        //print("previous loc: " + previousMousePosition.ToString());
        //print("curr loc: " + Input.mousePosition.ToString());

        if (Input.GetMouseButtonDown(0))
        {
            //print("mousedown");
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Store the initial hit point when the mouse button is pressed
            if (Physics.Raycast(ray, out hit))
            {
                previousMousePosition = hit.point;
            }
        }

        if (Input.GetMouseButton(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                MeshCollider meshCollider = hit.collider as MeshCollider;
                if (meshCollider != null)
                {
                    Mesh mesh = meshCollider.sharedMesh;
                    Vector3[] vertices = mesh.vertices;
                    int[] triangles = mesh.triangles;
                    Vector3 localHitPoint = meshCollider.transform.InverseTransformPoint(hit.point);
                    int closestVertexIndex = -1;
                    float closestDistance = Mathf.Infinity;

                    for (int i = 0; i < vertices.Length; i++)
                    {
                        float distance = Vector3.Distance(localHitPoint, vertices[i]);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestVertexIndex = i;
                        }
                    }

                    // Use the current hit point and the previous hit point to calculate the drag direction
                    Vector3 dragDirection = hit.point - previousMousePosition;
                    //print(dragDirection);
                    dragDirection = meshCollider.transform.InverseTransformDirection(dragDirection);

                    print(dragDirection);

                    for (int i = 0; i < vertices.Length; i++)
                    {
                        float distance = Vector3.Distance(vertices[i], vertices[closestVertexIndex]);
                        //print("Distance: " + distance.ToString());
                        if (distance < sculptRadius)
                        {
                            print("Sculpting");
                            float influence = Mathf.Clamp01(1f - (distance / sculptRadius));
                            vertices[i] += dragDirection * sculptStrength * influence;
                        }
                    }

                    mesh.vertices = vertices;
                    mesh.RecalculateNormals();
                    mesh.RecalculateBounds();
                }
            }

            // Update the previous hit point for the next frame
            previousMousePosition = hit.point;
        }
    }
}