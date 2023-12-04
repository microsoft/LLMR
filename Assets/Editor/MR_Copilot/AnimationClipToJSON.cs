using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text;
using System.IO;

public class AnimationClipToJSON
{
    private static StringBuilder sb;
    private static string folder;
    private static string jsonFileExt = ".json";
    private static float progress = 0.0f;

    [MenuItem("Animation/Parse Clip(s)")]
    public static void ParseAnimation()
    {
        if (Selection.gameObjects.Length < 1)
        {
            EditorUtility.DisplayDialog("Animation Reader", "Select a game object which has 'Animation' component with " +
                "Animation Clips in it to parse the clips.", "Back");
            return;
        }
        Animation animation = Selection.activeGameObject.GetComponent<Animation>();
        if (animation == null)
        {
            EditorUtility.DisplayDialog("Animation Reader", "Selected object (" + Selection.activeGameObject.name + ") doesn't have \"Animation\" component.", "Back");
            return;
        }
        AnimationClip[] clips = AnimationUtility.GetAnimationClips(animation);
        if (clips.Length > 0)
        {
            folder = EditorUtility.SaveFolderPanel("Save Clip", "", animation.gameObject.name);
            //foreach (AnimationClip clip in clips) {
            for (int i = 0; i < clips.Length; i++)
            {
                AnimationClip clip = clips[i];
                progress = ((float)clips.Length) / (float)i;
                EditorUtility.DisplayProgressBar("Animation Reader", clip.name, progress);
                ParseClip(clip);
            }
            EditorUtility.ClearProgressBar();
        }
    }

    private static void ParseClip(AnimationClip clip)
    {
        sb = new StringBuilder();
        BeginObject();
        WriteString("clipname", clip.name); Next();
        WriteString("wrapmode", clip.wrapMode.ToString()); Next();
        BeginArray("curves");

        AnimationClipCurveData[] cdataarray = AnimationUtility.GetAllCurves(clip, true);
        int l = ((AnimationClipCurveData[])cdataarray).Length;
        for (int x = 0; x < l; x++)
        {
            AnimationClipCurveData cdata = cdataarray[x];
            BeginObject();
            WriteString("property_name", cdata.propertyName); Next();
            WriteString("type", cdata.type.Name); Next();
            BeginArray("keys");
            Keyframe[] keys = cdata.curve.keys;
            for (int i = 0; i < keys.Length; i++)
            {
                Keyframe kf = keys[i];
                BeginObject();
                WriteFloat("time", kf.time); Next();
                WriteFloat("value", kf.value); Next();
                WriteFloat("intangent", kf.inTangent); Next();
                WriteFloat("outtangent", kf.outTangent);
                EndObject();
                if (i < keys.Length - 1)
                    Next();
            }
            EndArray();
            EndObject();
            if (x < l - 1)
                Next();
        }
        EndArray();
        EndObject();
        if (folder.Length > 0)
        {
            string filename = folder + "/" + clip.name + jsonFileExt;
            FileStream f = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(f);
            sw.Write(sb.ToString());
            sw.Close();
            f.Close();
        }
    }
    private static void BeginObject() { sb.Append("{ "); }
    private static void EndObject() { sb.Append(" }"); }
    private static void BeginArray(string keyname) { sb.AppendFormat("\"{0}\" : [", keyname); }
    private static void EndArray() { sb.Append(" ]"); }
    private static void WriteString(string key, string value) { sb.AppendFormat("\"{0}\" : \"{1}\"", key, value); }
    private static void WriteFloat(string key, float val) { sb.AppendFormat("\"{0}\" : {1}", key, val); }
    private static void Next() { sb.Append(", "); }
}