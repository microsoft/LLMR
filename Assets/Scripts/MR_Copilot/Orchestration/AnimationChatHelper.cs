using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


// this is basically a memoryless GPT supporting a couple of animation related methods
public class AnimationChatHelper : ChatBot
{
    [TextArea(10, 30)]
    public string metaprompt_finding_armature_root;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async Task<GameObject> GetArmatureRoot(GameObject model_root, string object_JSON)
    {
        metaprompt = metaprompt_finding_armature_root;
        input = object_JSON;
        // assume memoryless is the way to go
        // this also refreshes the metaprompt for the chat system, which we've set above
        await SendNewChat();
        Transform arm_root = model_root.transform.Find(output);
        //Transform arm_root = model_root.transform.Find("Armature");

        if (arm_root == null)
        {
            return model_root;
        }
        return arm_root.gameObject;
    }

    public async Task<string> GetObjectNameToAnimate(GameObject model, string model_JSON, string animation_description)
    {
        return model.name;
        // TODO: take in animation description + model JSON, then send a chat to determine the model name
    }



}
