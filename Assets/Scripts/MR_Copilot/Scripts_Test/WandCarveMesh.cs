// To change the function of the wand tip to carve objects, I will do the following:
// 1. Create a new C# script called WandCarveMesh that inherits from Widgets and has a summary variable that explains its function.
// 2. In the WandCarveMesh script, find the WandTip GameObject by searching for its name.
// 3. In the WandCarveMesh script, write an Update() method that uses Raycast to detect if the WandTip GameObject is close enough to another object.
// 4. If the WandTip GameObject is close enough to another object, modify the object's mesh according to the normal direction of the wand tip.




using UnityEngine;

public class WandCarveMesh : Widgets
{
    private GameObject wandTip;
    private GameObject wandHandle;
    

    void Start()
    {
        summary = "This script allows the magic wand's tip to carve objects it touches by modifying their mesh according to the normal direction of the wand tip";

        // Find the WandTip GameObject by searching for its name.
        wandTip = GameObject.Find("WandTip");
        wandHandle = GameObject.Find("WandHandle");
    }

    void Update()
    {
        // Use Raycast to detect if the WandTip GameObject is close enough to another object.
        RaycastHit hit;
        float maxDistance = 0.5f;
        Ray ray = new Ray(wandTip.transform.position, wandTip.transform.forward);

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            Debug.Log("hit");
            // Get the MeshFilter component of the object.
            MeshFilter meshFilter = hit.collider.gameObject.GetComponent<MeshFilter>();
            MeshCollider meshCollider = hit.collider.gameObject.GetComponent<MeshCollider>();
            Debug.Log(hit.collider.gameObject);
            if (hit.collider.gameObject != wandTip && hit.collider.gameObject != wandHandle)
            {
                if (meshFilter != null && meshCollider != null)
                {
                    // Modify the object's mesh according to the normal direction of the wand tip.
                    Mesh meshMod = meshFilter.mesh;
                    Debug.Log(meshMod.ToString());
                    Vector3[] vertices = meshMod.vertices;
                    int[] triangles = meshMod.triangles;
                    Vector3 hitPoint = hit.point;
                    Vector3 hitNormal = hit.normal;

                    for (int i = 0; i < triangles.Length; i += 3)
                    {
                        Vector3 v0 = vertices[triangles[i]];
                        Vector3 v1 = vertices[triangles[i + 1]];
                        Vector3 v2 = vertices[triangles[i + 2]];
                        Debug.Log(Vector3.Distance(v0, hitPoint));
                        if (Vector3.Distance(v0, hitPoint) < maxDistance || Vector3.Distance(v1, hitPoint) < maxDistance || Vector3.Distance(v2, hitPoint) < maxDistance)
                        {
                            Debug.Log(hitNormal * 0.1f);
                            vertices[triangles[i]] -= hitNormal * 0.1f;
                            vertices[triangles[i + 1]] -= hitNormal * 0.1f;
                            vertices[triangles[i + 2]] -= hitNormal * 0.1f;
                        }
                    }

                    meshMod.vertices = vertices;
                    meshMod.RecalculateNormals();
                    meshMod.RecalculateBounds();
                    
                    meshFilter.mesh = meshMod;
                    meshCollider.sharedMesh = meshMod;
                }
            }
        }
    }
}