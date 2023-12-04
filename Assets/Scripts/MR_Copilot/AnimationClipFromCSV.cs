using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Text;
using static System.Net.Mime.MediaTypeNames;


//[RequireComponent(typeof(Animation))]
public class AnimationClipFromCSV : MonoBehaviour
{
    
    AnimationClip animationClip;
    public List<List<string>> data;
    public List<LineData> fileData;
    private Regex fieldPattern = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)");
    private Dictionary<string, string> field_mapping = new Dictionary<string, string>();
    public List<string> curve_names;
    public List<string> root_motion_curve_names;
    private int num_curves_on_single_joint;
    private int num_curves_root_motion;
    
    // legacy
    private int num_keys = 10; //number of keys to keep *in addition to* the initial key
    private int num_meta_columns = 3;
    private string inputPath = "C:\\Bill\\temp/whale_excited2.txt";
    private string outputPath;
    private GameObject joint_hierarchy_root;
    [TextArea(10, 30)]
    private string object_JSON;
    private Animation anim;
    private string clip_name;

    //public GameObject obj;
    //public GameObject root_obj;

    void Awake()
    {
        num_curves_on_single_joint = curve_names.Count;
        num_curves_root_motion = root_motion_curve_names.Count;

        //anim = GetComponent<Animation>();
        //// define animation curve
        //animationClip = new AnimationClip();
        //// set animation clip to be legacy
        //animationClip.legacy = true;
        //ConfigurateAnimationCurvesTxt();
        //// post-processing for visual quality
        //animationClip.EnsureQuaternionContinuity();
        //// add clip to the animator
        //anim.AddClip(animationClip, clip_name);
        //// play it on loop
        //AnimationState state = anim[clip_name];
        //state.wrapMode = WrapMode.Loop;
        //anim.Play(clip_name);

    }

    List<LineData> ParseTxtFileToData(string animation_txt)
    {
        List<LineData> data = new List<LineData>();

        string[] lines = animation_txt.Split('\n');
        // the first line is root motion
        string root_motion_line = lines[0];
        data.Add(ParseRootMotionData(RemoveWhiteSpaces(root_motion_line)));
        // other animation curves
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            //data.Add(ParseString(line));
            data.Add(ParseString(RemoveWhiteSpaces(line)));
        }

        return data;
    }

    public AnimationClip GetClipFromTxt(string animation_txt)
    {
        AnimationClip clip = new AnimationClip();
        clip.legacy = true;

        List<LineData> animation_data = ParseTxtFileToData(animation_txt);

        //// the first data is root motion
        //LineData root_motion_data = animation_data[0];
        //List<AnimationCurve> root_motion_curves = new List<AnimationCurve>();
        //string root_name = root_motion_data.name;
        //List<float[]> root_values = root_motion_data.values;

        //for (int k = 0; k < num_curves_root_motion; k++)
        //{
        //    AnimationCurve new_curve = new AnimationCurve();
        //    root_motion_curves.Add(new_curve);
        //}

        //for (int j = 0; j < root_values.Count; j++) // loop over keys at different times
        //{
        //    float[] key_values = root_values[j];
        //    for (int k = 0; k < num_curves_root_motion; k++) // loop over property curves
        //    {
        //        float key_time = key_values[0];
        //        float key_value = key_values[k + 1];
        //        root_motion_curves[k].AddKey(key_time, key_value);
        //        //print(root_name);
        //        //print(key_time);
        //        //print(key_value);
        //    }
        //}
        //for (int k = 0; k < num_curves_root_motion; k++)
        //{
        //    clip.SetCurve(root_name, typeof(Transform), root_motion_curve_names[k], root_motion_curves[k]);
        //}

        // other animation curves
        for (int i = 0; i < animation_data.Count; i++) // loop through gameObjects (different joints)
        {
            LineData row = animation_data[i];
            string obj_name = row.name.Trim();
            List<float[]> values = row.values;
            // the same GO has many animation curves
            List<AnimationCurve> ani_curves = new List<AnimationCurve>();
            int num_curves = (i == 0 ? num_curves_root_motion : num_curves_on_single_joint);
            List<string> property_curve_names = (i == 0 ? root_motion_curve_names : curve_names);

            for (int k = 0; k < num_curves; k++) // create all animation curves for this GO (ex: rotation in x,y,z)
            {
                AnimationCurve new_curve = new AnimationCurve();
                ani_curves.Add(new_curve);
            }

            for (int j = 0; j < values.Count; j++) // loop over keys at different times
            {
                float[] key_values = values[j];
                for (int k = 0; k < num_curves; k++) // loop over property curves
                {
                    //print(key_values.Length);
                    float key_time = key_values[0];
                    float key_value = key_values[k + 1];
                    ani_curves[k].AddKey(key_time, key_value);
                }
            }

            for (int k = 0; k < num_curves; k++) // create all animation curves for this GO (ex: rotation in x,y,z)
            {
                clip.SetCurve(obj_name, typeof(Transform), property_curve_names[k], ani_curves[k]); // first entry is GO name
            }
        }

        return clip;
    }

    //// A class to represent a node in the scene hierarchy
    //[System.Serializable]
    //public class ObjectNode
    //{
    //    public string name; // The name of the gameObject
    //    public string position;
    //    //public string scale;
    //    //public string rotation;
    //    public List<ObjectNode> children; // The list of child nodes

    //    // A constructor that takes a gameObject and recursively creates child nodes
    //    public ObjectNode(GameObject gameObject)
    //    {
    //        name = gameObject.name;
    //        position = gameObject.transform.localPosition.ToString("F4");
    //        //scale = gameObject.transform.localScale.ToString("F1");
    //        //rotation = gameObject.transform.rotation.ToString("F1");

    //        // recurse on its children
    //        children = new List<ObjectNode>();
    //        foreach (Transform child in gameObject.transform)
    //        {
    //            children.Add(new ObjectNode(child.gameObject));
    //        }
    //    }
    //}

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

    string GetRelativeGOName(GameObject obj, GameObject root_obj)
    {
        string name = "";
        while (obj.transform.parent != null && obj != root_obj) 
        {
            name = obj.name + "/" + name;
            obj = obj.transform.parent.gameObject;
        }

        // remove the last "/"
        name = name.Remove(name.Length-1, 1);

        return name;
    }

    void ProcessCSVFiles()
    {
        char separator = ',';

        string[] keepNames = { "Name", "ParentName", "HierarchyIndex" };
        // The substring to check for rotation columns
        string rotationSubstring = "Rotation";
        // Read the input file as an array of lines
        string[] lines = File.ReadAllLines(inputPath);

        // Assume the first line is the header and split it by the separator
        string[] header = lines[0].Split(separator);
        // trim names with white spaces
        header = header.Select(p => p.Trim()).ToArray();

        // Find the indices of the columns to keep by checking the names and the substring
        int[] keepIndices = header.Select((name, index) => new { name, index })
            .Where(x => keepNames.Contains(x.name) || x.name.Contains(rotationSubstring))
            .Select(x => x.index).ToArray();

        // downsample the frames
        //int startIdx = GetFirstSubstringIdx(header, rotationSubstring);
        //int endIdx = GetEndSubstringIdx(header, rotationSubstring);
        keepIndices = DownsampleFrames(keepIndices);

        // Create a new array of lines for the output file
        string[] outputLines = new string[lines.Length];

        // For each line, keep only the values at the keep indices and join them by the separator
        for (int i = 0; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(separator);
            string[] outputValues = keepIndices.Select(index => values[index]).ToArray();
            outputLines[i] = string.Join(separator.ToString(), outputValues);
        }

        // Write the output lines to the output file
        File.WriteAllLines(outputPath, outputLines);
    }

    int[] DownsampleFrames(int[] indices)
    {
        //// Calculate the interval to skip rotations
        //int interval = indices.Length / num_curves_on_single_joint / num_keys;

        //// Create a list to store the selected rotations
        //List<int> selected = new List<int>();

        //// Loop through the indices and add every interval-th rotation to the list
        //for (int i = num_meta_columns; i < indices.Length; i += num_curves_on_single_joint)
        //{
        //    if ((i-num_meta_columns) % (interval * num_curves_on_single_joint) == 0)
        //    {
        //        for (int j = 0; j < num_curves_on_single_joint; j++)
        //        {
        //            selected.Add(indices[i + j]);
        //        }
        //    }
        //}

        //return selected.ToArray();

        // Copy the first four elements (name, parent name, hier, and first rotation)
        List<int> idx_list = new List<int>(indices);
        List<int> output = new List<int>(idx_list.GetRange(0, num_meta_columns+1));

        // Calculate the interval between rotations to keep
        int interval = (idx_list.Count - num_meta_columns-1) / (num_curves_on_single_joint * num_keys);

        // Loop through the remaining rotations and add them to the output if they match the interval
        for (int i = num_meta_columns+1; i < idx_list.Count; i += num_curves_on_single_joint)
        {
            if ((i - num_meta_columns-1) % (interval * num_curves_on_single_joint) == 0)
            {
                output.AddRange(idx_list.GetRange(i, num_curves_on_single_joint-1));
            }
        }

        return output.ToArray();
    }

    int GetFirstSubstringIdx(string[] str_list, string sub_string)
    {
        for (int i = 0; i < str_list.Length; i++)
        {
            string s = str_list[i]; 
            // Check if the current string contains "Rotation" as a substring
            if (s.Contains(sub_string))
            {
                // If yes, assign it to the result variable and break the loop
                return i;
            }
        }
        return -1;
    }

    void ConfigurateAnimationCurvesTxt()
    {
        ParseTxtFile();
        
        for (int i = 0; i < fileData.Count; i++) // loop through gameObjects (different joints)
        {
            LineData row = fileData[i];
            string obj_name = row.name;
            List<float[]> values = row.values;

            // the same GO has many animation curves
            List<AnimationCurve> ani_curves = new List<AnimationCurve>();
            for (int k = 0; k < num_curves_on_single_joint; k++) // create all animation curves for this GO (ex: rotation in x,y,z)
            {
                AnimationCurve new_curve = new AnimationCurve();
                ani_curves.Add(new_curve);
            }

            for (int j = 0; j < values.Count; j++) // loop over keys at different times
            { 
                float[] key_values = values[j];
                for (int k = 0; k < num_curves_on_single_joint; k++) // loop over property curves
                {
                    
                    float key_time = key_values[0];
                    float key_value = key_values[k+1];
                    ani_curves[k].AddKey(key_time, key_value);
                }
            }

            for (int k = 0; k < num_curves_on_single_joint; k++) // create all animation curves for this GO (ex: rotation in x,y,z)
            {
                //animationClip.SetCurve("", typeof(Transform), row[0], ani_curves[k]); // first entry is GO name
                animationClip.SetCurve(obj_name, typeof(Transform), curve_names[k], ani_curves[k]); // first entry is GO name
            }
        }
    }

    void ConfigurateAnimationCurves()
    {
        ParseCSV(inputPath);

        // loop through gameObjects (different joints)
        for (int i = 0; i < data.Count; i++) 
        {
            List<string> row = data[i];

            // the same GO has many animation curves
            //Dictionary<string, AnimationCurve> ani_curves = new Dictionary<string, AnimationCurve>();
            List<AnimationCurve> ani_curves = new List<AnimationCurve>();
            for (int k = 0; k < num_curves_on_single_joint; k++) // create all animation curves for this GO (ex: rotation in x,y,z)
            {
                AnimationCurve new_curve = new AnimationCurve();
                ani_curves.Add(new_curve);
            }

            // loop through different frames for the same GO
            // skip the first few columns, which are metadata
            int j = num_meta_columns;
            while (j < row.Count - num_curves_on_single_joint)
            {
                // assume the first entry in each block is the time
                float time_stamp = 0f;
                float.TryParse(row[j], out time_stamp);

                for (int k = 0; k < num_curves_on_single_joint; k++)
                {
                    float animation_key_value = 0f;
                    float.TryParse(row[j + k], out animation_key_value);

                    ani_curves[k].AddKey(time_stamp, animation_key_value);
                }

                j += num_curves_on_single_joint;
            }

            for (int k = 0; k < num_curves_on_single_joint; k++) // create all animation curves for this GO (ex: rotation in x,y,z)
            {
                //animationClip.SetCurve("", typeof(Transform), row[0], ani_curves[k]); // first entry is GO name
                animationClip.SetCurve(row[0], typeof(Transform), curve_names[k], ani_curves[k]); // first entry is GO name
            }
        }
    }

    void ConfigurateAnimationCurvesExample()
    {
        //AnimationCurve translateX = AnimationCurve.Linear(0.0f, 0.0f, 2.0f, 2.0f);
        AnimationCurve translateX = new AnimationCurve();
        translateX.AddKey(0f, 0f);
        translateX.AddKey(2f, 2f);
        translateX.AddKey(3f, -2f);
        animationClip.SetCurve("", typeof(Transform), "localPosition.x", translateX);
        
    }
    
    public string RemoveWhiteSpaces(string json)
    {
        // Use a regular expression to match and replace any sequence of white space characters
        return Regex.Replace(json, @"\s+", "");
    }


    public class LineData
    {
        public string name; // The first element
        public List<float[]> values; // The rest of the elements as float arrays

        public LineData(string name, List<float[]> values)
        {
            this.name = name;
            this.values = values;
        }
    }

    void ParseTxtFile()
    {
        fileData = new List<LineData>();
        using (StreamReader reader = new StreamReader(inputPath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // Split the line by commas
                fileData.Add(ParseString(line));
            }
        }
    }

    // A method to parse a single string
    public LineData ParseString(string input)
    {
        // Split the string by the first (, assuming it is the separator between the name and the values
        string[] split = input.Split(new char[] { '(' }, 2);

        // If the split length is not 2, the input is invalid
        if (split.Length != 2)
        {
            Debug.LogError("Invalid input: " + input);
            return null;
        }

        // Store the name as the first element of the split
        string name = split[0].TrimEnd(',');

        // Trim the trailing ) from the second element of the split
        char[] charsToTrim = { ')', ' ', '\n' };
        string valuesString = split[1].Trim().TrimEnd(charsToTrim);

        //foreach(char c in valuesString.ToCharArray()) { print(c); }

        // Split the values string by ),(, assuming they are the separators between the value groups
        string[] valueGroups = valuesString.Split(new string[] { "),(" }, StringSplitOptions.None);

        // Create a list to store the float arrays
        List<float[]> values = new List<float[]>();

        // Loop through each value group
        foreach (string valueGroup in valueGroups)
        {
            // Split the value group by ,, assuming they are the separators between the float values
            string[] valueStrings = valueGroup.Split(',');

            // Create a float array to store the parsed values
            float[] valueArray = new float[valueStrings.Length];

            // Loop through each value string
            for (int i = 0; i < valueStrings.Length; i++)
            {
                // Try to parse the value string as a float and store it in the array
                // If the parsing fails, log an error and return null
                if (!float.TryParse(valueStrings[i], out valueArray[i]))
                {
                    Debug.LogError("Invalid value: " + valueStrings[i]);
                    return null;
                }
            }

            //foreach(var v in valueArray) { print(v); }

            // Add the float array to the list
            values.Add(valueArray);
        }

        // Create and return a ParsedData object with the name and the values
        return new LineData(name, values);
    }

    // A method to parse multiple strings and store them in the list
    public void ParseStrings(string[] inputs)
    {
        // Clear the list
        fileData.Clear();

        // Loop through each input string
        foreach (string input in inputs)
        {
            // Parse the string and add the result to the list
            LineData parsedData = ParseString(input);
            if (parsedData != null)
            {
                fileData.Add(parsedData);
            }
        }
    }

    public LineData ParseRootMotionData(string input)
    {
        // Split the string by the first (, assuming it is the separator between the name and the values
        string[] split = input.Split(new char[] { '[' }, 2);

        // If the split length is not 2, the input is invalid
        if (split.Length != 2)
        {
            Debug.LogError("Invalid input: " + input);
            return null;
        }

        // Store the name as the first element of the split
        string name = split[0].TrimEnd(',');

        // Trim the trailing ) from the second element of the split
        char[] charsToTrim = { ']', ' ', '\n' };
        string valuesString = split[1].Trim().TrimEnd(charsToTrim);

        //foreach(char c in valuesString.ToCharArray()) { print(c); }

        // Split the values string by ),(, assuming they are the separators between the value groups
        string[] valueGroups = valuesString.Split(new string[] { "],[" }, StringSplitOptions.None);

        // Create a list to store the float arrays
        List<float[]> values = new List<float[]>();

        // Loop through each value group
        foreach (string valueGroup in valueGroups)
        {
            // Split the value group by ,, assuming they are the separators between the float values
            string[] valueStrings = valueGroup.Split(',');

            // Create a float array to store the parsed values
            float[] valueArray = new float[valueStrings.Length];

            // Loop through each value string
            for (int i = 0; i < valueStrings.Length; i++)
            {
                // Try to parse the value string as a float and store it in the array
                // If the parsing fails, log an error and return null
                if (!float.TryParse(valueStrings[i], out valueArray[i]))
                {
                    Debug.LogError("Invalid value: " + valueStrings[i]);
                    return null;
                }
            }

            //foreach(var v in valueArray) { print(v); }

            // Add the float array to the list
            values.Add(valueArray);
        }

        // Create and return a ParsedData object with the name and the values
        return new LineData(name, values);
    }

    void ParseTxt()
    {
        // Initialize the file data list
        fileData = new List<LineData>();

        // Read the text file line by line
        using (StreamReader reader = new StreamReader(inputPath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // Split the line by commas
                string[] elements = line.Split(',');

                // The first element is the name
                string name = elements[0];

                // The rest of the elements are float arrays
                List<float[]> values = new List<float[]>();
                for (int i = 1; i < elements.Length; i++)
                {
                    // Remove the parentheses and split by spaces
                    string[] numbers = elements[i].Trim('(', ')').Split(' ');

                    //print(string.Join(", ", numbers));

                    // Parse the numbers as floats and store them in an array
                    float[] value = new float[numbers.Length];
                    for (int j = 0; j < numbers.Length; j++)
                    {
                        value[j] = float.Parse(numbers[j]);
                    }

                    // Add the array to the values list
                    values.Add(value);
                }

                // Create a new line data object and add it to the file data list
                LineData lineData = new LineData(name, values);
                fileData.Add(lineData);
            }
        }

        //// Print the file data for testing
        //foreach (LineData lineData in fileData)
        //{
        //    Debug.Log("Name: " + lineData.name);
        //    Debug.Log("Values: ");
        //    foreach (float[] value in lineData.values)
        //    {
        //        Debug.Log(string.Join(", ", value));
        //    }
        //}
    }

    // A method to load and parse a CSV file from a given path
    public void ParseCSV(string path)
    {
        // Initialize the data list
        data = new List<List<string>>();

        // Try to open the file with a stream reader
        try
        {
            using (StreamReader reader = new StreamReader(path))
            {
                // Read each line until the end of the file
                while (!reader.EndOfStream)
                {
                    // Get the current line
                    string line = reader.ReadLine();

                    // Initialize a list to store the fields
                    List<string> fields = new List<string>();

                    // Match the fields with the regular expression
                    MatchCollection matches = fieldPattern.Matches(line);

                    // Loop through the matches
                    foreach (Match match in matches)
                    {
                        // Get the field value
                        string field = match.Value;

                        // Trim the leading and trailing commas
                        field = field.TrimStart(',').TrimEnd(',');

                        // Trim the leading and trailing quotes
                        field = field.TrimStart('"').TrimEnd('"');

                        // Replace any escaped quotes with single quotes
                        field = field.Replace("\"\"", "\"");

                        // Add the field to the list
                        fields.Add(field);
                    }

                    // Add the list of fields to the data list
                    data.Add(fields);
                }
            }
        }
        // Catch any exceptions and log them
        catch (Exception e)
        {
            Debug.LogError("Error loading CSV file: " + e.Message);
        }
    }
}