using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

public class FuzzyModels : ChatBot
{
    [TextArea(5, 20)]
    public string fuzzyModel_metapromptStageOne;

    public string finalFuzzyModel;
    
    protected override void Awake()
    {
        metaprompt = fuzzyModel_metapromptStageOne;
        base.Awake();
    }

    public async Task StageOne(string user_input)
    {
        input = "## user input:" + user_input + "\n";
        await SendChat();
        finalFuzzyModel = output;

    }

    public async Task StageTwo(string user_input)
    {
        input = @"You are now in Stage two, please consider the specificity of the user prompt" + "\n"
                + "and reduce the fuzzy world model list so that the reader can" + "\n"
                + "attend to the most relevant components." + "\n"
                + "user prompt: " + user_input + "\n";

        
        await SendChat();
        finalFuzzyModel = output;
        
    }


}
