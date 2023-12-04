/* InputManager.cs
 * Input (and platform?) agnostics plugin
 * 
 * Cathy Fang
 * Created on: 06/30/2023
 * 
 */

using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text;
using System.Runtime.InteropServices;

public class InputManager : MonoBehaviour
{

    public List<GameObject> Hands;
    //public string handsJson = "";
    public List<GameObject> Controller;
    public ChatBot chatbot;
    public bool useRawJson = false;

    [TextArea(30, 100)]
    public string handsJsonCompact;

    // Start is called before the first frame update
    void Start()
    {
        if (Hands != null) {
            string handsJson = "";
            foreach (GameObject Hand in Hands)
            {
                handsJson += ParseInput(Hand);
            }
            handsJsonCompact = handsJson;
            chatbot.input = handsJsonCompact;
        }
    }

    public async Task AnalyzeInput()
    {
        if (chatbot.input != null)
        {
            await chatbot.SendNewChat();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string ParseInput(GameObject InputGO)
    {
        GONode inputNode = new GONode(InputGO);
        string inputJson = JsonUtility.ToJson(inputNode, true);
        string inputJson_Compact = RemoveQuotesAndSpacesIgnoringNames(inputJson);
        //inputJson_Compact = RemoveQuotesAndSpacesIgnoringComponents(inputJson_Compact);
        inputJson_Compact = RemoveEmptyChildrenAndScripts(inputJson_Compact);
        return inputJson_Compact;
    }

    public string RemoveEmptyChildrenAndScripts(string json)
    {
        // Use regular expressions to match the patterns and replace them with empty strings or "}"
        string output = Regex.Replace(json, @",children:\[\]", "");
        output = Regex.Replace(output, @",components:\[\]", "");
        return output;
    }

    public string RemoveQuotesAndSpacesIgnoringNames(string input)
    {
        string pattern = @"""name"": "".*\n"; // double check if components might have spaces in name
        var matches = Regex.Matches(input, pattern);

        // Use a string builder to append the matched parts and remove the quotes, spaces, and new lines from the rest
        var output = new StringBuilder();

        // Keep track of the last index of the matched part
        int lastIndex = 0;

        // Loop through the matches
        foreach (Match match in matches)
        {
            
            // Remove the quotes, spaces, and new lines from the substring before the match
            output.Append(Regex.Replace(input.Substring(lastIndex, match.Index - lastIndex), @"[""\s]", ""));
            output.Append(Regex.Replace(match.Value.Substring(0, 9), @"[""\s]", ""));

            output.Append(match.Value.Substring(9, match.Value.Length-9-3));
            output.Append(",");
            // Append the matched part, remove the new line at the end.
            //output.Append(match.Value.Remove(match.Value.Length - 1, 1));
            //output.Append(match.Value);
           

            // Update the last index
            lastIndex = match.Index + match.Length;
        }

        // Remove the quotes, spaces, and new lines from the remaining substring
        output.Append(Regex.Replace(input.Substring(lastIndex), @"[""\s]", ""));

        // Return the output string
        return output.ToString();
    }

    public string RemoveQuotesAndSpacesIgnoringComponents(string input)
    {
        string pattern = @"components"":\n"; // double check if components might have spaces in name
        var matches = Regex.Matches(input, pattern);

        // Use a string builder to append the matched parts and remove the quotes, spaces, and new lines from the rest
        var output = new StringBuilder();

        // Keep track of the last index of the matched part
        int lastIndex = 0;

        // Loop through the matches
        foreach (Match match in matches)
        {
            //print (match.Value);
            // Remove the quotes, spaces, and new lines from the substring before the match
            output.Append(Regex.Replace(input.Substring(lastIndex, match.Index - lastIndex), @"[""\s]", ""));

            // Append the matched part, remove the new line at the end.
            output.Append(match.Value.Remove(match.Value.Length - 1, 1));
            //output.Append(match.Value);

            // Update the last index
            lastIndex = match.Index + match.Length;
        }

        // Remove the quotes, spaces, and new lines from the remaining substring
        output.Append(Regex.Replace(input.Substring(lastIndex), @"[""\s]", ""));

        // Return the output string
        return output.ToString();
    }



    [System.Serializable]
    public class GONode
    {
        public string name;
        public List<GONode> children;
        public List<string> components;

        public GONode(GameObject InputGO)
        {
            
            name = InputGO.name;
            components = GetComponents(InputGO);
            children = new List<GONode>();
            foreach (Transform child in InputGO.transform)
            {
                children.Add(new GONode(child.gameObject));
            }
            
            

        }

        public List<string> GetComponents(GameObject GO)
        {
            List<string> components = new List<string>();
            //get all components that is a type of c sharp monoscript
            foreach (UnityEngine.Component s in GO.GetComponents(typeof(MonoBehaviour)))
            {
                string ret = "";
                System.Type type = s.GetType();
                MonoBehaviour targetScript = GO.GetComponent(type) as MonoBehaviour;
                //ret += "ComponentName: " + type.ToString();
                ret += type.ToString();

                //ret += ";";
                //// Include all the public fields in the summary
                //FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

                //// Loop through each field
                //ret += "Public fields: ";
                //foreach (FieldInfo field in fields)
                //{
                //    // Get the name and value of the field
                //    string name = field.Name;
                //    ret += name + ",";
                //}
                //ret.Trim(',');
                //ret += ";";
                components.Add(ret);
            }

            return components;

            
        }
    }

}
