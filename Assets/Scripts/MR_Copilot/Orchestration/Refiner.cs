using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Refiner : ChatBot
{

    public string PrepareRefinerInput(string scene_summary, string user_request)
    {
        string ret = "";
        ret += "Scene: " + scene_summary + '\n';
        ret += "Request: " + user_request;

        return ret;
    }

    public string ParseRefinementResult()
    {
        //// Initialize the output variable
        //string instruction = "";

        //// Split the output, which is the inspection result, by the newline character
        //string[] lines = output.Split('\n');

        //// Loop through the lines
        //foreach (string line in lines)
        //{
        //    // Trim any whitespace from the line
        //    string trimmed = line.Trim();

        //    if (trimmed.StartsWith("Instructions:"))
        //    {
        //        // Get the substring after the colon and trim any whitespace
        //        //instruction = trimmed.Substring(trimmed.IndexOf(':') + 1).Trim();
        //        instruction = trimmed;
        //    }
        //}

        //return instruction;

        string instruction = "Instructions: ";
        instruction += GetInstructions(output);

        return instruction.Trim();
    }

    string GetInstructions(string input_str)
    {
        // Check if the input contains "Instructions:"
        if (input_str.Contains("Instructions:"))
        {
            // Find the index of "Instructions:" in the input
            int index = input_str.IndexOf("Instructions:");

            // Return the substring starting from the index of "Instructions: " plus its length
            return input_str.Substring(index + "Instructions:".Length);
        }
        else
        {
            // If the input does not contain "Instructions:", return an empty string or throw an exception
            return "";
            // Alternatively, you could throw an exception, such as:
            // throw new ArgumentException("The input does not contain 'Instructions:'");
        }
    }
}
