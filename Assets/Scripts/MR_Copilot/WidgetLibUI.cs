using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Collections.Generic;

public class WidgetLibUI : MonoBehaviour
{
    // The Gameobject with the json string
    public GameObject Compiler;

    // The json string with key-value pairs
    private string widgetJson = ""; 

    // The Toggle prefab with a Text component as a child
    public GameObject togglePrefab;

    // The list of created toggles
    public List<Toggle> toggles;

    // The parent object that will hold the toggles as children
    private Transform parent;

    void Start()
    {
        parent = this.gameObject.transform;
        GridLayoutGroup grid = parent.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(200, 30);
        grid.spacing = new Vector2(10, 10);

        widgetJson = Compiler.GetComponent<WidgetSaveReload>().widgetJson;
        if (widgetJson != "" && widgetJson != null && widgetJson != " ")
        {
            CreateToggles();
        }
    }

    public void RefreshWidgetLib()
    {
        
        while (parent.childCount > 0)
        {
            DestroyImmediate(parent.GetChild(0).gameObject);
        }
    }

    public void SaveWidgetLib() 
    {
        widgetJson = Compiler.GetComponent<WidgetSaveReload>().widgetJson;
        if (widgetJson != "" && widgetJson != null && widgetJson != " ")
        {
            CreateToggles();
        }
    }

    private void CreateToggles()
    {
        //widgetJson = Compiler.GetComponent<WidgetSaveReload>().widgetJson;
        //Debug.Log(widgetJson);
        // Parse the json string into a dictionary of strings
        Dictionary<string, string> script = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(widgetJson);

        // Initialize the list of toggles
        toggles = new List<Toggle>();

        // Find or create the parent object and add a GridLayoutGroup component to it
        //parent = transform.Find("ToggleParent");
        //if (parent == null)
        //{
        //    parent = new GameObject("ToggleParent").transform;
        //    parent.SetParent(transform);
        //}
        
        // Loop through the key-value pairs and create a toggle for each one
        foreach (var pair in script)
        {
            // Instantiate a new toggle from the prefab and set its parent
            Toggle toggle = Instantiate(togglePrefab, parent).GetComponent<Toggle>();

            // Set the toggle's text to the pair's key value
            toggle.GetComponentInChildren<Text>().text = pair.Key;
            //toggle.GetComponentInChildren<Text>().text = pair.Key + ": " + pair.Value;

            // Add the toggle to the list
            toggles.Add(toggle);
        }
    }

    //public void GetWidgetLib()
    //{
    //    while (parent.childCount > 0)
    //    {
    //        foreach Toggle toggle in toggles)
    //        {
    //            if (toggle.isOff)
    //            {
    //                Debug.Log("toggle is off");
    //                Debug.Log("toggle text: " + toggle.GetComponentInChildren<Text>().text);
                    
    //            }
    //        }
    //    }
    //}
}