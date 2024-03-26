using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


#nullable enable


namespace Meryel.UnityCodeAssist.Editor
{
    public class FeedbackWindow : EditorWindow
    {

        GUIStyle? styleLabel;

        public static void Display()
        {
            NetMQInitializer.Publisher?.SendRequestInternalLog();

            // Get existing open window or if none, make a new one:
            var window = GetWindow<FeedbackWindow>();
            window.Show();

            Serilog.Log.Debug("Displaying feedback window");

            NetMQInitializer.Publisher?.SendAnalyticsEvent("Gui", "FeedbackWindow_Display");
        }


        private void OnEnable()
        {
            //**--icon
            //var icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Sprites/Gear.png");
            //titleContent = new GUIContent("Code Assist", icon);
            titleContent = new GUIContent("Code Assist Feedback");
        }

        private void OnGUI()
        {
            var errorCount = Logger.ELogger.GetErrorCountInInternalLog();
            var warningCount = Logger.ELogger.GetWarningCountInInternalLog();
            var logContent = Logger.ELogger.GetInternalLogContent();
            if (!string.IsNullOrEmpty(Logger.ELogger.VsInternalLog))
                logContent += Logger.ELogger.VsInternalLog;

            styleLabel ??= new GUIStyle(GUI.skin.label)
            {
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
            };
            
            if (errorCount > 0)
                EditorGUILayout.LabelField($"{errorCount} error(s) found in logs. Please submit a feedback (via e-mail, Discord or GitHub) with the logs if possible.", styleLabel, GUILayout.ExpandWidth(true));
            else if (warningCount > 0)
                EditorGUILayout.LabelField($"{warningCount} warnings(s) found in logs. Please submit a feedback (via e-mail, Discord or GitHub) with the logs if possible.", styleLabel, GUILayout.ExpandWidth(true));
            else
                EditorGUILayout.LabelField("No errors found in logs. Please submit a feedback (via e-mail, Discord or GitHub) describing what went wrong or unexpected.", styleLabel, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("Send e-mail"))
            {
                var uri = "mailto:merryyellow@outlook.com";
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(uri));
            }

            if (GUILayout.Button("Send Discord message"))
            {
                //var uri = "discord://invites/2CgKHDq";
                var uri = "https://discord.gg/2CgKHDq";
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(uri));
            }

            if (GUILayout.Button("Submit GitHub issue"))
            {
                var uri = "https://github.com/merryyellow/Unity-Code-Assist/issues/new/choose";
                Application.OpenURL(uri);
            }

            EditorGUILayout.Separator();

            if (GUILayout.Button("View Unity full log"))
            {
                var filePath = Logger.ELogger.FilePath;
                System.Diagnostics.Process.Start(filePath);
            }

            if (GUILayout.Button("View Visual Studio full log"))
            {
                var filePath = Logger.ELogger.VSFilePath;
                System.Diagnostics.Process.Start(filePath);
            }

            if (GUILayout.Button("Copy recent logs to clipboard"))
            {
                GUIUtility.systemCopyBuffer = logContent;
            }

            EditorGUILayout.LabelField("Recent logs:", styleLabel, GUILayout.ExpandWidth(true));
            EditorGUILayout.SelectableLabel(logContent, EditorStyles.textArea, GUILayout.ExpandHeight(true));
        }



    }
}