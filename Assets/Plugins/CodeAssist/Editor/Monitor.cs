using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;


#nullable enable


namespace Meryel.UnityCodeAssist.Editor
{

    [InitializeOnLoad]
    public static class Monitor
    {
        private readonly static string tagManagerFilePath;
        private static System.DateTime previousTagManagerLastWrite;

        private static bool isAppFocused;
        private static bool isAppFocusedOnTagManager;

        private static int dirtyCounter;
        private static readonly Dictionary<GameObject, int> dirtyDict;

        static Monitor()
        {
            tagManagerFilePath = CommonTools.GetTagManagerFilePath();
            previousTagManagerLastWrite = System.IO.File.GetLastWriteTime(tagManagerFilePath);

            dirtyDict = new Dictionary<GameObject, int>();
            dirtyCounter = 0;

            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.update += OnUpdate;
            Undo.postprocessModifications += MyPostprocessModificationsCallback;
            Undo.undoRedoPerformed += MyUndoCallback;
            Selection.selectionChanged += OnSelectionChanged;
            //EditorSceneManager.sceneOpened += EditorSceneManager_sceneOpened;
            EditorSceneManager.activeSceneChangedInEditMode += EditorSceneManager_activeSceneChangedInEditMode;

            Application.logMessageReceived += Application_logMessageReceived;
            //System.Threading.Tasks.TaskScheduler.UnobservedTaskException += 
        }

        private static void EditorSceneManager_activeSceneChangedInEditMode(Scene arg0, Scene arg1)
        {
            //Debug.Log("EditorSceneManager_activeSceneChangedInEditMode");
            OnHierarchyChanged();
        }

        private static void EditorSceneManager_sceneOpened(Scene scene, OpenSceneMode mode)
        {
            Serilog.Log.Debug("Monitor {Event} scene:{Scene} mode:{Mode}", nameof(EditorSceneManager_sceneOpened), scene.name, mode);
            //Debug.Log("EditorSceneManager_sceneOpened");
            OnHierarchyChanged();
        }

        static void OnUpdate()
        {
            string? currentEditorFocus = null;
            if (Selection.activeObject)
                currentEditorFocus = Selection.activeObject.GetType().ToString();

            var currentTagManagerLastWrite = System.IO.File.GetLastWriteTime(tagManagerFilePath);
            if (currentTagManagerLastWrite != previousTagManagerLastWrite)
            {
                previousTagManagerLastWrite = currentTagManagerLastWrite;
                OnTagsOrLayersModified();
            }
            else if (currentEditorFocus == "UnityEditor.TagManager")
            {
                // since unity does not commit changes to the file immediately, checking if user is displaying and focusing on tag manager (tags & layers) inspector
                isAppFocusedOnTagManager = true;
            }
            

            if (isAppFocused != UnityEditorInternal.InternalEditorUtility.isApplicationActive)
            {
                isAppFocused = UnityEditorInternal.InternalEditorUtility.isApplicationActive;
                OnOnUnityEditorFocusChanged(isAppFocused);
                Serilog.Log.Debug("On focus {State}", isAppFocused);
            }
        }

        static void OnTagsOrLayersModified()
        {
            Serilog.Log.Debug("Monitor {Event}", nameof(OnTagsOrLayersModified));

            Assister.SendTagsAndLayers();
        }

        static void OnHierarchyChanged()
        {
            Serilog.Log.Debug("Monitor {Event}", nameof(OnHierarchyChanged));

            // For requesting active doc's GO
            NetMQInitializer.Publisher?.SendHandshake();

            if (ScriptFinder.GetActiveGameObject(out var activeGO))
                NetMQInitializer.Publisher?.SendGameObject(activeGO);
            //Assister.SendTagsAndLayers(); Don't send tags & layers here
        }

        static UndoPropertyModification[] MyPostprocessModificationsCallback(UndoPropertyModification[] modifications)
        {
            Serilog.Log.Debug("Monitor {Event}", nameof(MyPostprocessModificationsCallback));

            foreach (var modification in modifications)
            {
                var target = modification.currentValue?.target;
                SetDirty(target);
            }

            // here, you can perform processing of the recorded modifications before returning them
            return modifications;
        }

        static void MyUndoCallback()
        {
            Serilog.Log.Debug("Monitor {Event}", nameof(MyUndoCallback));
            // code for the action to take on Undo
        }

        static void OnOnUnityEditorFocusChanged(bool isFocused)
        {
            if (!isFocused)
            {
                if (isAppFocusedOnTagManager)
                {
                    isAppFocusedOnTagManager = false;
                    OnTagsOrLayersModified();
                }

                OnSelectionChanged();
                FlushAllDirty();
                /*
                Serilog.Log.Debug("exporting {Count} objects", selectedObjects.Count);

                //**--if too many
                foreach (var obj in selectedObjects)
                {
                    if (obj is GameObject go)
                        NetMQInitializer.Publisher.SendGameObject(go);
                }

                selectedObjects.Clear();
                */
            }
        }

        static void OnSelectionChanged()
        {
            
            //**--check order, last selected should be sent last as well
            //**--limit here, what if too many?
            //selectedObjects.UnionWith(Selection.objects);
            foreach(var so in Selection.objects)
            {
                SetDirty(so);
            }
        }

        public static void SetDirty(Object? obj)
        {
            if (obj == null)
                return;
            else if (obj is GameObject go && go)
                SetDirty(go);
            else if (obj is Component component && component)
                SetDirty(component.gameObject);
            //else
                //;//**--scriptable obj
        }

        public static void SetDirty(GameObject go)
        {
            dirtyCounter++;
            dirtyDict[go] = dirtyCounter;
        }

        static void FlushAllDirty()
        {
            // Sending order is important, must send them in the same order as they are added to/modified in the collection
            // Using dict instead of hashset because of that. Dict value is used as add/modify order

            var sortedDict = from entry in dirtyDict orderby entry.Value descending select entry;

            foreach (var entry in sortedDict)
            {
                var go = entry.Key;
                NetMQInitializer.Publisher?.SendGameObject(go);
            }

            dirtyDict.Clear();
            dirtyCounter = 0;
        }


        private static void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Exception && type != LogType.Error && type != LogType.Warning)
                return;

            if (!stackTrace.Contains("Meryel.UnityCodeAssist.Editor"))
                return;

            var typeStr = type.ToString();

            NetMQInitializer.Publisher?.SendErrorReport(condition, stackTrace, typeStr);
        }


        public static void LazyLoad(string category)
        {
            if (category == "PlayerPrefs")
            {
                Preferences.PreferenceMonitor.InstanceOfPlayerPrefs.Bump();
            }
            else if(category == "EditorPrefs")
            {
                Preferences.PreferenceMonitor.InstanceOfEditorPrefs.Bump();
            }
            else if(category == "InputManager")
            {
                Input.InputManagerMonitor.Instance.Bump();
            }
            else
            {
                Serilog.Log.Error("Invalid LazyLoad category {Category}", category);
            }
        }
    }

}