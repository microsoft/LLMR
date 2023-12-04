using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class DepthAnalyzer : ChatBot
{
    public Option option;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public enum Option
    {
        Raycast, MonocularDepth, HL_sensor
    }

    public List<Vector3> GetObjCoords(DetectedObject[] detected_obj)
    {
        List<float> depths = new List<float>();
        List<Vector3> coords = new List<Vector3>();
        bool in_world_coords = false;
        if (option == Option.Raycast)
        {
            List<float> ret = new List<float>();
            for (int i = 0; i < detected_obj.Length; i++)
            {
                DetectedObject obj = detected_obj[i];
                // cast a ray to detect the object's world position.
                Ray ray = Camera.main.ScreenPointToRay(Utils.image_to_screen_space(obj.center));

                // Declare a variable to store the raycast hit information
                RaycastHit hit;

                // Perform the raycast and check if it hits anything
                if (Physics.Raycast(ray, out hit))
                {
                    ////debug
                    //Color raycastColor = Color.red;
                    //// A duration for the raycast line
                    //float raycastDuration = 1000f;
                    //Debug.DrawLine(ray.origin, hit.point, raycastColor, raycastDuration);

                    // Get the position of the hit point
                    Vector3 hitPosition = hit.point;
                    coords.Add(hitPosition);
                }
            }
            in_world_coords = true;
        }
        else if (option == Option.MonocularDepth)
        {
            throw new NotImplementedException("Not implemented.");
        }
        else if (option == Option.HL_sensor)
        {
            throw new NotImplementedException("Not implemented.");
        }
        else
        {
            throw new NotImplementedException("Not implemented.");
        }

        // transform image space coordinate + depth to world space coordinates
        if (!in_world_coords)
        {
            // assume depths is processed and has the same length as detected_obj
            for(int i=0; i < detected_obj.Length;i++)
            {
                DetectedObject obj = detected_obj[i];
                float depth = depths[i];
                coords.Add(Utils.image_to_world_space(obj.center, depth));
            }
        }

        return coords;
    }

    public List<float> GetDepths(DetectedObject[] detected_obj)
    {
        if (option == Option.Raycast)
        {
            List<float> ret = new List<float>();
            for (int i = 0; i < detected_obj.Length; i++)
            {
                ret.Add(1f);
            }

            return ret;
        }
        else if (option == Option.MonocularDepth)
        {
            throw new NotImplementedException("Not implemented.");
        }
        else if (option == Option.HL_sensor)
        {
            throw new NotImplementedException("Not implemented.");
        }
        else
        {
            throw new NotImplementedException("Not implemented.");
        }
    }
}
