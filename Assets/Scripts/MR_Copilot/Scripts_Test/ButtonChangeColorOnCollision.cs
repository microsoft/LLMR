using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

//public class FooTest : Widgets
//{
//    // Start is called before the first frame update
//    void Start()
//    {
//        summary = "This widget is a foo";
//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }

public class ButtonChangeColorOnCollision : Widgets
{
    private GameObject button;
    public class CreateButtonWidget : Widgets 
    {
        private GameObject button;
        void Start()
        {
            summary = "This script creates a widget named Button if the button widget has not been created";
           // Check if the Button GameObject has been created.
            button = GameObject.Find("Button");
            // if the Button GameObject has not been created, create it
            if (button == null) {
                // Create a new GameObject called Button and make it a child of the "---Widgets---" GameObject.
                button = new GameObject("Button");
                button.transform.parent = GameObject.Find("---Widgets---").transform;

                // Add a BoxCollider and a MeshRenderer component to the Button GameObject.
                button.AddComponent<BoxCollider>();
                button.AddComponent<MeshRenderer>();
            }
        }
    }

    // Create a new C# script called ChangeColorOnCollision that inherits from Widgets and has a summary variable that explains its function.
    public class ChangeColorOnCollision : Widgets
        {
            // Declare a public Color variable called color and assign it a random value in the Start() method.
            public Color color;

            void Start()
            {
                summary = "This script changes the color of the object when it collides with another object";
                // Assign a random color to the color variable.
                color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            }

            // Write a OnCollisionEnter() method that changes the color of the MeshRenderer component to the color variable.
            void OnCollisionEnter(Collision collision)
            {
                // Get the MeshRenderer component of the object.
                MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

                // Change the color of the MeshRenderer component to the color variable.
                meshRenderer.material.color = color;
            }
        }

    void Start()
    {
        summary = "This script attach the ChangeColorOnCollision script to the Button GameObject";
        button = GameObject.Find("Button");
        // if the Button GameObject has not been created, create it
        if (button != null)
        {
            // Attach the ChangeColorOnCollision script to the Button GameObject.
            button.AddComponent<ChangeColorOnCollision>();
        }
    }
    

}
