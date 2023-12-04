using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacementTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SketchfabLoader loader = gameObject.GetComponent<SketchfabLoader>();
        GameObject rabbit = loader.Load("rabbit");
        loader.Move(rabbit, new Vector3(1, 1, 1));
        loader.Scale(rabbit, 0.3f);

        GameObject girl = loader.Load("girl");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
