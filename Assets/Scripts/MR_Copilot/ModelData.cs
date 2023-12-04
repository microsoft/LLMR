using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelData : MonoBehaviour
{
    public string label; // the name of the model in Sketchfab
    public Vector3 position; // the desired position in the scene
    public float scale; // the desired scale factor
    public string nexto_label;

    public string uid;
    void Start()
    {

    }

    // A constructor to initialize the fields
    public ModelData(string uid, string label, string nexto_label, Vector3 position, float scale)
    {
        this.label = label;
        this.position = position;
        this.scale = scale;
        this.uid = uid;
        this.nexto_label = nexto_label;
        
    }
}
