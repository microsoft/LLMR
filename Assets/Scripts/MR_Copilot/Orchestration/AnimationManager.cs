using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class AnimationManager : ChatBot
{
    public bool use_CoT;
    public bool use_fuzzy_model;
    public AnimationClipFromCSV animation_converter;
    public AnimationChatHelper animation_chat_helper;

    public string input_intro;
    public string input_;
    public string metaprompt_examples;

    public bool ensure_rotation_continuity;
    

    public string GetObjectFrame(GameObject model)
    {
        // expose the root frame so that the root motion can be animated correctly
        string ret = "Object root forward direction: " + model.transform.forward + "; right direction: " + model.transform.right
            + "; up direction: " + model.transform.up + '\n';

        return ret;
    }

    public string GetObjectJSON(GameObject armature_root)
    {
        ObjectNode json_node = new ObjectNode(armature_root);
        string object_JSON = JsonUtility.ToJson(json_node, true);
        object_JSON = RemoveQuotesAndSpacesIgnoringSummary(object_JSON);
        object_JSON = RemoveEmptyChildren(object_JSON);
        object_JSON = RemoveBracesFromString(object_JSON);

        //// expose the root frame so that the root motion can be animated correctly
        ////object_JSON += '\n' + "Object root forward direction: " + armature_root.transform.forward + "; right direction: " + armature_root.transform.right
        ////    + "; up direction: " + armature_root.transform.up;
        //object_JSON += '\n' + "Object root forward direction: " + model.transform.forward + "; right direction: " + armature_root.transform.right
        //    + "; up direction: " + model.transform.up;

        return object_JSON + '\n';
    }

    string GetObjectIntro(string model_name)
    {
        string intro_str = $"The object you will animate is: **{model_name}**.";

        //optionally, add a short description about the kinematics of the object to animate

        // transition to the object JSON part
        //intro_str += "The object has the following JSON hierarchy in Unity for its armature, " +
        //    "which contains all movable joints and their spatial relations, where a child's position is relative to its parent's: \n";
        intro_str += '\n' + "Object JSON: \n";

        return intro_str;
    }

    string GetExamplesForMetaprompt(GameObject model)
    {
        string examples = "";
        // Ideally, this methods extracts all animation clips from the model, parse them into strings, then use these as part of the metaprompt.
        // The technical issue is that to my knowledge, Unity does not support runtime access of animation clips.
        // so currently, you have to fill in the examples by hand in the public field...
        examples = metaprompt_examples;
        return examples;
    }

    void BuildChatInput(GameObject model, GameObject armature_root, string animation_description, string model_name)
    {
        input = GetObjectIntro(model_name);
        input += GetObjectJSON(armature_root);
        input += GetObjectFrame(armature_root);

        // finds existing animations on the model, if any, to use as demonstrations
        //input += GetExamplesForMetaprompt();
        input += "Instruction: Create the animation for " + animation_description;
    }

    public async Task GenerateAnimationTxt(GameObject model, string animation_description)
    {
        // parses the object JSON from the actual object root (not armature root)
        // use this JSON to determine what the armature root is.
        string model_JSON = GetObjectJSON(model);
        GameObject armature_root = await animation_chat_helper.GetArmatureRoot(model, model_JSON);
        string armature_root_name = animation_chat_helper.output;
        // the name of the object to animate is also stored in the output
        // which may different from model.name if the model is poorly named in the hierarchy.
        string model_name = await animation_chat_helper.GetObjectNameToAnimate(model, model_JSON, animation_description);

        // prepare the input necessary for animation generation
        BuildChatInput(model, armature_root, animation_description, model_name);

        await SendNewChat();

        // postprocess the joint names to be correct
        output = PostprocessJointNames(armature_root_name);
    }

    public async Task<AnimationClip> CreateAnimation(GameObject model, string animation_description)
    {
        await GenerateAnimationTxt(model, animation_description); // afterwards, the animation txt is stored in output
        AnimationClip clip = animation_converter.GetClipFromTxt(output);

        if (ensure_rotation_continuity)
        {
            clip.EnsureQuaternionContinuity();
        }

        return clip;
    }

    string PostprocessJointNames(string armature_root_name)
    {
        armature_root_name = RemoveLastStringAfterSlash(armature_root_name);
        string[] lines = output.Split('\n');
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            sb.Append(armature_root_name + line + '\n');
        }
        return sb.ToString().Trim();
    }

    public string PostprocessJointNames(string animation_txt, string armature_root_name)
    {
        armature_root_name = RemoveLastStringAfterSlash(armature_root_name);
        string[] lines = animation_txt.Split('\n');
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            sb.Append(armature_root_name + line + '\n');
        }
        return sb.ToString().Trim();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public string GetRelativeNameToRootExceptRoot(GameObject obj)
    {
        // Start with the gameobject's name
        string relativeName = obj.name;

        // Loop through the gameobject's ancestors until reaching the root
        Transform current = obj.transform.parent;
        while (current != null && current.transform.parent != null)
        {
            // Prepend the ancestor's name and a slash to the relative name
            relativeName = current.name + "/" + relativeName;

            // Move to the next ancestor
            current = current.parent;
        }

        // Return the relative name
        return relativeName;
    }

    //public async Task<GameObject> GetArmatureRoot(string object_JSON)
    //{
    //    metaprompt = metaprompt_finding_armature_root;
    //    // assume memoryless is the way to go
    //    await SendNewChat();
    //    return Transform.Find(output).gameObject;
    //}

    //public async Task<string> GetObjectNameToAnimate(GameObject model, string model_JSON, string animation_description)
    //{
    //    return model.name;
    //    // TODO: take in animation description + model JSON, then send a chat to determine the model name
    //}

    // A class to represent a node in the scene hierarchy
    [System.Serializable]
    public class ObjectNode
    {
        public string name; // The name of the gameObject
        public string position;
        //public string forward_direction;
        //public string right_direction;
        //public string up_direction;
        //public string scale;
        public string rotation;
        public List<ObjectNode> children; // The list of child nodes

        // A constructor that takes a gameObject and recursively creates child nodes
        public ObjectNode(GameObject gameObject)
        {
            name = gameObject.name;
            position = gameObject.transform.localPosition.ToString("F2");
            //forward_direction = gameObject.transform.forward.ToString("F1");
            //right_direction = gameObject.transform.right.ToString("F1");
            //up_direction = gameObject.transform.up.ToString("F1");
            //scale = gameObject.transform.localScale.ToString("F1");
            rotation = gameObject.transform.localRotation.ToString("F1");

            // recurse on its children
            children = new List<ObjectNode>();
            foreach (Transform child in gameObject.transform)
            {
                children.Add(new ObjectNode(child.gameObject));
            }
        }
    }

    public string RemoveQuotesAndSpacesIgnoringSummary(string input)
    {
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

    public string RemoveEmptyChildren(string json)
    {
        // Use regular expressions to match the patterns and replace them with empty strings or "}"
        string output = Regex.Replace(json, @",children:\[\]", "");
        return output;
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

    public string RemoveLastStringAfterSlash(string s)
    {
        // Find the index of the last "/"
        int lastIndex = s.LastIndexOf("/");

        // If the index is valid, return the substring from the start to the index (inclusive)
        if (lastIndex >= 0)
        {
            return s.Substring(0, lastIndex + 1);
        }
        // Otherwise, return an empty string
        else
        {
            return "";
        }
    }
}
