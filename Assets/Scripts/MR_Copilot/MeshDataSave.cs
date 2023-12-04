
using UnityEngine;
using System;
using System.IO; // Add this namespace to use the File class
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;


public class MeshDataSave : MonoBehaviour
{
    // A reference to the existing 3D object
    public GameObject objectToGetMesh;

    // A variable to store the mesh data
    private Mesh meshData;

    // A variable to store the mesh filter component
    private MeshFilter meshFilter;

    // A variable to store the file path
    private string filePath;
    public float samplingRate = 0.1f; // The fraction of vertices to keep
    private Mesh newMesh;

    private void Start()
    {
        // Get the mesh filter component of the object
        meshFilter = objectToGetMesh.GetComponent<MeshFilter>();

        // Check if the mesh filter exists and has a valid mesh
        if (meshFilter != null && meshFilter.mesh != null)
        {
            // Create a mesh data object from the mesh filter's mesh
            meshData = meshFilter.mesh;

            //SubSampleMesh(meshData);

            // Print some information about the mesh data
            Debug.Log("Mesh data created");
            // Set the file path to a txt file in the project folder
            filePath = @"Assets/Materials/" + objectToGetMesh.name+".txt";

            // Create a string builder to store the vertex positions
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            Vector3[] vertices = meshData.vertices;

            Vector3[] savedVtx = new Vector3[] { vertices[0] };

            // Loop through the vertices of the mesh data
            for (int i = 0; i < vertices.Length; i++)
            {
                // Get the vertex position
                float x = (float)Math.Round(vertices[i].x, 4);
                float y = (float)Math.Round(vertices[i].y, 4);
                float z = (float)Math.Round(vertices[i].z, 4);

                Vector3 vertex = new Vector3( x, y, z );
                if (System.Array.IndexOf(savedVtx, vertex) == -1)
                {
                    savedVtx = savedVtx.Concat(new Vector3[] { vertex }).ToArray();
                    string vx;
                    string vy;
                    string vz;
                    if (vertex.x == 0)
                    {
                        vx = "0";
                    }
                    else
                    {
                        vx = vertex.x.ToString("N4");
                    }
                    if (vertex.y == 0)
                    {
                        vy = "0";
                    }
                    else
                    {
                        vy = vertex.y.ToString("N4");
                    }
                    if (vertex.z == 0)
                    {
                        vz = "0";
                    }
                    else
                    {
                        vz = vertex.z.ToString("N4");
                    }
                    // Append the vertex position to the string builder, separated by commas
                    //sb.Append(vertex.x.ToString("N4") + "," + vertex.y.ToString("N4") + "," + vertex.z.ToString("N4") + ";");
                    sb.Append("[" + vx + "," + vy + "," + vz + "],");
                    
                }
            }
            Debug.Log(savedVtx.Length);
            VizMesh(savedVtx);


            // Write the string builder content to the file
            //File.WriteAllText(filePath, sb.ToString());

            // Print a message that the file is saved
            Debug.Log("File saved at " + filePath);

           
            
        }
        else
        {
            // Print a warning if the mesh filter is missing or has no mesh
            Debug.LogWarning("Mesh filter is null or has no mesh");
        }
    }

    private void VizMesh(Vector3[] savedVtx)
    {
        const float thres = 0.0001f;
        float xMin = 0.03f;
        float xMax = 0.06f;
        float yMin = 0.07f;
        float yMax = 0.09f;

        // Define some lists to store the indices of the interaction vertices
        List<int> handleIndices = new List<int>();


        // Loop through the vertices array and check their coordinates
        //for (int i = 0; i < savedVtx.Length; i++)
        //{
        //    // Get the current vertex
        //    Vector3 v = savedVtx[i];
        //    if ((Math.Abs(v.y - 0.01f) < 0.01f) && (v.x > -0.04f && v.x < 0.04f && v.z > -0.04f && v.z < 0.04f))
        //    {
        //        holdVertices.Add(i);
        //    }
            
        //}


        // Loop through the vertices array
        for (int i = 0; i < savedVtx.Length; i++)
        {
            // Get the current vertex
            Vector3 vertex = savedVtx[i];

            // Check if the vertex is on the positive x-axis
            if (vertex.x > 0)
            {
                // Check if the vertex is roughly between y = 0.05 and y = 0.1
                if (vertex.y >= 0.05f && vertex.y <= 0.1f)
                {
                    // Check if the vertex is roughly between z = -0.04 and z = 0.04
                    if (vertex.z >= -0.04f && vertex.z <= 0.04f)
                    {
                        // Add the index of the vertex to the handle list
                        handleIndices.Add(i);
                    }
                }
            }
            // Check if the vertex is at the ends of the loop
            else if (vertex.x == -0.01f)
            {
                // Check if the vertex is roughly between y = 0.05 and y = 0.1
                if (vertex.y >= 0.05f && vertex.y <= 0.1f)
                {
                    // Check if the vertex is roughly between z = -0.04 and z = 0.04
                    if (vertex.z >= -0.04f && vertex.z <= 0.04f)
                    {
                        // Add the index of the vertex to the handle list
                        handleIndices.Add(i);
                    }
                }
            }
        }

        List<List<double>> handleIndices2 = new List<List<double>>();
        //handleIndices2.Add( new List<double> {-0.0609, 0.0927, 0.0035});
        //handleIndices2.Add( new List<double> {-0.0609, 0.0927, 0.0035});
        //handleIndices2.Add( new List<double> {-0.0649, 0.0862, 0.0001});
        //handleIndices2.Add( new List<double> {-0.0641, 0.0862, 0.0035});
        //handleIndices2.Add( new List<double> {-0.0621, 0.0862, 0.0058});
        //handleIndices2.Add( new List<double> {-0.0616, 0.0932, 0.0001});
        //handleIndices2.Add( new List<double> {-0.0609, 0.0927, -0.0035});
        //handleIndices2.Add( new List<double> {-0.0621, 0.0862, -0.0057});
        //handleIndices2.Add( new List<double> {-0.0641, 0.0862, -0.0035});
        //handleIndices2.Add( new List<double> {-0.0568, 0.0867, 0.0001});
        //handleIndices2.Add( new List<double> {-0.0577, 0.0865, 0.0035});
        //handleIndices2.Add( new List<double> {-0.0547, 0.0904, 0.0001});
        //handleIndices2.Add( new List<double> {-0.0555, 0.0906, 0.0035});
        //handleIndices2.Add( new List<double> {-0.0577, 0.0865, -0.0035});
        //handleIndices2.Add( new List<double> {-0.0596, 0.0863, -0.0057});
        //handleIndices2.Add( new List<double> {-0.0555, 0.0906, -0.0035});
        //handleIndices2.Add( new List<double> {-0.0572, 0.0911, -0.0058});
        //handleIndices2.Add( new List<double> {-0.0547, 0.0904, 0.0001});
        //handleIndices2.Add( new List<double> {-0.0555, 0.0906, 0.0035});
        //handleIndices2.Add( new List<double> {-0.0552, 0.0970, 0.0035});
        //handleIndices2.Add( new List<double> {-0.0557, 0.0977, 0.0001});
        //handleIndices2.Add( new List<double> {-0.0552, 0.0970, -0.0035});
        //handleIndices2.Add( new List<double> {-0.0572, 0.0911, -0.0058});
        //handleIndices2.Add( new List<double> {-0.0555, 0.0906, -0.0035});
        //handleIndices2.Add( new List<double> {-0.0542, 0.0957, -0.0058});
        //handleIndices2.Add( new List<double> {-0.0557, 0.0977, 0.0001});
        //handleIndices2.Add( new List<double> {-0.0552, 0.0970, 0.0035});
        //handleIndices2.Add( new List<double> {-0.0482, 0.0988, 0.0035});
        //handleIndices2.Add( new List<double> {-0.0485, 0.0991, 0.0001});
        //handleIndices2.Add( new List<double> {-0.0482, 0.0988, -0.0035});
        //handleIndices2.Add( new List<double> {-0.0542, 0.0957, -0.0058});
        //handleIndices2.Add( new List<double> {-0.0552, 0.0970, -0.0035});
        //handleIndices2.Add( new List<double> {-0.0477, 0.0973, -0.0058});
        //handleIndices2.Add( new List<double> {-0.0485, 0.0991, 0.0001});
        //handleIndices2.Add( new List<double> {-0.0482, 0.0988, 0.0035});
        //handleIndices2.Add( new List<double> {-0.0421, 0.0985, 0.0035});
        //handleIndices2.Add( new List<double> {-0.0421, 0.0991, 0.0001});
        //handleIndices2.Add( new List<double> {-0.0421, 0.0985, -0.0035});
        //handleIndices2.Add( new List<double> {-0.0477, 0.0973, -0.0058});
        //handleIndices2.Add( new List<double> {-0.0482, 0.0988, -0.0035});
        //handleIndices2.Add( new List<double> {-0.0419, 0.0972, -0.0058});
        //handleIndices2.Add( new List<double> {-0.0421, 0.0991, 0.0001});
        //handleIndices2.Add( new List<double> {-0.0421, 0.0985, 0.0035});
        //handleIndices2.Add( new List<double> {-0.0403, 0.0985, 0.0035});
        //handleIndices2.Add( new List<double> {-0.0404, 0.0990, 0.0001});
        //handleIndices2.Add( new List<double> {-0.0403, 0.0985, -0.0035});
        //handleIndices2.Add( new List<double> {-0.0419, 0.0972, -0.0058});
        //handleIndices2.Add( new List<double> {-0.0421, 0.0985, -0.0035});
        //handleIndices2.Add( new List<double> {-0.0404, 0.0970, -0.0058});
        //handleIndices2.Add( new List<double> {-0.0404, 0.0990, 0.0001});
        //handleIndices2.Add( new List<double> {-0.0403, 0.0985, 0.0035});
        //handleIndices2.Add( new List<double> {0.0001, 0.1001, 0.0035});
        //handleIndices2.Add( new List<double> {0.0001, 0.1001, 0.0001});
        //handleIndices2.Add( new List<double> {-0.0403, 0.0985, -0.0035});
        //handleIndices2.Add( new List<double> {0.0001, 0.1001, -0.0035});
        //handleIndices2.Add( new List<double> {-0.0404, 0.0970, -0.0058});
        //handleIndices2.Add( new List<double> { 0.0001, 0.1001, -0.0058});

        handleIndices2.Add( new List<double> { -0.0649,0.0862,0.0001 });
        handleIndices2.Add( new List<double> {-0.0641,0.0862,0.0035});
        handleIndices2.Add( new List<double> {-0.0621,0.0862,0.0058 });
        handleIndices2.Add( new List<double> {-0.0641,0.0862,-0.0035});
        handleIndices2.Add( new List<double> {-0.0621,0.0862,-0.0057 });
        handleIndices2.Add( new List<double> {-0.0568,0.0867,0.0001});
        handleIndices2.Add( new List<double> {-0.0577,0.0865,0.0035 });
        handleIndices2.Add( new List<double> {-0.0577,0.0865,-0.0035});
        handleIndices2.Add( new List<double> {-0.0596,0.0863,-0.0057 });
        handleIndices2.Add( new List<double> {-0.0596,0.0863,0.0058 });

        Debug.Log("Hold vertices: " + string.Join(", ", handleIndices));
        for (int i = 0; i < savedVtx.Length; i++)
        {
            //sanity check
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = savedVtx[i];
            sphere.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);

            if (handleIndices.Contains(i))
            {
                sphere.GetComponent<MeshRenderer>().material.color = Color.red;
                sphere.name = "hold_" + i.ToString();
            }
            else
            {
                sphere.GetComponent<MeshRenderer>().material.color = Color.black;
                sphere.name = "sphere_" + i.ToString();
            }
        }
        foreach (var tempVertex in handleIndices2)
        {
            float x = (float)Math.Round(tempVertex[0], 4);
            float y = (float)Math.Round(tempVertex[1], 4);
            float z = (float)Math.Round(tempVertex[2], 4);

            Vector3 vertex = new Vector3(x, y, z);
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere); 
            sphere.transform.position = vertex;
            sphere.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
            sphere.GetComponent<MeshRenderer>().material.color = Color.blue;
            sphere.name = "hold_temp_";
            
            
        }
    }


    private void SubSampleMesh(Mesh originalMesh)
    {
        newMesh = new Mesh();

        // Get the indices of the original mesh
        int[] originalIndices = originalMesh.GetIndices(0);

        // Get the number of vertices to keep
        int newVertexCount = Mathf.RoundToInt(originalIndices.Length * samplingRate);

        // Create an array to store the selected indices
        int[] newIndices = new int[newVertexCount];

        // Create a random number generator
        System.Random random = new System.Random();

        // Loop through the new indices array
        for (int i = 0; i < newIndices.Length; i++)
        {
            // Pick a random index from the original indices array
            int randomIndex = random.Next(originalIndices.Length);

            // Assign the random index to the new indices array
            newIndices[i] = originalIndices[randomIndex];
        }

        // Get the vertices of the original mesh
        Vector3[] originalVertices = originalMesh.vertices;

        // Create an array to store the selected vertices
        Vector3[] newVertices = new Vector3[newVertexCount];

        // Create a dictionary to store the mapping from the original indices to the new indices
        Dictionary<int,int> indexMap = new Dictionary<int,int>();

        // Loop through the new vertices array
        for (int i = 0; i < newVertices.Length; i++)
        {
            // Get the index of the selected vertex
            int index = newIndices[i];

            // Assign the position of the selected vertex to the new vertices array
            newVertices[i] = originalVertices[index];

            // Add the mapping from the original index to the new index to the dictionary
            indexMap[index] = i;
        }

        // Set the vertices of the new mesh first
        newMesh.vertices = newVertices;

        // Loop through the new indices array again
        for (int i = 0; i < newIndices.Length; i++)
        {
            // Get the original index of the selected vertex
            int index = newIndices[i];

            // Remap the original index to the new index using the dictionary
            newIndices[i] = indexMap[index];
        }

        // Then set the indices of the new mesh
        newMesh.SetIndices(newIndices, MeshTopology.Triangles, 0);

        // Optionally, get and set the other attributes of the vertices, such as normals, uvs, colors, etc.

        // Optionally, recalculate the bounds and normals of the new mesh
        newMesh.RecalculateBounds();
            newMesh.RecalculateNormals();

            objectToGetMesh.GetComponent<MeshFilter>().mesh = newMesh;
    }

}