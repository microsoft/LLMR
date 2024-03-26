using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


#nullable enable


namespace Meryel.UnityCodeAssist.Editor
{
    public class AboutWindow : EditorWindow
    {
        GUIStyle? styleLabel;

        public static void Display()
        {
            // Get existing open window or if none, make a new one:
            var window = GetWindow<AboutWindow>();
            window.Show();

            Serilog.Log.Debug("Displaying about window");

            NetMQInitializer.Publisher?.SendAnalyticsEvent("Gui", "AboutWindow_Display");
        }

        private void OnEnable()
        {
            //**--icon
            //var icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Sprites/Gear.png");
            //titleContent = new GUIContent("Code Assist", icon);
            titleContent = new GUIContent("Code Assist About");
        }

        private void OnGUI()
        {
            styleLabel ??= new GUIStyle(GUI.skin.label)
            {
                wordWrap = true,
                alignment = TextAnchor.MiddleLeft,
            };

            EditorGUILayout.LabelField($"Version number: {Assister.Version}", styleLabel, GUILayout.ExpandWidth(true));

#if MERYEL_UCA_LITE_VERSION
            EditorGUILayout.LabelField($"License type: Lite", styleLabel, GUILayout.ExpandWidth(true));
#else // MERYEL_UCA_LITE_VERSION
            EditorGUILayout.LabelField($"License type: Full", styleLabel, GUILayout.ExpandWidth(true));
#endif // MERYEL_UCA_LITE_VERSION

            if (GUILayout.Button("View changelog"))
            {
                Application.OpenURL("https://unitycodeassist.netlify.app/changelog");
            }

            if (GUILayout.Button("View third party notices"))
            {
                Application.OpenURL("https://unitycodeassist.netlify.app/thirdpartynotices");
            }

        }
    }

}