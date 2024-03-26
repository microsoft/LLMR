using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;


#nullable enable


namespace Meryel.UnityCodeAssist.Editor
{

    public class ScriptFinder //: MonoBehaviour
    {

        static Type? GetType123(string typeName)
        {
            //**--
            //**--
            /*
             * for performance,
             * check assembly-csharp, assembly-csharp-editor, assembly-csharp-first-pass,assembly-csharp-editor-first-pass
             * first, (then maybe asmdef dlls), then check mscorlib and other referenced dlls
             */


            //**--use typecache???
            //TypeCache

            //**--check this again
            //https://github.com/Unity-Technologies/SuperScience/blob/main/Editor/GlobalNamespaceWatcher.cs

            // Try Type.GetType() first. This will work with types defined
            // by the Mono runtime, in the same assembly as the caller, etc.
            Type type = Type.GetType(typeName);

            // If it worked, then we're done here
            if (type != null)
            {
                return type;
            }

            // Attempt to search for type on the loaded assemblies
            Assembly[] currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in currentAssemblies)
            {
                type = assembly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            // If we still haven't found the proper type, we can enumerate all of the
            // loaded assemblies and see if any of them define the type
            var currentAssembly = Assembly.GetExecutingAssembly();
            var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
            foreach (var assemblyName in referencedAssemblies)
            {
                // Load the referenced assembly
                var assembly = Assembly.Load(assemblyName);
                if (assembly != null)
                {
                    // See if that assembly defines the named type
                    type = assembly.GetType(typeName);
                    if (type != null)
                    {
                        return type;
                    }
                }
            }

            // The type just couldn't be found...
            return null;
        }

        public static bool FindInstanceOfType(string typeName, string docPath, out GameObject? gameObjectInstanceOfType, out ScriptableObject? scriptableObjectInstanceOfType)
        {
            gameObjectInstanceOfType = null;
            scriptableObjectInstanceOfType = null;

            var type = GetType123(typeName);

            if (type == null)
            {
                // Possibly a class has been created in Visual Studio, and these changes are not reflected in Unity domain yet
                // We can force Unity to recompile and get the type, but since there will be no instance of that type, it won't be of any use, will be just a performance burden
                Serilog.Log.Debug("{Type} type couldn't be found", typeName);
                return false;
            }


            var obj = GetObjectOfType(type, out var requestVerboseType);
            if (requestVerboseType)
                NetMQInitializer.Publisher?.SendRequestVerboseType(typeName, docPath);

            if (obj != null && obj is GameObject go)
            {
                gameObjectInstanceOfType = go;
                return true;
            }
            else if (obj != null && obj is ScriptableObject so)
            {
                scriptableObjectInstanceOfType = so;
                return true;
            }

            Serilog.Log.Debug("Instance of {Type} type couldn't be found", typeName);
            return false;
        }

        static UnityEngine.Object? GetObjectOfType(Type type, out bool requestVerboseType)
        {
            requestVerboseType = false;
            var isMonoBehaviour = type.IsSubclassOf(typeof(MonoBehaviour));
            var isScriptableObject = type.IsSubclassOf(typeof(ScriptableObject));

            if (!isMonoBehaviour && !isScriptableObject)
            {
                // Possibly a class's base class changed from none to MonoBehaviour in Visual Studio, and these changes are not reflected in Unity domain yet
                // We can force Unity to recompile and get the type correctly, but since there will be no instance of that type, it won't be of any use, will be just a performance burden
                Serilog.Log.Debug("{Type} is not a valid Unity object", type.ToString());
                //requestVerboseType = true;
                return null;
            }

            UnityEngine.Object? obj;

            obj = getObjectToSend(Selection.activeGameObject, type);
            if (obj != null)
                return obj;


            obj = getObjectToSend(Selection.activeTransform, type);
            if (obj != null)
                return obj;


            obj = getObjectToSend(Selection.activeObject, type);
            if (obj != null)
                return obj;
            

            //**--check source code of this, for sorting
            var filteredArray = Selection.GetFiltered(type, SelectionMode.Unfiltered);
            if (filteredArray != null)
            {
                //**--sort
                foreach (var filtered in filteredArray)
                {
                    obj = getObjectToSend(filtered, type);
                    if (obj != null)
                        return obj;
                }
            }



            //**--rest can be slow, try avoiding them, make own db etc
            //**--can add a stop-wacher and add warning if slow as well
            //**--can also cache the result

            try
            {
                // UnityEngine.Object.FindObjectOfType is deprecated in new versions of Unity
#if UNITY_2022_3 || UNITY_2023_1_OR_NEWER
                // Object.FindAnyObjectOfType doesn't return Assets (for example meshes, textures, or prefabs), or inactive objects. It also doesn't return objects that have HideFlags.DontSave set.
                obj = UnityEngine.Object.FindAnyObjectByType(type);
#else
                // Object.FindObjectOfType will not return Assets (meshes, textures, prefabs, ...) or inactive objects. It will not return an object that has HideFlags.DontSave set.
                obj = UnityEngine.Object.FindObjectOfType(type);
#endif
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning(ex, "FindObjectOfType/FindAnyObjectByType failed for {Type}, mb:{isMB}, so:{isSO}", type.ToString(), isMonoBehaviour, isScriptableObject);
            }

            obj = getObjectToSend(obj, type);
            if (obj != null)
                return obj;

            UnityEngine.Object[]? arr = null;
            try
            {
                // This function can return any type of Unity object that is loaded, including game objects, prefabs, materials, meshes, textures, etc.
                // Contrary to Object.FindObjectsOfType this function will also list disabled objects.
                arr = Resources.FindObjectsOfTypeAll(type);
            }
            catch (Exception ex)
            {
                //var isMonoBehaviour = type.IsSubclassOf(typeof(MonoBehaviour));
                //var isScriptableObject = type.IsSubclassOf(typeof(ScriptableObject));
                Serilog.Log.Warning(ex, "FindObjectsOfTypeAll failed for {Type}, mb:{isMB}, so:{isSO}", type.ToString(), isMonoBehaviour, isScriptableObject);
            }

            if (arr != null)
            {
                //**--sort
                foreach (var item in arr)
                {
                    obj = getObjectToSend(item, type);
                    if (obj != null)
                        return obj;
                }
            }


            return obj;


            static UnityEngine.Object? getObjectToSend(UnityEngine.Object? obj, Type type)
            {
                if (obj == null || !obj)
                    return null;

                if (obj is GameObject go)
                {
                    if (!go)
                        return null;
                    if (isTypeComponent(type) && go.GetComponent(type) != null)
                        return go;
                }
                else if (obj is Transform transform)
                {
                    go = transform.gameObject;
                    if (!go)
                        return null;
                    if (isTypeComponent(type) && go.GetComponent(type) != null)
                        return go;
                }
                else if (obj is Component comp)
                {
                    go = comp.gameObject;
                    if (!go)
                        return null;
                    else
                        return go;
                }
                else if (obj is ScriptableObject so)
                {
                    if (!so)
                        return null;
                    else
                        return so;
                }

                return null;
            }

            static bool isTypeComponent(Type type)
            {
                var componentType = typeof(Component);//**--cache these types
                if (type == componentType || type.IsSubclassOf(componentType))
                    return true;

                // MonoBehaviour is Component, so below is unnecessary
                //var monoBehaviourType = typeof(MonoBehaviour);
                //if (type == monoBehaviourType || type.IsSubclassOf(monoBehaviourType))
                //    return true;
                
                //else if(type is interface)//**--

                return false;
            }
        }

        public static void DENEMEEEE()
        {
            //UnityEditor.SceneManagement.EditorSceneManager.all
            //AssetDatabase.get

            foreach (var sceneGUID in AssetDatabase.FindAssets("t:Scene", new string[] { "Assets" }))
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(sceneGUID);
                Debug.Log("scenePath: " + scenePath);

                //EditorSceneManager.OpenScene(scenePath);
                //var scene = EditorSceneManager.GetActiveScene();
            }

            var assets = AssetDatabase.FindAssets("Deneme_OtherScene");
            Debug.Log("Assets: " + assets.Length);

            foreach (var assetGuid in assets)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                Debug.Log("Asset: " + assetGuid + " " + assetPath);


            }
        }

        public static bool GetActiveGameObject(out GameObject activeGameObject)
        {
            activeGameObject = Selection.activeGameObject;
            return activeGameObject ? true : false;
        }

    }


}