using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;


#nullable enable


namespace Meryel.UnityCodeAssist.Editor.Preferences
{
    public class PreferenceMonitor 
    {
        private static readonly Lazy<PreferenceMonitor> _instanceOfPlayerPrefs = new Lazy<PreferenceMonitor>(() => new PreferenceMonitor(true));
        private static readonly Lazy<PreferenceMonitor> _instanceOfEditorPrefs = new Lazy<PreferenceMonitor>(() => new PreferenceMonitor(false));
        public static PreferenceMonitor InstanceOfPlayerPrefs => _instanceOfPlayerPrefs.Value;
        public static PreferenceMonitor InstanceOfEditorPrefs => _instanceOfEditorPrefs.Value;

        //const int Limit = 128;
        const int Limit = 8192;

        /// <summary>
        /// PlayerPrefs or EditorPrefs
        /// </summary>
        readonly bool isPlayerPrefs;

#region ErrorValues
        private readonly int ERROR_VALUE_INT = int.MinValue;
        private readonly string ERROR_VALUE_STR = "<UCA_ERR_2407201713>";
#endregion //ErrorValues

#pragma warning disable CS0414
        private static string pathToPrefs = String.Empty;
        private static string platformPathPrefix = @"~";
#pragma warning restore CS0414

        //private string[] userDef;
        //private string[] unityDef;
        //private bool showSystemGroup = false;

        private SerializedObject? serializedObject;
        private ReorderableList? userDefList;
        private ReorderableList? unityDefList;

        private PreferenceEntryHolder? prefEntryHolder;

        private PreferanceStorageAccessor? entryAccessor;


        private bool updateView = false;
        //private bool monitoring = false;
        //private bool showLoadingIndicatorOverlay = false;


#if UNITY_EDITOR_LINUX
        private readonly char[] invalidFilenameChars = { '"', '\\', '*', '/', ':', '<', '>', '?', '|' };
#elif UNITY_EDITOR_OSX
        private readonly char[] invalidFilenameChars = { '$', '%', '&', '\\', '/', ':', '<', '>', '|', '~' };
#endif



        PreferenceMonitor(bool isPlayerPrefs)
        {
            this.isPlayerPrefs = isPlayerPrefs;
            OnEnable();
            EditorApplication.update += Update;
        }

        ~PreferenceMonitor()
        {
            OnDisable();
        }

        public void Bump()
        {
            Serilog.Log.Debug("Bumping preference {IsPlayerPrefs}", isPlayerPrefs);
            
            RetrieveAndSendKeysAndValues(false);
        }

        private void RetrieveAndSendKeysAndValues(bool reloadKeys)
        {
            string[]? keys = GetKeys(reloadKeys);
            if (keys == null)
                return;
            string[] values = GetKeyValues(reloadKeys, keys, out var stringKeys, out var integerKeys, out var floatKeys, out var booleanKeys);

            if (isPlayerPrefs)
                NetMQInitializer.Publisher?.SendPlayerPrefs(keys, values, stringKeys, integerKeys, floatKeys);
            else
                NetMQInitializer.Publisher?.SendEditorPrefs(keys, values, stringKeys, integerKeys, floatKeys, booleanKeys);
        }

        private void OnEnable()
        {
#if UNITY_EDITOR_WIN
            if (isPlayerPrefs)
                pathToPrefs = @"SOFTWARE\Unity\UnityEditor\" + PlayerSettings.companyName + @"\" + PlayerSettings.productName;
            else
                pathToPrefs = @"Software\Unity Technologies\Unity Editor 5.x";
            
            platformPathPrefix = @"<CurrentUser>";
            entryAccessor = new WindowsPrefStorage(pathToPrefs);
#elif UNITY_EDITOR_OSX
            if (isPlayerPrefs)
                pathToPrefs = @"Library/Preferences/com." + MakeValidFileName(PlayerSettings.companyName) + "." + MakeValidFileName(PlayerSettings.productName) + ".plist";
            else
                pathToPrefs = @"Library/Preferences/com.unity3d.UnityEditor5.x.plist";
                
            platformPathPrefix = @"~";
            entryAccessor = new MacPrefStorage(pathToPrefs);
            //entryAccessor.StartLoadingDelegate = () => { showLoadingIndicatorOverlay = true; };
            //entryAccessor.StopLoadingDelegate = () => { showLoadingIndicatorOverlay = false; };
#elif UNITY_EDITOR_LINUX
            if (isPlayerPrefs)
                pathToPrefs = @".config/unity3d/" + MakeValidFileName(PlayerSettings.companyName) + "/" + MakeValidFileName(PlayerSettings.productName) + "/prefs";
            else
                pathToPrefs = @".local/share/unity3d/prefs";

            platformPathPrefix = @"~";
            entryAccessor = new LinuxPrefStorage(pathToPrefs);
#else
            Serilog.Log.Warning("Undefined Unity Editor platform");
            pathToPrefs = String.Empty;
            platformPathPrefix = @"~";
            entryAccessor = null;
#endif

            if (entryAccessor != null)
            {
                entryAccessor.PrefEntryChangedDelegate = () => { updateView = true; };
                entryAccessor.StartMonitoring();
            }
        }

        // Handel view updates for monitored changes
        // Necessary to avoid main thread access issue
        private void Update()
        {
            if (updateView)
            {
                updateView = false;
                //PrepareData();
                //Repaint();

                Serilog.Log.Debug("Updating preference {IsPlayerPrefs}", isPlayerPrefs);

                RetrieveAndSendKeysAndValues(true);
            }
        }

        private void OnDisable()
        {
            entryAccessor?.StopMonitoring();
        }

        private void InitReorderedList()
        {
            if (prefEntryHolder == null)
            {
                var tmp = Resources.FindObjectsOfTypeAll<PreferenceEntryHolder>();
                if (tmp.Length > 0)
                {
                    prefEntryHolder = tmp[0];
                }
                else
                {
                    prefEntryHolder = ScriptableObject.CreateInstance<PreferenceEntryHolder>();
                }
            }


            serializedObject ??= new SerializedObject(prefEntryHolder);

            userDefList = new ReorderableList(serializedObject, serializedObject.FindProperty("userDefList"), false, true, true, true);
            unityDefList = new ReorderableList(serializedObject, serializedObject.FindProperty("unityDefList"), false, true, false, false);

        }


        

        private string[]? GetKeys(bool reloadKeys)
        {
            if (entryAccessor == null)
            {
                Serilog.Log.Warning($"{nameof(entryAccessor)} is null");
                return null;
            }

            string[] keys = entryAccessor.GetKeys(reloadKeys);

            if (keys.Length > Limit)
                keys = keys.Where(k => !k.StartsWith("unity.") && !k.StartsWith("UnityGraphicsQuality")).Take(Limit).ToArray();

            return keys;
        }

        string[]? _cachedKeyValues = null;

        string[]? _cachedStringKeys = null;
        string[]? _cachedIntegerKeys = null;
        string[]? _cachedFloatKeys = null;
        string[]? _cachedBooleanKeys = null;

        private string[] GetKeyValues(bool reloadData, string[] keys,
            out string[] stringKeys, out string[] integerKeys, out string[] floatKeys, out string[] booleanKeys)
        {
            if (!reloadData && _cachedKeyValues != null && _cachedKeyValues.Length == keys.Length)
            {
                stringKeys = _cachedStringKeys!;
                integerKeys = _cachedIntegerKeys!;
                floatKeys = _cachedFloatKeys!;
                booleanKeys = _cachedBooleanKeys!;
                return _cachedKeyValues;
            }

            string[] values = new string[keys.Length];
            var stringKeyList = new List<string>();
            var integerKeyList = new List<string>();
            var floatKeyList = new List<string>();
            var boolenKeyList = new List<string>();

            for (int i = 0; i < keys.Length; i++)
            {
                var key = keys[i];

                string stringValue;
                if (isPlayerPrefs)
                    stringValue = PlayerPrefs.GetString(key, ERROR_VALUE_STR);
                else
                    stringValue = EditorPrefs.GetString(key, ERROR_VALUE_STR);

                if (stringValue != ERROR_VALUE_STR)
                {
                    values[i] = stringValue;
                    stringKeyList.Add(key);
                    continue;
                }

                float floatValue;
                if (isPlayerPrefs)
                    floatValue = PlayerPrefs.GetFloat(key, float.NaN);
                else
                    floatValue = EditorPrefs.GetFloat(key, float.NaN);

                if (!float.IsNaN(floatValue))
                {
                    values[i] = floatValue.ToString();
                    floatKeyList.Add(key);
                    continue;
                }

                int intValue;
                if (isPlayerPrefs)
                    intValue = PlayerPrefs.GetInt(key, ERROR_VALUE_INT);
                else
                    intValue = EditorPrefs.GetInt(key, ERROR_VALUE_INT);

                if (intValue != ERROR_VALUE_INT)
                {
                    values[i] = intValue.ToString();
                    integerKeyList.Add(key);
                    continue;
                }

                bool boolValue = false;
                if (!isPlayerPrefs)
                {
                    bool boolValueTrue = EditorPrefs.GetBool(key, true);
                    bool boolValueFalse = EditorPrefs.GetBool(key, false);

                    boolValue = boolValueFalse;
                    if (boolValueTrue == boolValueFalse)
                    {
                        values[i] = boolValueTrue.ToString();
                        boolenKeyList.Add(key);
                        continue;
                    }
                }

                values[i] = string.Empty;
                if (isPlayerPrefs)
                {
                    // Keys with ? causing problems, just ignore them
                    if (key.Contains("?"))
                        Serilog.Log.Debug("Invalid {PreferenceType} KEY WITH '?', '{Key}' at {Location}, str:{StringValue}, int:{IntegerValue}, float:{FloatValue}, bool:{BooleanValue}",
                            (isPlayerPrefs ? "PlayerPrefs" : "EditorPrefs"), key, nameof(GetKeyValues),
                            stringValue, intValue, floatValue, boolValue);

                    else
                        // EditorPrefs gives error for some keys
                        Serilog.Log.Error("Invalid {PreferenceType} '{Key}' at {Location}, str:{StringValue}, int:{IntegerValue}, float:{FloatValue}, bool:{BooleanValue}",
                            (isPlayerPrefs ? "PlayerPrefs" : "EditorPrefs"), key, nameof(GetKeyValues),
                            stringValue, intValue, floatValue, boolValue);
                }
            }

            stringKeys = stringKeyList.ToArray();
            integerKeys = integerKeyList.ToArray();
            floatKeys = floatKeyList.ToArray();
            booleanKeys = boolenKeyList.ToArray();

            _cachedKeyValues = values;

            _cachedStringKeys = stringKeys;
            _cachedIntegerKeys = integerKeys;
            _cachedFloatKeys = floatKeys;
            _cachedBooleanKeys = booleanKeys;

            return values;
        }

        private void LoadKeys(out string[]? userDef, out string[]? unityDef, bool reloadKeys)
        {
            if(entryAccessor == null)
            {
                userDef = null;
                unityDef = null;
                return;
            }

            string[] keys = entryAccessor.GetKeys(reloadKeys);

            //keys.ToList().ForEach( e => { Debug.Log(e); } );

            // Seperate keys int unity defined and user defined
            Dictionary<bool, List<string>> groups = keys
                .GroupBy((key) => key.StartsWith("unity.") || key.StartsWith("UnityGraphicsQuality"))
                .ToDictionary((g) => g.Key, (g) => g.ToList());

            unityDef = (groups.ContainsKey(true)) ? groups[true].ToArray() : new string[0];
            userDef = (groups.ContainsKey(false)) ? groups[false].ToArray() : new string[0];
        }


#if (UNITY_EDITOR_LINUX || UNITY_EDITOR_OSX)
        private string MakeValidFileName(string unsafeFileName)
        {
            string normalizedFileName = unsafeFileName.Trim().Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            // We need to use a TextElementEmumerator in order to support UTF16 characters that may take up more than one char(case 1169358)
            TextElementEnumerator charEnum = StringInfo.GetTextElementEnumerator(normalizedFileName);
            while (charEnum.MoveNext())
            {
                string c = charEnum.GetTextElement();
                if (c.Length == 1 && invalidFilenameChars.Contains(c[0]))
                {
                    stringBuilder.Append('_');
                    continue;
                }
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c, 0);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
#endif

    }


}
