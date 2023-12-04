using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text;
using TMPro;
using System.Reflection;
using System;
using Newtonsoft.Json.Linq;

public class SceneParser: MonoBehaviour
{
    public bool replace_space_in_obj_names;
    public List<GameObject> to_ignore;
    public Dictionary<GameObject, bool> ignored;
    public GameObject compiler;
    public ChatBot chatbot;
    public SceneHierarchyCleaner scene_hier_cleaner;
    public DetailedData detailedData;
    //public TextMeshPro refinedInput;

    public bool scene_clean_up;
    //public bool user_request_aware;

    [TextArea(10, 30)]
    public string metaprompt_request_aware;

    [TextArea(10, 30)]
    public string metaprompt_two_stage_output;

    [TextArea(10, 30)]
    public string scene_parsing_compact;

    [TextArea(5, 30)]
    public string scene_parsing;

    public bool tag_background;
    public BackgroundTagger background_tagger;

    //public bool two_stage_output;
    public SceneAnalyzerMode mode;

    public LargeSceneTokenManageMode large_scene_solution = LargeSceneTokenManageMode.None;

    //[TextArea(3, 100)]
    //public string background_obj_description;

    public enum LargeSceneTokenManageMode
    {
        // None: just throw the error that max token size has been exceeded
        // Chunk: divide up the scene representation into chunks that fit in the context length, then stitch the results together to get the final result.
        None, Chunk
    }

    public enum SceneAnalyzerMode
    {
        // legacy: always output summary of the entire scene. Pretty much obselete now.
        // request_aware: output summary of the part of scene that is relevant to user request
        // two_stages: 1st stage - output only the object names relevant to the user request; 2nd stage - add details on the relevant objects.
        legacy, request_aware, two_stages
    }

    public void Awake()
    {
        // use the correct metaprompt depending on the run option
        if (mode == SceneAnalyzerMode.two_stages)
        {
            chatbot.metaprompt = metaprompt_two_stage_output;
        }
        else if (mode == SceneAnalyzerMode.request_aware)
        {
            chatbot.metaprompt = metaprompt_request_aware;
        }

        if (replace_space_in_obj_names)
        {
            ReplaceWhiteSpacesInGONames();
        }
        
    }

    public void Start()
    {
        // record the gameObjects already in the scene
        ConstructIgnoredDict();

    }

    // call this from a button
    public async void AnalyzeScene()
    {
        if (scene_clean_up)
        {
            scene_hier_cleaner.CleanUpSceneHierarchy();
        }
        ParseSceneHierarchy();
        chatbot.input = scene_parsing_compact;
        //chatbot.SendChat();
        await chatbot.SendChat();
    }


    // the main function called for doing the scene analysis
    public async Task AnalyzeSceneAsync(string user_request)
    {
        // remove unnecessary scene hierarchy to make the JSON leaner
        if (scene_clean_up)
        {
            scene_hier_cleaner.CleanUpSceneHierarchy();
        }

        // use vision models to process the background, which does not have a built-in hierarchy.
        if (tag_background && background_tagger != null && background_tagger.needs_to_run)
        {
            await background_tagger.ProcessBackground();
        }

        ParseSceneHierarchy();
        // optionally, condition the summary on user output
        // this helps with complex scenes, in which case the summary only includes the relevant information.
        if (mode == SceneAnalyzerMode.request_aware || mode == SceneAnalyzerMode.two_stages)
        {
            chatbot.input = "Request: " + user_request + '\n';
            chatbot.input += "Scene JSON: " +  scene_parsing_compact;
        }
        else
        {
            chatbot.input = scene_parsing_compact;
        }
        
        // Note: if we have vision models, we should be careful about separating background and foreground objects
        // since the background ones may exist IRL and is thus immutable.
        await chatbot.SendNewChat();

        if (mode == SceneAnalyzerMode.two_stages)
        {
            // the output of the first stage is the GO names for all relevant objects
            // the output of the second stage is the detailed information on the relevant GOs
            chatbot.output = GetSecondStageOutput(); 
        }
    }


    public string GetSecondStageOutput()
    {
        // get the list of names for all relevant gameObjects 
        JObject result_json = JObject.Parse(chatbot.output);
        List<string> GO_names = ConvertJArrayToStrList((JArray)result_json["names"]);

        // TODO: Write the rest of this function. Everything below are just placeholders.
        string ret = "";
        foreach (string GO_name in GO_names)
        {

            ret += GO_name + " ";
            GameObject gameObject = GameObject.Find(GO_name);
            detailedData.OOI.Add(gameObject);
        }

        string output = detailedData.SecondStageParsing();
        return output;
    }

    List<string> ConvertJArrayToStrList(JArray jArray)
    {
        List<string> ret = new List<string>();
        for (int i = 0; i < jArray.Count; i++)
        {
            ret.Add(jArray[i].ToString());
        }

        return ret;
    }

    public void CleanAndParseHierarchy(bool include_compiler)
    {
        // don't ignore compiler if this option is enabled
        if (include_compiler)
        {
            ignored.Remove(compiler);
        }
        scene_hier_cleaner.CleanUpSceneHierarchy();
        ParseSceneHierarchy();
        if (include_compiler)
        {
            ignored[compiler] = true;
        }
    }


    public void ConstructIgnoredDict()
    {
        ignored = new Dictionary<GameObject, bool>();
        foreach (var game_obj in to_ignore)
        {
            ignored[game_obj] = true;
        }
    }
    
    
    // A class to represent a node in the scene hierarchy
    [System.Serializable]
    public class SceneNode
    {
        public string name; // The name of the gameObject
        //public string position;
        //public string scale;
        //public string rotation;
        public List<SceneNode> children; // The list of child nodes
        //private int childCount = 0;
        public string attached_scripts; // **NOTE**: leave this as the last field, otherwise the JSON parsing breaks slightly

        // A constructor that takes a gameObject and recursively creates child nodes
        public SceneNode(GameObject gameObject)
        {
            name = gameObject.name;
            //position = gameObject.transform.localPosition.ToString("F1");
            //scale = gameObject.transform.localScale.ToString("F1");
            //rotation = gameObject.transform.rotation.ToString("F1");

            // script name and summary
            //List<Widgets> widget_scripts = GetWidgetScripts(gameObject);
            //script_descriptions = ParseScriptDescriptions(widget_scripts);
            attached_scripts = GetScriptDescriptions(gameObject);
            // recurse on its children
            children = new List<SceneNode>();
            foreach (Transform child in gameObject.transform)
            {
                //childCount++;
                //if (childCount > 3)
                //    break;
                children.Add(new SceneNode(child.gameObject));
            }
        }

        public string GetScriptDescriptions(GameObject root)
        {
            string ret = "";
            //get all components that is a type of c sharp monoscript
            foreach (UnityEngine.Component s in root.GetComponents(typeof(Widgets)))
            {
                System.Type type = s.GetType();
                Widgets targetScript = root.GetComponent(type) as Widgets;
                string scriptSum = targetScript.summary;
                ret += "Name: " + type.ToString() + ", Summary: " + scriptSum + ". ";


                // Include all the public fields in the summary
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

                // Loop through each field
                ret += "Public fields: ";
                foreach (FieldInfo field in fields)
                {
                    // Get the name and value of the field
                    string name = field.Name;
                    ret += name + ",";
                }
            }

            return ret.Trim(',');
        }
        
        public List<Widgets> GetWidgetScripts(GameObject root)
        {
            // Create an empty list to store the scripts
            List<Widgets> scripts = new List<Widgets>();

            // Get all the components attached
            Component[] components = root.GetComponents<Component>();

            // Loop through each component
            foreach (Component component in components)
            {
                // Try to cast the component to the base class
                Widgets script = component as Widgets;

                // If the cast is successful, add the script to the list
                if (script != null)
                {
                    scripts.Add(script);
                }
            }

            // Return the list of scripts
            return scripts;
        }

        public string ParseScriptDescriptions(List<Widgets> scripts)
        {
            string ret = "Attached scripts: ";

            foreach(Widgets s in scripts)
            {
                ret += "Name: " + s.name + " Summary: " + s.summary;
            }

            return ret;
        }

    }

    // A class to represent the scene data
    [System.Serializable]
    public class SceneData
    {
        public string sceneName; // The name of the scene
        public List<SceneNode> objects; // The list of root nodes

        // A constructor that takes the current scene and creates root nodes
        public SceneData(Scene scene, Dictionary<GameObject, bool> ignored)
        {
            sceneName = scene.name;
            objects = new List<SceneNode>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (!ignored.ContainsKey(root)) // don't parse the ignored objects
                {
                    objects.Add(new SceneNode(root));
                }
                
            }
        }
    }


    public void ParseSceneHierarchy()
    {
        // Get the current scene
        Scene scene = SceneManager.GetActiveScene();

        // Create a scene data object
        SceneData sceneData = new SceneData(scene, ignored);

        // Convert the scene data object to JSON
        scene_parsing = JsonUtility.ToJson(sceneData, true);

        // simplify the JSON file to save tokens
        scene_parsing_compact = RemoveQuotesAndSpacesIgnoringSummary(scene_parsing);
        scene_parsing_compact = RemoveEmptyChildrenAndScripts(scene_parsing_compact);
        scene_parsing_compact = RemoveBracesFromString(scene_parsing_compact);
    }

    public string RemoveEmptyChildrenAndScripts(string json)
    {
        //// Use regular expressions to replace the patterns with an empty string
        //// Use the \b boundary marker to match only whole words
        //// Use the \s* modifier to match any optional whitespace
        //// Use the Regex.Escape method to escape any special characters in the patterns
        //string pattern1 = @"\b" + Regex.Escape(",children:[]") + @"\s*";
        //string pattern2 = @"\b" + Regex.Escape(",attached_scripts:}") + @"\s*";
        //string output = Regex.Replace(json, pattern1, "");
        //output = Regex.Replace(output, pattern2, "}");
        //return output;

        // Use regular expressions to match the patterns and replace them with empty strings or "}"
        string output = Regex.Replace(json, @",children:\[\]", "");
        output = Regex.Replace(output, @",attached_scripts:\}", "}");
        return output;
    }

    public void ReplaceWhiteSpacesInGONames()
    {
        // Get all gameObjects in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        // Loop through each gameObject
        foreach (GameObject obj in allObjects)
        {
            // Get the name of the gameObject
            string name = obj.name;

            // Check if the name contains blank space
            if (name.Contains(" "))
            {
                // Replace the blank space with "_"
                string newName = name.Replace(" ", "_");

                // Rename the gameObject
                obj.name = newName;
            }
        }
    }
    
    public string RemoveWhiteSpaces(string json)
    {
        // Use a regular expression to match and replace any sequence of white space characters
        return Regex.Replace(json, @"\s+", "");
    }

    public string RemoveQuotesAndSpaces(string input)
    {
        // Return a new string with all double quotations and white spaces replaced by empty strings
        input = Regex.Replace(input, @"\s+", "");
        return input.Replace("\"", "");
    }

    public string RemoveQuotesAndSpacesIgnoringSummary(string input)
    {
        //// Find the substring between "Summary: " and "."
        //var match = Regex.Match(input, @"Summary: (.*?)\.");
        //// If there is no match, return the input without any double quotes or white spaces
        //if (!match.Success) return input.Replace("\"", "").Replace(" ", "");
        //// Otherwise, extract the summary and the rest of the input
        //var summary = match.Groups[1].Value;
        //var rest = input.Substring(match.Index + match.Length);
        //Debug.Log("match " + match);
        //Debug.Log("summary " + summary);
        //Debug.Log("rest " + rest);
        //// Return the input without any double quotes or white spaces, except for the summary
        //return input.Substring(0, match.Index) + "Summary: " + summary + "." + rest.Replace("\"", "").Replace(" ", "");


        // Use a regular expression to match the parts between "Summary: " and "."
        //var matches = Regex.Matches(input, @"Summary: .*?\.");

        // Use a regular expression to match the parts between " Summary: " and "}"
        //string pattern = @"( Summary.*?\})";
        string pattern = @"Summary.*\n";
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

    public string RemoveBracesFromString(string input)
    {
        // If the input is null or empty, return it as it is
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // Use a StringBuilder to store the output without braces
        StringBuilder output = new StringBuilder();

        // Loop through each character in the input
        foreach (char c in input)
        {
            // If the character is not a brace, append it to the output
            if (c != '{' && c != '}')
            {
                output.Append(c);
            }
        }

        // Return the output as a string
        return output.ToString();
    }

}