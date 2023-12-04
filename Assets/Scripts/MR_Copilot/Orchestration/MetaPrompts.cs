using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MetaPrompts : MonoBehaviour
{
    // NOTE: the directories take priority when both the dir and the actual string are specified.
    public string builder_Base_metaprompt_filename;
    public string builder_SceneAnalyzer_SkillLibrary_metaprompt_filename;
    public string builder_SceneAnalyzer_metaprompt_filename;
    public string builder_SkillLibrary_metaprompt_filename;
    public string builder_zero_shot_metaprompt_filename;
    public string builder_FuzzyModelsAddition_metaprompt_filename;
    public string inspector_base_metaprompt_filename;
    public string inspector_SkillLibrary_metaprompt_filename;

    // if the Builder also receives scene summary, we use a separate metaprompt that contains
    // examples with scene summaries in them
    [TextArea(5, 20)]
    public string builder_Base_metaprompt;

    [TextArea(5, 20)]
    public string builder_SceneAnalyzer_SkillLibrary_metaprompt;

    [TextArea(5, 20)]
    public string builder_SceneAnalyzer_metaprompt;

    [TextArea(5, 20)]
    public string builder_SkillLibrary_metaprompt;

    [TextArea(5, 20)]
    public string builder_zero_shot_metaprompt;

    [TextArea(5, 20)]
    public string builder_FuzzyModelsAddition_metaprompt;

    [TextArea(5, 20)]
    public string inspector_base_metaprompt;

    [TextArea(5, 20)]
    public string inspector_SkillLibrary_metaprompt;

    void Awake()
    {
        if (builder_Base_metaprompt_filename != "")
        {
            builder_Base_metaprompt = LoadMetapromptFromFile(builder_Base_metaprompt_filename);
        }

        if (builder_SceneAnalyzer_SkillLibrary_metaprompt_filename != "")
        {
            builder_SceneAnalyzer_SkillLibrary_metaprompt = LoadMetapromptFromFile(builder_SceneAnalyzer_SkillLibrary_metaprompt_filename);
        }

        if (builder_SceneAnalyzer_metaprompt_filename != "")
        {
            builder_SceneAnalyzer_metaprompt = LoadMetapromptFromFile(builder_SceneAnalyzer_metaprompt_filename);
        }

        if (builder_SkillLibrary_metaprompt_filename != "")
        {
            builder_SkillLibrary_metaprompt = LoadMetapromptFromFile(builder_SkillLibrary_metaprompt_filename);
        }

        if (builder_zero_shot_metaprompt_filename != "")
        {
            builder_zero_shot_metaprompt = LoadMetapromptFromFile(builder_zero_shot_metaprompt_filename);
        }

        if (builder_FuzzyModelsAddition_metaprompt_filename != "")
        {
            builder_FuzzyModelsAddition_metaprompt = LoadMetapromptFromFile(builder_FuzzyModelsAddition_metaprompt_filename);
        }

        if (inspector_base_metaprompt_filename != "")
        {
            inspector_base_metaprompt = LoadMetapromptFromFile(inspector_base_metaprompt_filename);
        }

        if (inspector_SkillLibrary_metaprompt_filename != "")
        {
            inspector_SkillLibrary_metaprompt = LoadMetapromptFromFile(inspector_SkillLibrary_metaprompt_filename);
        }
    }

    string LoadMetapromptFromFile(string name)
    {
        string path = Path.Combine("Scripts", "MetaPrompt", name + ".txt");
        // Use Application.dataPath to get the absolute path to the Assets folder
        string fullPath = Path.Combine(Application.dataPath, path);
        // Use File.ReadAllText to read the file contents
        return File.ReadAllText(fullPath).ToString();
    }
}
