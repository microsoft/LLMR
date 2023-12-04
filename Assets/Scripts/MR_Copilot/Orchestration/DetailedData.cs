/* DetailedData.cs
 * Goes through the list of names of the gameobjects and crawl through all the components and their public fields
 * 
 * Cathy Fang
 * Created on: 07/11/2023
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

public class DetailedData : MonoBehaviour
{

    public List<GameObject> OOI;


    [TextArea(30, 100)]
    public string OOIJsonCompact;

    public ChatBot chatbot;

    // Start is called before the first frame update
    void Start()
    {
        //if (OOI != null)
        //{
        //    string OOIJson = "";
        //    foreach (GameObject GO in OOI)
        //    {
        //        OOIJson += ParseGO(GO);
        //    }
        //    OOIJsonCompact = OOIJson;
        //}
    }

    public string SecondStageParsing()
    {
        if (OOI != null)
        {
            string OOIJson = "";
            foreach (GameObject GO in OOI)
            {
                OOIJson += ParseGO(GO);
            }
            OOIJsonCompact = OOIJson;
        }
        return OOIJsonCompact;
    }

    //public async Task AnalyzeSceneAsync(string user_request)
    //{
    //    mockOutput();
    //}

    //public void mockOutput()
    //{
    //    chatbot.output = OOIJsonCompact;
    //}

    public string ParseGO(GameObject InputGO)
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

            output.Append(match.Value.Substring(9, match.Value.Length - 9 - 3));
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
        public string position;
        public string rotation;
        public string scale;
        public string meshSize;
        public List<string> components;

        public GONode(GameObject GO)
        {

            name = GO.name;
            //position = GO.transform.localPosition.ToString("F1");
            //rotation = GO.transform.rotation.ToString("F1");
            //scale = GO.transform.localScale.ToString("F1");
            Bounds meshBounds = GetRenderBounds(GO);
            //meshSize = meshBounds.size.ToString("F1");


            //components = GetComponents(GO);
            children = new List<GONode>();
            foreach (Transform child in GO.transform)
            {
                children.Add(new GONode(child.gameObject));
            }



        }

        private Bounds GetRenderBounds(GameObject obj)
        {
            
            var r = obj.GetComponent<Renderer>();

            var renderers = obj.GetComponentsInChildren<Renderer>();
            if (r == null)
                r = renderers[0];
          
            var bounds = r.bounds;
            foreach (Renderer render in renderers)
            {
                if (render != r)
                    bounds.Encapsulate(render.bounds);
            }
            return bounds;
        }


        public List<string> GetComponents(GameObject GO)
        {
            List<string> components = new List<string>();
            //get all components that is a type of c sharp monoscript
            foreach (UnityEngine.Component s in GO.GetComponents(typeof(UnityEngine.Component)))
            {
                string ret = "";
                System.Type type = s.GetType();
                if (type.ToString().Contains("Transform"))
                {
                    continue;
                }
                else
                {
                    UnityEngine.Component targetScript = GO.GetComponent(type) as UnityEngine.Component;
                    //ret += "ComponentName: " + type.ToString();
                    ret += type.ToString();
                }
                

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
