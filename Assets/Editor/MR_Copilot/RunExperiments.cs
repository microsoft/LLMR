using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using System;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using System.Threading;

public class RunExperiments : EditorWindow
{
    //public int num_exp_to_run = 1;
    public string exp_name = "test";
    public string file_name = "single_prompts_test";
    public string chat_manager_name = "ChatManager";
    public ExperimentType exp_type = ExperimentType.Single_Prompts;
    public bool use_scene_analyzer = true;
    public bool use_skill_library = true;
    public bool use_inspector = true;
    public bool use_demonstrations = true;
    public ChatCompilationManagerModular chat_manager;

    //public string exp_prompt_dir_rel = "Experiments/prompts/";
    //public string exp_result_dir_rel = "Experiments/results/";

    public string exp_prompt_dir_rel = Path.Combine("Experiments", "prompts");
    public string exp_result_dir_rel = Path.Combine("Experiments", "results");

    public string exp_prompt_dir;
    public string exp_result_dir;
    public string exp_context_dir;
    public string exp_recording_dir;

    public int delay_time = 2500;
    public bool record_vid;

    // for halting the async functions
    private CancellationTokenSource cts = new CancellationTokenSource();

    // A reference to the recorder controller that handles the recording settings and session
    private RecorderController m_RecorderController;

    // A flag to indicate if the recording is active
    private bool m_IsRecording;


    // Add a menu item to open the window
    [MenuItem("Experiments/Run")]
    public static void ShowWindow()
    {
        // Get or create the window instance
        var window = GetWindow<RunExperiments>();
        // Set the window title
        window.titleContent = new GUIContent("Run Experiments");
        // Show the window
        window.Show();
    }

    void ManageExperimentDirs()
    {
        exp_prompt_dir = Path.Combine(Application.dataPath, exp_prompt_dir_rel);
        exp_result_dir = Path.Combine(Application.dataPath, exp_result_dir_rel, exp_name);
        exp_context_dir = Path.Combine(Application.dataPath, exp_result_dir_rel, exp_name, "builder_prompts");
        exp_recording_dir = Path.Combine(Application.dataPath, exp_result_dir_rel, exp_name, "recordings");
        // create directories if they don't exist
        CreateDirIfNotExist(exp_result_dir);
        CreateDirIfNotExist(exp_prompt_dir);
        CreateDirIfNotExist(exp_context_dir);
        CreateDirIfNotExist(exp_recording_dir);
    }


    async Task RunAllExperiments()
    {
        bool[] bool_values = { false, true };
        // make a root dir for all ablation experiments, which use the same set of prompts
        exp_result_dir_rel = Path.Combine(exp_result_dir_rel, exp_name);
        // run all ablation experiments: 
        for (int i = 0; i < bool_values.Length; i++)
        {
            for (int j = 0; j < bool_values.Length; j++)
            {
                use_scene_analyzer = true;
                use_inspector = bool_values[i];
                use_skill_library = bool_values[j];
                // reset the exp name
                exp_name = "SA"; // SA is always used here.
                if (use_inspector)
                {
                    exp_name += "_I";
                }
                if (use_skill_library)
                {
                    exp_name += "_SL";
                }

                await RunExperiment();
            }
        }

        // all ablated
        use_scene_analyzer = false;
        use_inspector = false;
        use_skill_library = false;
        exp_name = "base";

        await RunExperiment();

        // all ablated and zero_shot
        use_scene_analyzer = false;
        use_inspector = false;
        use_skill_library = false;
        use_demonstrations = false;
        exp_name = "base_zero_shot";

        await RunExperiment();

    }

    //void RunExperiment()
    async Task RunExperiment()
    {
        // create directories if they don't exist
        ManageExperimentDirs();
        // load relevant files
        Debug.Log("Running experiment on file: " + file_name);
        string file_path = Path.Combine(exp_prompt_dir, file_name + ".txt");
        string exp_prompts = ReadTxtAsString(file_path);

        // record meta data, e.g., ablation options
        LogMetaData();

        // ablation: enable/disable various modules
        chat_manager = GameObject.Find(chat_manager_name).GetComponent<ChatCompilationManagerModular>();
        chat_manager.use_sceneAnalyzer = use_scene_analyzer;
        chat_manager.use_inspector = use_inspector;
        chat_manager.use_debugger = use_inspector;
        chat_manager.use_skillLibrary = use_skill_library;
        chat_manager.use_demonstrations = use_demonstrations;

        //Debug.Log("use skill lib: " + chat_manager.use_skillLibrary + "; use ins: " + chat_manager.use_inspector);

        // run experiments based on type
        //EditorApplication.isPlaying = true;

        if (exp_type == ExperimentType.Single_Prompts)
        {
            //RunSinglePromptExperiments(exp_prompts);
            await RunSinglePromptExperiments(exp_prompts);
        }
        else if (exp_type == ExperimentType.Sequential_Prompts)
        {
            await RunSequentialPromptExperiments(exp_prompts);
        }

        Debug.Log("Experiments done!");

        //EditorApplication.isPlaying = false;
    }

    //async void RunSinglePromptExperiments(string prompt_str)
    async Task RunSinglePromptExperiments(string prompt_str)
    {
        List<string> prompts = prompt_str.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
        float num_error = 0f;
        float total_num = (float)prompts.Count;
        string results_path = Path.Combine(exp_result_dir, "results.txt");

        //foreach (string prompt in prompts)
        for (int i = 0; i < prompts.Count; i++)
        {
            // halt the experiments if the stop button is pressed
            cts.Token.ThrowIfCancellationRequested();

            // get the prompt
            string prompt = prompts[i];
            await Task.Delay(delay_time);

            // enter the play mode if not already playing
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = true;
                await Task.Delay(delay_time);
                // Start the recording
                if (record_vid)
                {
                    InitRecorder(exp_recording_dir, i);
                    m_RecorderController.PrepareRecording();
                    m_RecorderController.StartRecording();
                    m_IsRecording = true;
                }

            }

            string outcome = "passed";
            string errs = "";
            // NOTE: need to locate the chat manager again each time we re-enter the play mode.
            chat_manager = GameObject.Find(chat_manager_name).GetComponent<ChatCompilationManagerModular>();
            chat_manager.use_sceneAnalyzer = use_scene_analyzer;
            chat_manager.use_inspector = use_inspector;
            chat_manager.use_debugger = use_inspector;
            chat_manager.use_skillLibrary = use_skill_library;
            chat_manager.use_demonstrations = use_demonstrations;

            // run the framework
            try
            {
                await chat_manager.RunFrameworkWithInputAsyncForExperiment(prompt);
            }
            catch (Exception ex)
            {
                Debug.Log("Compilation error appeared.");
            }
            //await chat_manager.RunFrameworkWithInputAsync(prompt);

            // let the scene run for a bit to see if any runtime errors are thrown. In most cases they should be thrown immediately.
            await Task.Delay(delay_time);

            // check for compilation and runtime errors
            if (chat_manager.error_messages.Count > 0)
            {
                outcome = "failed";
                num_error += 1;
                errs = chat_manager.ConcatenateErrorMessages();
            }

            // record the result
            File.AppendAllText(results_path, "prompt: " + prompt + "; Result: " + outcome + "; Errors: " + errs + '\n');
            string context_path = Path.Combine(exp_context_dir, "builder_context_" + i + ".txt");
            chat_manager.builder.SaveCurrentContext(context_path);

            // exit the play mode to completely reset the scene, GPT memories, etc.
            if (EditorApplication.isPlaying)
            {   // Stop the recording
                if (record_vid)
                {
                    m_RecorderController.StopRecording();
                    m_IsRecording = false;
                }

                EditorApplication.isPlaying = false;
            }
        }

        // record the result
        File.AppendAllText(results_path, "Percentage of errors generated: " + num_error / total_num);

        //m_RecorderController.StopRecording();
        //m_IsRecording = false;
    }

    async Task RunSequentialPromptExperiments(string prompt_str)
    {
        List<string> prompts = prompt_str.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
        List<List<string>> prompts_seq = ParseSequentialPrompts(prompts);
        float num_error = 0f;
        float percentage_accomplished = 0f;
        float accomplished = 0f;
        float num_before_err = 0f;
        float total_prompt_seq_num = (float)prompts.Count;
        float total_prompt_num = (float)CountTotalNumPrompts(prompts_seq);
        string results_path = Path.Combine(exp_result_dir, "results.txt");
        Debug.Log("results_path: " + results_path);

        //foreach (string prompt in prompts)
        for (int i = 0; i < prompts_seq.Count; i++)
        {
            List<string> prompt_seq = prompts_seq[i];
            string outcome = "passed";
            string errs = "";
            int err_idx = prompt_seq.Count;

            for (int j = 0; j < prompt_seq.Count; j++)
            {
                // if any errors were generated in the sequence, we early-stop this sequence and deem the rest of the prompts to have failed
                if (outcome == "failed")
                {
                    err_idx = j - 1;
                    num_error += prompt_seq.Count - j;
                    break;
                }
                // halt the experiments if the stop button is pressed
                cts.Token.ThrowIfCancellationRequested();

                // get the prompt
                string prompt = prompt_seq[j];
                Debug.Log("Prompt_" + i + j + ": " + prompt);
                await Task.Delay(delay_time);

                // enter the play mode if not already playing
                if (!EditorApplication.isPlaying)
                {
                    EditorApplication.isPlaying = true;
                    await Task.Delay(delay_time);
                    // Start the recording
                    if (record_vid)
                    {
                        InitRecorder(exp_recording_dir, i);
                        m_RecorderController.PrepareRecording();
                        m_RecorderController.StartRecording();
                        m_IsRecording = true;
                    }
                }

                // NOTE: need to locate the chat manager again each time we re-enter the play mode.
                chat_manager = GameObject.Find(chat_manager_name).GetComponent<ChatCompilationManagerModular>();
                chat_manager.use_sceneAnalyzer = use_scene_analyzer;
                chat_manager.use_inspector = use_inspector;
                chat_manager.use_debugger = use_inspector;
                chat_manager.use_skillLibrary = use_skill_library;
                chat_manager.use_demonstrations = use_demonstrations;

                // run the framework
                try
                {
                    await chat_manager.RunFrameworkWithInputAsyncForExperiment(prompt);
                }
                catch (Exception ex)
                {
                    Debug.Log("Compilation error appeared.");
                }

                // let the scene run for a bit to see if any runtime errors are thrown. In most cases they should be thrown immediately.
                await Task.Delay(delay_time);

                // check for compilation and runtime errors
                if (chat_manager.error_messages.Count > 0)
                {
                    outcome = "failed";
                    num_error += 1;
                    errs = chat_manager.ConcatenateErrorMessages();
                }
                else // no errors for this prompt 
                {
                    num_before_err += 1;
                }

                // record the result
                //File.AppendAllText(results_path, "prompt: " + prompt + "; Result: " + outcome + "; Errors: " + errs + '\n');
                string context_path = Path.Combine(exp_context_dir, "builder_context_" + i + "_" + j + ".txt");
                chat_manager.builder.SaveCurrentContext(context_path);

                // exit the play mode to completely reset the scene, GPT memories, etc.
                if (EditorApplication.isPlaying)
                {   // Stop the recording
                    if (record_vid)
                    {
                        m_RecorderController.StopRecording();
                        m_IsRecording = false;
                    }

                    EditorApplication.isPlaying = false;
                }
            }

            // a sequence of prompt is finished
            // compute stats
            if (err_idx == prompt_seq.Count)
            {
                accomplished += 1;
            }
            float percentage_passed = (float)err_idx / prompt_seq.Count;
            percentage_accomplished += percentage_passed;

            // write stats
            File.AppendAllText(results_path, "prompt: " + String.Join("; ", prompt_seq.ToArray()) + "; " +
                "Result: " + outcome + "; Percentage accomplished: " + percentage_passed + "; Errors: " + errs + '\n');
        }

        // record results
        File.AppendAllText(results_path, "Percentage of errors generated: " + num_error / total_prompt_num + '\n');
        File.AppendAllText(results_path, "Average percentage of passed prompts: " + percentage_accomplished / total_prompt_seq_num + '\n');
        File.AppendAllText(results_path, "Number of sequential prompts completely fulfilled: " + accomplished / total_prompt_seq_num + '\n');
        //File.AppendAllText(results_path, "Average number of prompts: " + total_prompt_num / total_prompt_seq_num + '\n');
        //File.AppendAllText(results_path, "Average number of prompts passed before first error: " + num_before_err / total_prompt_seq_num);
    }


    // Draw the window content
    //private void OnGUI()
    async void OnGUI()
    {
        // Use a label field to display some instructions
        //EditorGUILayout.LabelField("Instructions go here", EditorStyles.boldLabel);

        // Get input from the user
        //num_exp_to_run = EditorGUILayout.IntField("Number of experiments", num_exp_to_run);
        file_name = EditorGUILayout.TextField("Experiment file", file_name);
        exp_name = EditorGUILayout.TextField("Experiment name", exp_name);
        delay_time = EditorGUILayout.IntField("Delay time (ms)", delay_time);

        // Draw a label and a popup for the options
        EditorGUILayout.LabelField("Pick the type of experiment to run:");
        exp_type = (ExperimentType)EditorGUILayout.EnumPopup(exp_type);

        // enable or disable different modules
        EditorGUILayout.LabelField("Choose the modules to use.");
        use_scene_analyzer = EditorGUILayout.Toggle("Use Scene Analyzer", use_scene_analyzer);
        use_skill_library = EditorGUILayout.Toggle("Use Skill Library", use_skill_library);
        use_inspector = EditorGUILayout.Toggle("Use Inspector", use_inspector);
        use_demonstrations = EditorGUILayout.Toggle("Use Demonstrations in Builder metaprompt", use_demonstrations);

        // other options
        EditorGUILayout.LabelField("Misc.");
        record_vid = EditorGUILayout.Toggle("Record Video", record_vid);

        if (GUILayout.Button("Run Experiment"))
        {
            //RunExperiment();
            await RunExperiment();
        }

        else if (GUILayout.Button("Run All Ablation Experiments"))
        {
            await RunAllExperiments();
        }

        else if (GUILayout.Button("Halt Experiment"))
        {
            cts.Cancel();
        }
    }

    void LogMetaData()
    {
        string configuration_path = Path.Combine(exp_result_dir, "configuration.txt");
        File.AppendAllText(configuration_path, "Use SA: " + use_scene_analyzer + "; Use SL: " + use_skill_library + "; Use Inspector: " + use_inspector);
    }

    void CreateDirIfNotExist(string path)
    {
        bool exists = System.IO.Directory.Exists(path);

        if (!exists)
            System.IO.Directory.CreateDirectory(path);
    }


    List<List<string>> ParseSequentialPrompts(List<string> prompts)
    {
        List<List<string>> ret = new List<List<string>>();
        foreach (string prompt in prompts)
        {
            List<string> prompt_split = prompt.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
            ret.Add(prompt_split);
            //Debug.Log("parsing: " + prompt + ". Parsed count: " + prompt_split.Count);
        }
        return ret;
    }

    int CountTotalNumPrompts(List<List<string>> prompts)
    {
        int ret = 0;
        for (int i = 0; i < prompts.Count; i++)
        {
            ret += prompts[i].Count;
        }

        return ret;
    }

    string ReadTxtAsString(string file_path)
    {
        string ret = "";
        StreamReader inp_stm = new StreamReader(file_path);

        while (!inp_stm.EndOfStream)
        {
            string inp_ln = inp_stm.ReadLine();
            // Do Something with the input. 
            ret += inp_ln + '\n';
        }

        return ret;
    }

    public enum ExperimentType
    {
        Single_Prompts,
        Sequential_Prompts
    }



    // ======== recording ========= //

    // A method to initialize the recorder controller and the recording settings
    private void InitRecorder(string save_dir, int prompt_num)
    {
        // Create a new recorder controller settings
        var settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();

        // Create a new movie recorder settings
        var movieSettings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        // Set the output file name
        //Debug.Log(save_dir);
        //Debug.Log(Path.Combine(save_dir, "PlayModeRecording"));
        string filename = "PlayModeRecording_" + prompt_num;
        movieSettings.OutputFile = Path.Combine(save_dir, filename);
        // Set the image input settings to capture the game view
        movieSettings.ImageInputSettings = new GameViewInputSettings();
        // Set the audio input settings to capture the game audio
        movieSettings.AudioInputSettings.PreserveAudio = true;

        // Add the movie recorder settings to the recorder controller settings
        settings.AddRecorderSettings(movieSettings);

        // Set the recorder controller settings to use the play mode frame rate
        settings.SetRecordModeToManual();
        settings.FrameRate = 60.0f;

        // Create a new recorder controller with the settings
        m_RecorderController = new RecorderController(settings);
    }

    // A method to handle the play mode state change
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // If the play mode is exiting
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            // If the recording is active
            if (m_IsRecording)
            {
                // Stop the recording
                m_RecorderController.StopRecording();
                // Set the flag to false
                m_IsRecording = false;
            }
        }
        // If the play mode is entering
        else if (state == PlayModeStateChange.EnteredPlayMode)
        {
            // If the recording was active before exiting
            if (m_RecorderController != null && m_RecorderController.Settings != null)
            {
                // Start the recording
                m_RecorderController.StartRecording();
                // Set the flag to true
                m_IsRecording = true;
            }
        }
    }


    //// A method to register the play mode state change callback
    //private void OnEnable()
    //{
    //    // Register the play mode state change callback
    //    EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    //}

    //// A method to unregister the play mode state change callback
    //private void OnDisable()
    //{
    //    // Unregister the play mode state change callback
    //    EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    //}


}