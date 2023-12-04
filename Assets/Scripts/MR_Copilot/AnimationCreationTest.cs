using UnityEngine;
using UnityEngine.Animations;

public class AnimationCreationTest : MonoBehaviour
{
    // main components for animation creation
    public AnimationManager animation_manager;
    private AnimationClip clip;
    private Animation anim;
    public AnimationClipFromCSV animation_converter;

    public GameObject obj;
    // should be a children of obj
    public GameObject armature_root;

    [TextArea(10, 30)]
    public string obj_JSON;

    [TextArea(10, 30)]
    public string animation_string;

    void Start()
    {
        // optionally, output the object hierarchy JSON
        if (armature_root != null)
        {
            //GameObject armature_root = GameObject.Find(armature_root_name);
            obj_JSON = animation_manager.GetObjectJSON(armature_root);
            string armature_root_name = animation_manager.GetRelativeNameToRootExceptRoot(armature_root);
            animation_string = animation_manager.PostprocessJointNames(animation_string, armature_root_name);
        }

        // parse animation txt into a clip
        string clip_name = "new_clip";
        clip = animation_converter.GetClipFromTxt(animation_string);
        clip.EnsureQuaternionContinuity();
        anim = obj.AddComponent<Animation>();
        // add clip to the whale animation
        anim.AddClip(clip, clip_name);
        // play it on loop
        AnimationState state = anim[clip_name];
        state.wrapMode = WrapMode.Loop;
        anim.Play(clip_name);
    }
}