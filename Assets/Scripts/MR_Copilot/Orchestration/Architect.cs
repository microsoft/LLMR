using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using TMPro;
using System.Diagnostics.SymbolStore;

public class Architect : MonoBehaviour
{

    public Builder builder;
    public GameObject roslynCompiler;
    public Inspector inspector;
    public DebuggerGPT debugger;
    public Refiner refiner;
    public SceneParser scene_parser;
    //public DetailedData detailedData;
    public TextMeshPro refinedInput;
    private bool refined;
    public FuzzyModelMock FuzzyModel;

    // for chat stream interruption
    //private CancellationTokenSource cts = new CancellationTokenSource();


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Run();
            refined = false;
        }
        if (Input.GetKeyDown(KeyCode.RightControl))
        {
            bool run_scene_parser = true;
            Run_with_Inspector(run_scene_parser);
            refined = false;
        }
        if (Input.GetKeyDown(KeyCode.RightAlt))
        {
            Run_with_Inspector_and_Refiner();
            refined = true;
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Compile();
        }
        //if (Input.GetKeyDown(KeyCode.LeftAlt))
        //{
        //    Listen();
        //}

    }

    async void Run_with_Inspector_and_Refiner()
    {
        // get user request
        string user_input = builder.input_TMP.text;
        // analyze scene
        await scene_parser.AnalyzeSceneAsync(user_input);
        // refine user request
        // uses raw scene parsing as input
        string refiner_input = refiner.PrepareRefinerInput(scene_parser.chatbot.output, user_input);
        await refiner.SendChatWithInput(refiner_input);
        // send the refined request to builder; include scene summary if desired
        string refinement_result = refiner.ParseRefinementResult();
        //print(refinement_result);
        builder.ReceiveRefinerOutput(scene_parser.chatbot.output, refinement_result);
        
        // attempt to compile and run, with the usual inspection checks in place.
        // no need to run the scene parser again since we just ran it
        bool run_scene_parser = false;
        bool refined = true;
        Run_with_Inspector(run_scene_parser);
    }

    async void Run_with_Inspector(bool run_scene_parser)
    {
        if (!refined) //otherwise assume this is done by the Refiner
        {
            string user_input = builder.input_TMP.text;
            refinedInput.text = user_input;
            builder.DisplayProcessingStatusText();
        } 
        // analyze the scene if desired
        if (run_scene_parser)
        {
            await scene_parser.AnalyzeSceneAsync(refinedInput.text);
            // add scene summary to Builder input if desired
            if (builder.receive_scene_summary)
            {
                //builder.input_TMP.text = "Scene: " + scene_parser.chatbot.output + '\n' + "Instruction: " + builder.input_TMP.text;
                //refinedInput.text = "Scene: " + scene_parser.chatbot.output + '\n' + "Instruction: " + builder.input_TMP.text;
                refinedInput.text = "Scene: " + scene_parser.chatbot.output + '\n' + "Instruction: " + refinedInput.text;
            }
        }
       
        //checking compiler errors
        for (int j = 0; j < debugger.max_debugging_count; j++)
        {
            try
            {
                // checking stylistic issues
                for (int i = 0; i < inspector.max_inspection_count; i++)
                {
                    // make the builder write code
                    await builder.WriteCode();
                    // inspect the builder's code
                    string generated_code = builder.output;
                    // uses the raw scene parsing as input
                    string inspector_input = inspector.PrepareInspectorInput(scene_parser.chatbot.output, generated_code);
                    // clear the inspector's memory before chatting with it
                    await inspector.SendNewChatWithInput(inspector_input);
                    // process inspector output. inspection_done will be set here.
                    string inspector_suggestion = inspector.ParseInspectionResult(generated_code, builder.IsMemoryless());
                    
                    if (inspector.inspection_done)
                    {
                        //builder.input_TMP.text = "";
                        //builder.input = "";
                        //refinedInput.text = "";
                        break;
                    }
                    // if the Inspector finds problems, its suggestions are used as input to the Builder
                    builder.ReceiveInspectorSuggestion(inspector_suggestion);
                }

                // try compiling. Any errors will be caught and the debugger will trigger.
                Compile();
                // if we reached this point, we have succeeded in compiling the code and are done with verification
                break;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                string generated_code = builder.output;
                //builder.input_TMP.text = debugger.ParseDebuggerResultSimple(generated_code, e.Message, builder.IsMemoryless());
                refinedInput.text = debugger.ParseDebuggerResultSimple(generated_code, e.Message, builder.IsMemoryless());
            }
        }

        // clear input if passed both inspection and compiler debugging
        //builder.input_TMP.text = "";
        builder.DisplayProcessingFinishedStatusText();
        refinedInput.text = "";
    }

    async void Run()
    {
        string user_request = builder.input_TMP.text;
        // tells the user that the GPT is processing 
        //builder.inputTMP.text = builder.processing_status_text;
        builder.DisplayProcessingStatusText();
        try
        {
            //await OpenAICompleter.GetComponent<ChatTest>().TestChatStream(cts.Token);
            if (builder.receive_scene_summary)
            {
                await scene_parser.AnalyzeSceneAsync(user_request);
                refinedInput.text = "Scene: " + scene_parser.chatbot.output + '\n' + "Instructions: " + user_request;
                //builder.input_TMP.text = "Scene: " + scene_parser.chatbot.output + '\n' + "Instructions: " + builder.input_TMP.text;
            }
            else { refinedInput.text = user_request; }
            
            await builder.WriteCode();

            Compile();
            // Tells user that the processing is finished.
            builder.DisplayProcessingFinishedStatusText();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
        //finally
        //{
        //    Compile();
        //}
    }

    void Listen()
    {
        //speechButton.GetComponent<SpeechButton>().ButtonClick();
    }

    void Compile()
    {
        roslynCompiler.GetComponent<CompileCompletionsWithReferences>().RunCode();
    }


    //void OnDisable()
    //{
    //    // interrupt the OpenAI response 
    //    cts.Cancel();
    //}
}
