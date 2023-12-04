using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using TMPro;


public class CompleteAndCompile : MonoBehaviour
{

    public GameObject OpenAICompleter;
    public GameObject roslynCompiler;
    public Inspector inspector;
    public DebuggerGPT debugger;
    public Refiner refiner;
    public SceneParser scene_parser;
    public GameObject sceneprompt;




    private CancellationTokenSource cts = new CancellationTokenSource();


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Run();   
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            bool run_scene_parser = true;
            Run_with_Inspector(run_scene_parser);
            //Run_with_Inspector();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            Run_with_Inspector_and_Refiner();
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Compile();
        }
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            Listen();
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            RunTerrainCode();
            CallGenerateSceneDallE();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            CallSEEM();
        }

    }


    void OnDisable()
    {
        // interrupt the OpenAI response 
        cts.Cancel();
    }


    async void Run_with_Inspector_and_Refiner()
    {
        // get user request
        string user_input = OpenAICompleter.GetComponent<ChatTest>().input.GetComponent<TextMeshPro>().text;
        // analyze scene
        await scene_parser.AnalyzeSceneAsync(user_input);
        //bool include_background_description = true;
        //await scene_parser.AnalyzeSceneAsync(include_background_description);
        //bool include_compiler = true;
        //scene_parser.CleanAndParseHierarchy(include_compiler);

        // refine user request
        // uses interpreted scene summary as input 
        //string refiner_input = refiner.PrepareRefinerInput(scene_parser.scene_parsing_compact, user_input);
        // uses raw scene parsing as input
        string refiner_input = refiner.PrepareRefinerInput(scene_parser.chatbot.output, user_input);
        await refiner.SendChatWithInput(refiner_input);
        // send the refined request to GPT
        OpenAICompleter.GetComponent<ChatTest>().input.GetComponent<TextMeshPro>().text = refiner.output;
        // attempt to compile and run, with the usual inspection checks in place.
        // no need to run the scene parser again since we just ran it
        bool run_scene_parser = false;
        Run_with_Inspector(run_scene_parser);
    }

    async void Run_with_Inspector(bool run_scene_parser)
    {
        if (run_scene_parser)
        {
            string user_input = OpenAICompleter.GetComponent<ChatTest>().input.GetComponent<TextMeshPro>().text;
            await scene_parser.AnalyzeSceneAsync(user_input);
            //bool include_background_description = true;
            //await scene_parser.AnalyzeSceneAsync(include_background_description);

            //bool include_compiler = true;
            //scene_parser.CleanAndParseHierarchy(include_compiler);
        }
        //checking compiler errors
        for (int j = 0; j < debugger.max_debugging_count; j++)
        {
            //Debug.Log("debugging number " + j.ToString());
            try
            {
                // checking style issues
                for (int i = 0; i < inspector.max_inspection_count; i++)
                {
                    //Debug.Log("inspection number " + i.ToString());
                    // get builder's code
                    await OpenAICompleter.GetComponent<ChatTest>().TestChatStream(cts.Token);
                    // inspect builder's code
                    string generated_code = OpenAICompleter.GetComponent<ChatTest>().Output.GetComponent<TextMeshPro>().text;

                    // uses interpreted scene summary as input
                    //string inspector_input = inspector.PrepareInspectorInput(scene_parser.scene_parsing_compact, generated_code);
                    // uses raw scene parsing as input
                    string inspector_input = inspector.PrepareInspectorInput(scene_parser.chatbot.output, generated_code);

                    await inspector.SendNewChatWithInput(inspector_input);
                    // use inspector suggestions as the builder input
                    OpenAICompleter.GetComponent<ChatTest>().input.GetComponent<TextMeshPro>().text = inspector.ParseInspectionResult(generated_code);
    
                    if (inspector.inspection_done)
                    {
                        Debug.Log("inspection done");
                        // clear input if inspection is clear
                        OpenAICompleter.GetComponent<ChatTest>().input.GetComponent<TextMeshPro>().text = "";
                        break;
                    }
                }

                Compile();
                // if we reached this point, we have succeeded in compiling the code and are done with verification
                break;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                string generated_code = OpenAICompleter.GetComponent<ChatTest>().Output.GetComponent<TextMeshPro>().text;
                //await debugger.SendChatWithInput(code_and_error);
                //OpenAICompleter.GetComponent<ChatTest>().input.GetComponent<TextMeshPro>().text = debugger.ParseDebuggerResult();
                OpenAICompleter.GetComponent<ChatTest>().input.GetComponent<TextMeshPro>().text = debugger.ParseDebuggerResultSimple(generated_code, e.Message);

            }
        }


        // clear input if passed both inspection and compiler debugging
        OpenAICompleter.GetComponent<ChatTest>().input.GetComponent<TextMeshPro>().text = "";
    }
   

    async void Run_with_Inspector()
    {
        //checking compiler errors
        for (int j = 0; j < debugger.max_debugging_count; j++)
        {
            try
            {
                // checking style issues
                for (int i = 0; i < inspector.max_inspection_count; i++)
                {
                    // get builder's code
                    await OpenAICompleter.GetComponent<ChatTest>().TestChatStream(cts.Token);
                    // inspect builder's code
                    string generated_code = OpenAICompleter.GetComponent<ChatTest>().Output.GetComponent<TextMeshPro>().text;
                    await inspector.SendNewChatWithInput(generated_code);
                    // use inspector suggestions as the builder input
                    OpenAICompleter.GetComponent<ChatTest>().input.GetComponent<TextMeshPro>().text = inspector.ParseInspectionResult(generated_code);

                    if (inspector.inspection_done)
                    {
                        Debug.Log("inspection done");
                        // clear input if inspection is clear
                        OpenAICompleter.GetComponent<ChatTest>().input.GetComponent<TextMeshPro>().text = "";
                        break;
                    }
                }

                Compile();
                // if we reached this point, we have succeeded in compiling the code and are done with verification
                break;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                string generated_code = OpenAICompleter.GetComponent<ChatTest>().Output.GetComponent<TextMeshPro>().text;
                //await debugger.SendChatWithInput(code_and_error);
                //OpenAICompleter.GetComponent<ChatTest>().input.GetComponent<TextMeshPro>().text = debugger.ParseDebuggerResult();
                OpenAICompleter.GetComponent<ChatTest>().input.GetComponent<TextMeshPro>().text = debugger.ParseDebuggerResultSimple(generated_code, e.Message);

            }
        }

        // clear input if passed both inspection and compiler debugging
        OpenAICompleter.GetComponent<ChatTest>().input.GetComponent<TextMeshPro>().text = "";
    }

    async void Run()
    {
        try
        {
            await OpenAICompleter.GetComponent<ChatTest>().TestChatStream(cts.Token);

            Compile();
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

    async void RunTerrainCode()
    {
        try
        {
            await OpenAICompleter.GetComponent<ChatTerrainBuilder>().TestChatStream(cts.Token);

            CompileTerrain();
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


    public void CallSEEM()
    {
        Debug.Log("Calling seem");

        CSHttpClientSample seemcall = FindObjectOfType<CSHttpClientSample>();
        

        //seemcall.local_img;
        seemcall.MakeRequest();
    }

  
    // a method to start the coroutine
    public void CallGenerateSceneDallE()
    {
        // start the coroutine and pass the user prompt
        string userPrompt = sceneprompt.GetComponent<TextMeshPro>().text;
        Debug.Log(userPrompt);
        StartCoroutine(GenerateSceneDallECoroutine(userPrompt));
    }

    // a coroutine to call GenerateSceneDallE and wait for the result
    private IEnumerator GenerateSceneDallECoroutine(string prompt)
    {
        // a variable to store the result
        string result = null;

        CreateDallEScene createDallEScene = FindObjectOfType<CreateDallEScene>();

        // call GenerateSceneDallE and pass the prompt and a callback
        createDallEScene.GenerateSceneDallE(prompt, (r) => result = r);

        // wait until the result is not null
        yield return new WaitUntil(() => result != null);

        // do something with the result
        Debug.Log("The result is: " + result);

        //CallSEEM();

        // optionally, you can also access the image sprite from the createDallEScene script
        // Debug.Log("The image sprite is: " + createDallEScene.image.sprite);
    }

    void Listen()
    {
        //speechButton.GetComponent<SpeechButton>().ButtonClick();
    }
    

    void CompileTerrain()
    {
        roslynCompiler.GetComponent<CompileCompletionsWithReferences>().RunTerrrainCode();        
    }

    void Compile()
    {
        roslynCompiler.GetComponent<CompileCompletionsWithReferences>().RunCode();        
    }
}
