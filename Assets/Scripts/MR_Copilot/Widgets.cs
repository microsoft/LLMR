/*
 * Cathy Fang, 6/12/2023
 * Widgets.cs inherits from the MonoBehaviour class and is the base class for all widgets.
 * Each widget has a public string summary that is set in the Start() method of the widget.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Widgets : MonoBehaviour
{
    //public string name;
    public string summary;
    // Start is called before the first frame update
    void Start()
    {
        summary = "This widget is a test";
        updateWidgetDB();
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void updateWidgetDB()
    {
        Debug.Log(summary);
    }



    

}
