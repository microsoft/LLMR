using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inspector : ChatBot
{
    public int max_inspection_count;
    public bool inspection_done;

    [TextArea(5, 100)]
    public string overwrite_prompt;

    // use a different prompt for the Builder if it is memoryless
    [TextArea(5, 100)]
    public string overwrite_prompt_memoryless;

    protected override void Awake()
    {
        base.Awake();
        inspection_done = false;
    }


    public string PrepareInspectorInput(string scene_summary, string generated_code)
    {
        string ret = "";
        ret += "Scene: " + scene_summary + '\n';
        ret += "Code: " + generated_code;

        return ret;
    }
    public string PrepareInspectorWithInput(string scene_summary, string user_input, string generated_code)
    {
        string ret = "";
        ret += "Scene: " + scene_summary + '\n';
        ret += "UserInput: " + user_input + '\n';
        ret += "Code: " + generated_code;

        return ret;
    }

    public string ParseInspectionResult(string generated_code, bool builder_is_memoryless)
    {
        // Initialize the output variable
        string suggestion = "";
        string reasoning = "";

        // Split the output, which is the inspection result, by the newline character
        string[] lines = output.Split('\n');

        // Loop through the lines
        foreach (string line in lines)
        {
            // Trim any whitespace from the line
            string trimmed = line.Trim();

            // Check if the line starts with "Task status:"
            if (trimmed.StartsWith("Verdict:"))
            {
                // Get the substring after the colon and trim any whitespace
                string value = trimmed.Substring(trimmed.IndexOf(':') + 1).Trim(' ', '.');

                // Set the status to true if the value is not "failed", otherwise false
                Debug.Log("Verdict: " + value.ToLower());
                inspection_done = !(value.ToLower() == "fail");
            }
            else if (trimmed.StartsWith("Reasoning:"))
            {
                // Get the substring after the colon and trim any whitespace
                reasoning = trimmed.Substring(trimmed.IndexOf(':') + 1).Trim();
            }
            else if (trimmed.StartsWith("Suggestion:"))
            {
                // Get the substring after the colon and trim any whitespace
                suggestion = trimmed.Substring(trimmed.IndexOf(':') + 1).Trim();
            }
        }

        string code_suggestion = "";

        if (builder_is_memoryless)
        {
            code_suggestion = overwrite_prompt_memoryless + "Explanation: " + reasoning + "Suggestion: " + suggestion;
        }
        else
        {
            code_suggestion = overwrite_prompt + "Explanation: " + reasoning + "Suggestion: " + suggestion;
        }
        

        return code_suggestion;
    }


    // used in CompleteAndCompile.cs, legacy
    public string ParseInspectionResult(string generated_code)
    {
        // Initialize the output variable
        string suggestion = "";
        string reasoning = "";

        // Split the output, which is the inspection result, by the newline character
        string[] lines = output.Split('\n');

        // Loop through the lines
        foreach (string line in lines)
        {
            // Trim any whitespace from the line
            string trimmed = line.Trim();

            // Check if the line starts with "Task status:"
            if (trimmed.StartsWith("Verdict:"))
            {
                // Get the substring after the colon and trim any whitespace
                string value = trimmed.Substring(trimmed.IndexOf(':') + 1).Trim(' ', '.');

                // Set the status to true if the value is not "failed", otherwise false
                Debug.Log("Verdict: " + value.ToLower());
                inspection_done = !(value.ToLower() == "fail");
            }
            else if (trimmed.StartsWith("Reasoning:"))
            {
                // Get the substring after the colon and trim any whitespace
                reasoning = trimmed.Substring(trimmed.IndexOf(':') + 1).Trim();
            }
            else if (trimmed.StartsWith("Suggestion:"))
            {
                // Get the substring after the colon and trim any whitespace
                suggestion = trimmed.Substring(trimmed.IndexOf(':') + 1).Trim();
            }
        }

        string code_suggestion = overwrite_prompt + "Explanation: " + reasoning + "Suggestion: " + suggestion;

        return code_suggestion;
    }
}
