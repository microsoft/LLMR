using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CreateAnimation : Skills
{
    // Start is called before the first frame update
    void Awake()
    {
        textToArchitect = "Creates an animation that fits the clip_name for object. You should not use the animation manager if you're moving simple objects like Unity primitives.";

        string path = Path.Combine("Scripts", "MR_Copilot", "SkillLibrary", "animation_generation.txt");
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
