using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using TMPro;
using System;
using NUnit.Framework;

public class ChatCompilationManagerModular : MonoBehaviour
{

    public Builder builder;
    public GameObject roslynCompiler;
    public Inspector inspector;
    public DebuggerGPT debugger;
    public APIManager skillLibrary;
    public Planner planner;
    public SceneParser sceneAnalyzer;
    public TextMeshPro refinedInput;
    private bool refined;
    public List<string> error_messages = new List<string>();
    //public List<string> error_messages_for_exp = new List<string>(); //used for experiments only
    public MetaPrompts listMetaPrompts;
    public FuzzyModels fuzzyModels;
    public TMP_InputField output_window;

    public bool use_inspector;
    public bool use_skillLibrary;
    //public bool use_planner;
    public bool use_sceneAnalyzer;
    public bool use_debugger;
    public bool use_fuzzyModels;
    public bool use_demonstrations;
    public bool use_FeedBackLoop;


    private string sceneForInput = "";
    private string libraryForInput = "";
    private string fuzzyModelsForInput = "";
    private string firstTransformRelevantObject = "";

    private int modularCallCount = 0;

    // this is where all the feedback scripts will be (added one by one)]

    // public MovementFeedback movementFeedback; 

    void Start()
    {
        SetMetaprompts(); // these are configurated during Awake() in MetaPrompts.cs
        Application.logMessageReceived += OnLogMessageReceived;
        
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ClearRecordedErrorMessages();
            modularCallCount++;
            RunFramework();
        }

        if (Input.GetKeyDown(KeyCode.RightAlt))
        {
            Chat_with_Planner();
        }
    }

    async void Chat_with_Planner()
    {
        await planner.ConverseWithUser(planner.input_TMP.text);
    }

    async void RunFramework()
    {
        //if (use_planner)
        //{
        //    await RunPlanner();
        //}
   
        string user_input = builder.input_TMP.text;
        refinedInput.text = "Instruction: " + user_input;

        builder.DisplayProcessingStatusText();

        if (use_sceneAnalyzer)
        {
            await RunSA(user_input);
        }

        if (use_skillLibrary)
        {
            await RunSkillLibrary(user_input);
        }

        if (use_fuzzyModels)
        {
            await RunFuzzyModels(user_input);
        }
        
        // if(use_FeedBackLoop)
        // {
        //     string task_type = "motion"; 
        //     await RunFeedbackStepOne(user_input, task_type); // update with some information about the transform?
        // }
        // only call this if it is the first time it is being called

        //if (modularCallCount <= 1)
        //{
        //    Debug.Log("set prompt true");
        //    await SetBuilderMetaPrompt();
        //}
            
        SetBuilderInput(refinedInput.text);
        //await SetBuilderInput(user_input);

        //builder.DisplayProcessingStatusText();

        // if (use_FeedBackLoop)
        // {
        //     RunFeedback();
        // }
        // else
        // {
        await WriteAndExecuteCode();
        // }

        builder.DisplayProcessingFinishedStatusText();
    }



    public async Task RunFrameworkWithInputAsyncForExperiment(string user_request)
    {
        output_window.text = "Prompt: " + user_request;
        builder.input_TMP.text = user_request;

        string user_input = builder.input_TMP.text;
        refinedInput.text = "Instruction: " + user_input;

        builder.DisplayProcessingStatusText();

        if (use_sceneAnalyzer)
        {
            await RunSA(user_input);
        }

        if (use_skillLibrary)
        {
            await RunSkillLibrary(user_input);
        }

        if (use_fuzzyModels)
        {
            await RunFuzzyModels(user_input);
        }

        // if(use_FeedBackLoop)
        // {
        //     string task_type = "motion"; 
        //     await RunFeedbackStepOne(user_input, task_type); // update with some information about the transform?
        // }
        // only call this if it is the first time it is being called


        //if (modularCallCount <= 1)
        //{
        //    Debug.Log("set prompt true");
        //    await SetBuilderMetaPrompt();
        //}


        SetBuilderInput(refinedInput.text);
        //await SetBuilderInput(user_input);

        //builder.DisplayProcessingStatusText();

        // if (use_FeedBackLoop)
        // {
        //     RunFeedback();
        // }
        // else
        // {
        await WriteAndExecuteCode();
        // }

        builder.DisplayProcessingFinishedStatusText();
    }

    async Task WriteAndExecuteCode(){

        if (use_debugger && use_inspector)
        {
            await RunDebugger(() => RunInspector(() => RunBuilder()));
        }
        else if (use_debugger)
        {
            await RunDebugger(() => RunBuilder());
        }
        else if(use_inspector)
        {
            await RunInspector(() => RunBuilder());
            Compile();
        }
        else
        {
            await RunBuilder();
            Compile();
        }

    }



    // async Task RunFeedbackStepOne(string user_input, string task_type)
    // {
        
    //     if(task_type == "motion")
    //     {
    //         Debug.Log("Running Motion Feedback First Step...");
    //         string object_name = "";
            
    //         if (use_sceneAnalyzer)
    //         {
                
    //                 await movementFeedback.GetObjectFromSA(sceneForInput);
    //                 string objectname = movementFeedback.objectName;
    //                 if(objectname != "null"){
    //                     Debug.Log("****" + objectname + "****");
    //                     await movementFeedback.GetOriginalState();
    //                     firstTransformRelevantObject = "Transform of Relevant Object: " + movementFeedback.initialActionState;
    //                 }

    //         }
    //         else
    //         {
    //             Debug.Log("The feedback loop benefits from the Scene Analyzer. Please enable it or manually in the code: object_name");

    //         }

    //     }
       

    //     // await call to feedback for specific task (starting with animation)
    //     // inside that get the transforms (original and after), understand the world view 
    //     // perhaps consider the user's feedback 
    //     // then run the comparison gate, 
    //     // the GPT that determines what has occurred
    //     // which obtains the initial state and the state after the code has been executed
    //     // then run the event and compare with the original user prompt 
    //     // make an example with feedback (the intuitive physics example )


    // }


    async Task RunPlanner()
    {
        Debug.Log("Now running Planner");
  
        await planner.ConverseWithUser(planner.input_TMP.text);
    }

    async Task RunSA(string user_input)
    {
        Debug.Log("Now running Scene Analyzer");
        await sceneAnalyzer.AnalyzeSceneAsync(user_input);
        sceneForInput = "Scene: " + sceneAnalyzer.chatbot.output + '\n';
   
    }

    async Task RunSkillLibrary(string user_input)
    {
        Debug.Log("Now running Skill Library");
        await skillLibrary.AnalyzeInput(user_input);
        string apiString = skillLibrary.apiRefToBuilder;
        libraryForInput = "Library: \n" + apiString;
    }

    async Task RunFuzzyModels(string user_input)
    {
        Debug.Log("Now running Fuzzy Models");
        await fuzzyModels.StageOne(user_input);
        await fuzzyModels.StageTwo(user_input);
        fuzzyModelsForInput = "Fuzzy world model for object: " + fuzzyModels.finalFuzzyModel + '\n';
   
    }

    //async Task SetBuilderMetaPrompt()
    void SetMetaprompts()
    {
        Debug.Log("Now updating the Builder's MetaPrompt");
        
        // zero-shot. This is used as a baseline.
        if (!use_demonstrations)
        {
            builder.SetMetapromptAndClearHistory(listMetaPrompts.builder_zero_shot_metaprompt);
        }
        else if (use_sceneAnalyzer && use_skillLibrary)
        {
            //await builder.SetMetaPrompt(listMetaPrompts.builder_SceneAnalyzer_SkillLibrary_metaprompt);
            builder.SetMetapromptAndClearHistory(listMetaPrompts.builder_SceneAnalyzer_SkillLibrary_metaprompt);
            inspector.SetMetapromptAndClearHistory(listMetaPrompts.inspector_SkillLibrary_metaprompt);
        }
        else if(use_sceneAnalyzer)
        {
            //await builder.SetMetaPrompt(listMetaPrompts.builder_SceneAnalyzer_metaprompt);
            builder.SetMetapromptAndClearHistory(listMetaPrompts.builder_SceneAnalyzer_metaprompt);
            inspector.SetMetapromptAndClearHistory(listMetaPrompts.inspector_base_metaprompt);
        }

        // Note: for the following two cases, the metaprompts for the inspector is not set.
        // This is because you shouldn't use the inspector without the scene analyzer - it won't be able to check for a wide range of runtime errors
        else if (use_skillLibrary)
        {
            //await builder.SetMetaPrompt(listMetaPrompts.builder_SkillLibrary_metaprompt);
            builder.SetMetapromptAndClearHistory(listMetaPrompts.builder_SkillLibrary_metaprompt);
        }
        else
        {
            //await builder.SetMetaPrompt(listMetaPrompts.builder_Base_metaprompt);
            builder.SetMetapromptAndClearHistory(listMetaPrompts.builder_Base_metaprompt);
        }

        if (use_fuzzyModels)
        {
            builder.metaprompt += listMetaPrompts.builder_FuzzyModelsAddition_metaprompt;
        }

    }

    //async Task SetBuilderInput(string user_input)
    void SetBuilderInput(string user_input)
    {
        refinedInput.text =  firstTransformRelevantObject + "\n" + sceneForInput + libraryForInput + '\n' + fuzzyModelsForInput + user_input;

    }

    string SetInspectorInput(string code)
    {
        string ret = "";
        ret += sceneForInput + '\n';
        ret += libraryForInput + '\n';
        ret += "Code: " + code;

        return ret;
    }

    async Task RunBuilder()
    {
        Debug.Log("Now running Code Builder");
        await builder.WriteCode();
    }

    async Task RunInspector(Func<Task> builderCall)
    {
        Debug.Log("Now running Inspector");
      
        for (int i = 0; i < inspector.max_inspection_count; i++)
        {
            Debug.Log("Inspection #" + i);
         
            await builderCall?.Invoke();
    
            string generated_code = builder.output;

            //string inspector_input = inspector.PrepareInspectorInput(sceneAnalyzer.chatbot.output, generated_code);
            string inspector_input = SetInspectorInput(generated_code);

            await inspector.SendNewChatWithInput(inspector_input);
       
            string inspector_suggestion = inspector.ParseInspectionResult(generated_code, builder.IsMemoryless());

            if (inspector.inspection_done)
            {
                Debug.Log("Broke OUT");
                break;
            }
        
            builder.ReceiveInspectorSuggestion(inspector_suggestion);
        }
    }

    async Task RunDebugger(Func<Task> inspectorOrBuilderCall)
    {
        Debug.Log("Now running Debugger");

        if(debugger.max_debugging_count > 0)
        {
            for (int j = 0; j < debugger.max_debugging_count; j++)
            {
                try
                {
                    //ClearRecordedErrorMessages(); 
                    await inspectorOrBuilderCall?.Invoke();
                
                    Compile();
                
                    break;
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.Message);
                    string all_errors = ConcatenateErrorMessages();
                    string generated_code = builder.output;
                    refinedInput.text = debugger.ParseDebuggerResultSimple(generated_code, all_errors, builder.IsMemoryless());
                    ClearRecordedErrorMessages();
                }
            }

        }
        else{
            Debug.Log("DEBUGGER COUNT IS NOT > 0. UPDATE AND RERUN.");
        }
     
       
    }


    void Compile()
    {
        Debug.Log("Now running Compiler");
        roslynCompiler.GetComponent<CompileCompletionsWithReferences>().RunCode();
    }

    private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
    {

        if (type == LogType.Error || type == LogType.Exception)
        {
           
            error_messages.Add(condition);
        }
    }

    public void ClearRecordedErrorMessages()
    {
        error_messages = new List<string>();
    }

    public string ConcatenateErrorMessages()
    {
        string all_errors = "";
        foreach (string error in error_messages)
        {
            all_errors += error + "; ";
        }
        return all_errors;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= OnLogMessageReceived;
    }

    public void ToggleSL(bool value)
    {
        use_skillLibrary = value;
        SetMetaprompts(); // reset metaprompts
    }

    

    public void ToggleInspector(bool value)
    {
        use_inspector = value;
        use_debugger = value;
        SetMetaprompts(); // reset metaprompts
    }
}