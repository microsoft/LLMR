using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;

// A custom editor window that allows the user to start and stop recording the play mode screen
public class PlayModeRecorder : EditorWindow
{
    // A reference to the recorder controller that handles the recording settings and session
    private RecorderController m_RecorderController;

    // A flag to indicate if the recording is active
    private bool m_IsRecording;

    // A path to save the recorded video
    private string m_SavePath;

    // A menu item to open the editor window
    [MenuItem("Window/Play Mode Recorder")]
    public static void ShowWindow()
    {
        // Get or create the editor window
        var window = GetWindow<PlayModeRecorder>();
        // Set the title
        window.titleContent = new GUIContent("Play Mode Recorder");
    }

    // A method to initialize the recorder controller and the recording settings
    private void InitRecorder()
    {
        // Create a new recorder controller settings
        var settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();

        // Create a new movie recorder settings
        var movieSettings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        // Set the output file name
        movieSettings.OutputFile = m_SavePath + "/PlayModeRecording_" + DefaultWildcard.Take;
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

    // A method to handle the GUI of the editor window
    private void OnGUI()
    {
        // Draw a label for the save path
        EditorGUILayout.LabelField("Save Path", EditorStyles.boldLabel);
        // Draw a text field for the save path
        m_SavePath = EditorGUILayout.TextField(m_SavePath);
        // Draw a button to browse for the save path
        if (GUILayout.Button("Browse"))
        {
            // Open a folder panel to select the save path
            m_SavePath = EditorUtility.OpenFolderPanel("Select Save Path", m_SavePath, "");
        }

        // Draw a space
        //EditorGUILayout.Space();

        // Draw a label for the recording status
        //EditorGUILayout.LabelField("Recording Status", EditorStyles.boldLabel);
        //// Draw a label for the current frame
        //EditorGUILayout.LabelField("Current Frame: " + m_RecorderController.CurrentFrame);
        //// Draw a label for the current time
        //EditorGUILayout.LabelField("Current Time: " + m_RecorderController.Session.GetRecordingTimeSpan());

        // Draw a space
        //EditorGUILayout.Space();

        // Initialize the recorder
        InitRecorder();
        m_RecorderController.PrepareRecording();

        // Draw a button to start or stop the recording
        if (m_IsRecording)
        {
            // If the recording is active, draw a button to stop it
            if (GUILayout.Button("Stop Recording"))
            {
                // Stop the recording
                m_RecorderController.StopRecording();
                // Set the flag to false
                m_IsRecording = false;
            }
        }
        else
        {
            // If the recording is not active, draw a button to start it
            if (GUILayout.Button("Start Recording"))
            {
                //// Initialize the recorder
                //InitRecorder();
                // Start the recording
                m_RecorderController.StartRecording();
                // Set the flag to true
                m_IsRecording = true;
            }
        }
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

    // A method to register the play mode state change callback
    private void OnEnable()
    {
        // Register the play mode state change callback
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    // A method to unregister the play mode state change callback
    private void OnDisable()
    {
        // Unregister the play mode state change callback
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }
}