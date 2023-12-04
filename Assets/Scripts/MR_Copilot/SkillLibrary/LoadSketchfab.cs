using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LoadSketchfab : Skills
{
    // Start is called before the first frame update
    void Awake()
    {
        textToArchitect = "Creates complex objects from Sketchfab. You should not use this skill manager if you're asked to create things using simple primitives.";

        // textToBuilder = File.ReadAllText(@"Assets\Scripts\MR_Copilot\SkillLibrary\sketchfab_loading.txt").ToString();
        string path = Path.Combine("Scripts", "MR_Copilot", "SkillLibrary", "sketchfab_loading.txt");
        // Use Application.dataPath to get the absolute path to the Assets folder
        string fullPath = Path.Combine(Application.dataPath, path);
        // Use File.ReadAllText to read the file contents
        textToBuilder = File.ReadAllText(fullPath).ToString();
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
