using System.IO;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using static System.Net.Mime.MediaTypeNames;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;


// intended usage: attach this script to an empty GO. drag in the clip to parse
// fill in the file dir to save, the relative name of the animation root (for parsing root motion),
// and the number of curves (ex: 4 for rotation)

public class AnimationClipToCSV : MonoBehaviour
{
    public AnimationManager animation_manager;
    public bool remove_useless_data;
    public AnimationClip clip; // The animation clip to parse
    public string fileName; // The name of the .csv file to save
    public GameObject armature_root;
    private string animation_root_name;
    private int num_curves;
    public Dictionary<string, Dictionary<string, List<Vector2>>> clip_dict_pre_concat;
    public Dictionary<string, List<float[]>> clip_dict;
    public Dictionary<string, int> property_idx;
    public Dictionary<string, int> root_motion_property_idx;
    List<float[]> root_motion;
    //public Dictionary<string, bool> parsed_property_names;
    
    void SetUpPropertyIdx()
    {
        property_idx = new Dictionary<string, int>();
        root_motion_property_idx = new Dictionary<string, int>();
        // 0 is reserved for the timestamp of the key
        property_idx["m_LocalRotation.x"] = 1;
        property_idx["m_LocalRotation.y"] = 2;
        property_idx["m_LocalRotation.z"] = 3;
        property_idx["m_LocalRotation.w"] = 4;
        num_curves = property_idx.Count;

        root_motion_property_idx["m_LocalPosition.x"] = 1;
        root_motion_property_idx["m_LocalPosition.y"] = 2;
        root_motion_property_idx["m_LocalPosition.z"] = 3;
    }

#if UNITY_EDITOR
    private void Start()
    {
        //animation_root_name = armature_root.name;
        animation_root_name = animation_manager.GetRelativeNameToRootExceptRoot(armature_root);
        clip_dict_pre_concat = new Dictionary<string, Dictionary<string, List<Vector2>>>();
        clip_dict = new Dictionary<string, List<float[]>>();
        root_motion = new List<float[]>();
        SetUpPropertyIdx();
        // Create a string builder to store the .csv content
        var sb = new StringBuilder();

        AnimationClipCurveData[] cdataarray = AnimationUtility.GetAllCurves(clip, true);
        int l = ((AnimationClipCurveData[])cdataarray).Length;
        for (int x = 0; x < l; x++)
        {
            AnimationClipCurveData cdata = cdataarray[x];

            string joint_name = cdata.path;
            string property_name = cdata.propertyName;
            // root motion
            if (joint_name == animation_root_name && root_motion_property_idx.ContainsKey(property_name))
            {
                ParseRootMotion(cdata);
            }

            // only record properties that we want. Currently: transform rotations
            if (!property_idx.ContainsKey(property_name)) { continue; }

            if (!clip_dict_pre_concat.ContainsKey(joint_name))
            {
                clip_dict_pre_concat[joint_name] = new Dictionary<string, List<Vector2>>();
            }
            clip_dict_pre_concat[joint_name][property_name] = new List<Vector2>();

            Keyframe[] keys = cdata.curve.keys;
            for (int i = 0; i < keys.Length; i++)
            {
                Keyframe kf = keys[i];
                //print("key: " + joint_name + " " + property_name + " " + kf.time + " " + kf.value);
                clip_dict_pre_concat[joint_name][property_name].Add(new Vector2(kf.time, kf.value));
            }
        }

        // build parsed string
        BuildCompactDict();
        string to_write = BuildStringFromClipDict();
        // remove time series that have no changes to save token size
        if (remove_useless_data) { to_write = RemoveIdenticalTimeSeries(to_write); }
        // prepend the root motion string
        to_write = animation_root_name + "," + GetRootMotionStr() + to_write;

        File.WriteAllText(fileName + ".txt", to_write);

    }

    void ParseRootMotion(AnimationClipCurveData cdata)
    {
        if (root_motion_property_idx.ContainsKey(cdata.propertyName))
        {
            Keyframe[] keys = cdata.curve.keys;
            for (int i = 0; i < keys.Length; i++)
            {
                if (root_motion.Count <= i) //initialize the float arrays
                {
                    float[] val = new float[4]; // 1 for time, 3 for position
                    root_motion.Add(val);
                }

                Keyframe kf = keys[i];
                root_motion[i][0] = kf.time;
                root_motion[i][root_motion_property_idx[cdata.propertyName]] = kf.value;
            }
        }
    }
#endif
    string GetRootMotionStr()
    {
        StringBuilder sb = new StringBuilder();

        // Loop through each float[] in the list
        for (int i = 0; i < root_motion.Count; i++)
        {
            // Append the opening bracket of the inner list
            sb.Append("[");

            // Loop through each float in the float[]
            for (int j = 0; j < root_motion[i].Length; j++)
            {
                // Append the float value
                sb.Append(root_motion[i][j].ToString("F2"));

                // Append a comma if it is not the last element
                if (j < root_motion[i].Length - 1)
                {
                    sb.Append(",");
                }
            }

            // Append the closing bracket of the inner list
            sb.Append("]");

            // Append a comma if it is not the last element
            if (i < root_motion.Count - 1)
            {
                sb.Append(",");
            }
        }

        sb.Append('\n');

        return sb.ToString();
    }

    void BuildCompactDict()
    {
        foreach (string obj_name in clip_dict_pre_concat.Keys)
        {
            clip_dict[obj_name] = new List<float[]>();
            Dictionary<string, List<Vector2>> property_curves = clip_dict_pre_concat[obj_name];
            foreach (string property_name in property_curves.Keys)
            {
                List<Vector2> curve_val = property_curves[property_name];

                //print(obj_name + " " + property_name + " " + curve_val.Count);

                // instantiate float arrays 
                if (clip_dict[obj_name].Count != curve_val.Count)
                {
                    for (int i = 0; i < curve_val.Count; i++)
                    {
                        float[] val = new float[num_curves + 1]; // +1 since the first element is the time stamp
                        clip_dict[obj_name].Add(val);
                    }
                }


                for (int j = 0; j < curve_val.Count; j++)
                {
                    float key_time = curve_val[j].x;
                    float key_value = curve_val[j].y;
                    int time_idx = j;
                    int val_idx = property_idx[property_name];
                    clip_dict[obj_name][time_idx][0] = key_time;
                    clip_dict[obj_name][time_idx][val_idx] = key_value;
                }
            }
        }
    }

    string BuildStringFromClipDict()
    {
        var sb = new StringBuilder();
        foreach (var key in clip_dict.Keys)
        {
            //sb.Append(key);
            //print(key + " " + clip_dict[key].Count);
            sb.AppendLine(key + "," + ClipLineToStr(clip_dict[key]));
            //sb.AppendLine();
        }

        return sb.ToString();
    }

    string ClipLineToStr(List<float[]> line)
    {
        var sb = new StringBuilder();
        foreach (float[] curve in line)
        {
            sb.Append("(" + string.Join(",", curve.Select(x => x.ToString("F1")).ToArray()) + "),");
        }
        sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }


    string RemoveIdenticalTimeSeries(string input_str)
    {
        // Split the input into lines
        string[] lines = input_str.Split('\n');

        // A list to store the output lines
        List<string> output = new List<string>();

        // A regular expression to match the vectors
        Regex regex = new Regex(@"\((\d+\.\d+),(-?\d+\.\d+),(-?\d+\.\d+),(-?\d+\.\d+),(-?\d+\.\d+)\)");

        // Loop through the lines
        foreach (string line in lines)
        {
            // Find all the vectors in the line
            MatchCollection matches = regex.Matches(line);

            // If there are no vectors, skip the line
            if (matches.Count == 0) continue;

            // Get the first vector as a reference
            Match first = matches[0];

            // A flag to indicate if the line is useful
            bool useful = false;

            // Loop through the rest of the vectors
            for (int i = 1; i < matches.Count; i++)
            {
                // Get the current vector
                Match current = matches[i];

                // Compare the x, y, z, w components with the reference vector
                // If any of them are different, the line is useful and we can break the loop
                if (current.Groups[2].Value != first.Groups[2].Value ||
                    current.Groups[3].Value != first.Groups[3].Value ||
                    current.Groups[4].Value != first.Groups[4].Value ||
                    current.Groups[5].Value != first.Groups[5].Value)
                {
                    useful = true;
                    break;
                }
            }

            // If the line is useful, add it to the output list
            if (useful) output.Add(line);
        }

        // Join the output list with newlines and print the result
        string result = string.Join("\n", output);
        return result;
    }



    //void Parse()
    //{
    //    sb = new StringBuilder();
    //    AnimationClipCurveData[] cdataarray = AnimationUtility.GetAllCurves(clip, true);
    //    int l = ((AnimationClipCurveData[])cdataarray).Length;
    //    for (int x = 0; x < l; x++)
    //    {
    //        AnimationClipCurveData cdata = cdataarray[x];
    //        //WriteString("property_name", cdata.propertyName); Next();
    //        //WriteString("type", cdata.type.Name); Next();
    //        //BeginArray("keys");
    //        Keyframe[] keys = cdata.curve.keys;
    //        for (int i = 0; i < keys.Length; i++)
    //        {
    //            Keyframe kf = keys[i];
    //            WriteFloat("time", kf.time); Next();
    //            WriteFloat("value", kf.value); Next();
    //        }
    //    }
    //}
}