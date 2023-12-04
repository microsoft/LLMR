using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]
public class AnimationHelper : MonoBehaviour
{
    string _current_state;
    Animator _animator;
    //public ModelImporter modelImporter;
    //public Avatar avatar;

    // Start is called before the first frame update
    void Awake()
    {
        _animator = GetComponent<Animator>();
        _current_state = "";
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeAnimationStateTo(string new_state)
    {
        if (new_state == _current_state)
        {
            return;
        }

        _animator.Play(new_state);
        _current_state = new_state;
    }

    public bool IsAnimationPlaying(string state_name)
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName(state_name) && _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            return true;
        }
        return false;
    }

    public string GetCurrentAnimationState()
    {
        return _current_state;
    }

    public RuntimeAnimatorController LoadController(string controller_name)
    {
        if (controller_name == "humanoid")
        {
            return (RuntimeAnimatorController)Resources.Load("Animations/Controllers/humanoidController");
        }
        else
        {
            return null;
        }

    }

    public void ConfigurateAnimation(string model_category)
    {
        if (model_category == "humanoid")
        {
            _animator.runtimeAnimatorController = (RuntimeAnimatorController)Resources.Load("Animations/Controllers/humanoidController");
            // TODO: set animation type to Humanoid
            // TODO: generate avatar

            //GameObject characterModel = this.gameObject;

            //modelImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(characterModel)) as ModelImporter;
            //// Check if the model importer is valid
            //if (modelImporter == null)
            //{
            //    Debug.LogError("Model importer not found for " + characterModel.name);
            //    return;
            //}

            //// Set the animation type to human
            //modelImporter.animationType = ModelImporterAnimationType.Human;

            //// Save and reimport the asset
            //modelImporter.SaveAndReimport();

            //// Generate the avatar from the human description
            //avatar = AvatarBuilder.BuildHumanAvatar(characterModel, modelImporter.humanDescription);

            //// Check if the avatar is valid
            //if (avatar == null)
            //{
            //    Debug.LogError("Avatar not generated for " + characterModel.name);
            //    return;
            //}

            //// Save the avatar asset
            ////AssetDatabase.CreateAsset(avatar, "Assets/Resources/Animations/" + characterModel.name + ".asset");

            //// Assign the avatar to the character model's animator component
            //characterModel.GetComponent<Animator>().avatar = avatar;

            string avatar_name = "Animations/" + gameObject.name + "Avatar";
            //print(avatar_name);
            _animator.avatar = (Avatar)Resources.Load(avatar_name);
            //_animator.avatar = (Avatar)Resources.Load("Animations/Animations/archerAvatar")

        }
        else
        {
            return;
        }

    }

    // automatically
    public void ConfigurateAnimation()
    {
        
    }


}
