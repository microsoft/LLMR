using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Security.Cryptography;


#nullable enable


namespace Meryel.UnityCodeAssist.Editor.Input
{
    

    internal class UnityInputManager
    {
        //string yamlPath;
        TextReader? reader;
        InputManager? inputManager;

        public void ReadFromText(string text)
        {
            reader = new StringReader(text);
            ReadAux(false, out _);
        }

        public void ReadFromPath(string yamlPath)
        {

            switch (UnityEditor.EditorSettings.serializationMode)
            {
                case UnityEditor.SerializationMode.ForceText:
                    {
                        reader = new StreamReader(yamlPath);
                        ReadAux(false, out _);
                    }
                    break;

                case UnityEditor.SerializationMode.ForceBinary:
                    {
                        // this approach will work for InputManager since its file size is small and limited
                        // but in the future, we may need to switch to reading binary files for big files
                        // like this https://github.com/Unity-Technologies/UnityDataTools
                        // or this https://github.com/SeriousCache/UABE
                        var converted = GetOrCreateConvertedFile(yamlPath);
                        if (!File.Exists(converted))
                        {
                            Serilog.Log.Warning("Temp file {TempFile} couldn't found for converted yaml input file. Auto Input Manager will not work!", converted);
                            return;
                        }
                        var rawLines = File.ReadLines(converted);
                        var yamlText = Text2Yaml.Convert(rawLines);
                        reader = new StringReader(yamlText);
                        ReadAux(false, out _);
                    }
                    break;

                case UnityEditor.SerializationMode.Mixed:
                    {
                        reader = new StreamReader(yamlPath);
                        ReadAux(true, out var hasSemanticError);
                        if (hasSemanticError)
                        {
                            var converted = GetOrCreateConvertedFile(yamlPath);
                            if (!File.Exists(converted))
                            {
                                Serilog.Log.Warning("Temp file {TempFile} couldn't found for converted yaml input file. Auto Input Manager will not work!", converted);
                                return;
                            }
                            var rawLines = File.ReadLines(converted);
                            var yamlText = Text2Yaml.Convert(rawLines);
                            reader = new StringReader(yamlText);
                            ReadAux(false, out _);
                        }
                    }
                    break;
                
            }
        }


        void ReadAux(bool canHaveSemanticError, out bool hasSemanticError)
        {
            hasSemanticError = false;

            if (reader == null)
            {
                Serilog.Log.Warning($"{nameof(UnityInputManager)}.{nameof(reader)} is null");
                return;
            }

            //var reader = new StreamReader(yamlPath);
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithTagMapping("tag:unity3d.com,2011:13", typeof(Class13Mapper))
                .IgnoreUnmatchedProperties()
                .Build();
            //serializer.Settings.RegisterTagMapping("tag:unity3d.com,2011:13", typeof(Class13));
            //serializer.Settings.ComparerForKeySorting = null;
            Class13Mapper? result;
            try
            {
                result = deserializer.Deserialize<Class13Mapper>(reader);
            }
            catch (YamlDotNet.Core.SemanticErrorException semanticErrorException)
            {
                Serilog.Log.Debug(semanticErrorException, "Couldn't parse InputManager.asset yaml file");
                if (!canHaveSemanticError)
                    Serilog.Log.Error(semanticErrorException, "Couldn't parse InputManager.asset yaml file unexpectedly");

                hasSemanticError = true;
                return;
            }
            finally
            {
                reader.Close();
            }

            var inputManagerMapper = result?.InputManager;
            if (inputManagerMapper == null)
            {
                Serilog.Log.Warning($"{nameof(inputManagerMapper)} is null");
                return;
            }

            inputManager = new InputManager(inputManagerMapper);
        }


        public void SendData()
        {
            if (inputManager == null)
                return;

            var axisNames = inputManager.Axes.Select(a => a.Name!).Where(n => !string.IsNullOrEmpty(n)).Distinct().ToArray();
            var axisInfos = axisNames.Select(a => inputManager.Axes.GetInfo(a)).ToArray();
            if (!CreateBindingsMap(out var buttonKeys, out var buttonAxis))
                return;

            string[] joystickNames;
            try
            {
                joystickNames = UnityEngine.Input.GetJoystickNames();
            }
            catch (InvalidOperationException)
            {
                // Occurs if user have switched active Input handling to Input System package in Player Settings.
                joystickNames = new string[0];
            }

            NetMQInitializer.Publisher?.SendInputManager(axisNames, axisInfos, buttonKeys, buttonAxis, joystickNames);

            /*
            NetMQInitializer.Publisher?.SendInputManager(
                inputManager.Axes.Select(a => a.Name).Distinct().ToArray(),
                inputManager.Axes.Select(a => a.positiveButton).ToArray(),
                inputManager.Axes.Select(a => a.negativeButton).ToArray(),
                inputManager.Axes.Select(a => a.altPositiveButton).ToArray(),
                inputManager.Axes.Select(a => a.altNegativeButton).ToArray(),
                UnityEngine.Input.GetJoystickNames()
                );
            */

        }


        bool CreateBindingsMap([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string[]? inputKeys, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]  out string[]? inputAxis)
        {
            if (inputManager == null)
            {
                inputKeys = null;
                inputAxis = null;
                return false;
            }

            var dict = new Dictionary<string, string?>();

            foreach (var axis in inputManager.Axes)
            {
                if (axis.altNegativeButton != null && !string.IsNullOrEmpty(axis.altNegativeButton))
                    dict[axis.altNegativeButton] = axis.Name;
            }
            foreach (var axis in inputManager.Axes)
            {
                if (axis.negativeButton != null && !string.IsNullOrEmpty(axis.negativeButton))
                    dict[axis.negativeButton] = axis.Name;
            }
            foreach (var axis in inputManager.Axes)
            {
                if (axis.altPositiveButton != null && !string.IsNullOrEmpty(axis.altPositiveButton))
                    dict[axis.altPositiveButton] = axis.Name;
            }
            foreach (var axis in inputManager.Axes)
            {
                if (axis.positiveButton != null && !string.IsNullOrEmpty(axis.positiveButton))
                    dict[axis.positiveButton] = axis.Name;
            }

            var keys = new string[dict.Count];
            var values = new string[dict.Count];
            dict.Keys.CopyTo(keys, 0);
            dict.Values.CopyTo(values, 0);

            inputKeys = keys;
            inputAxis = values;
            return true;
        }



        static string GetOrCreateConvertedFile(string filePath)
        {
            var hash = GetMD5Hash(filePath);
            var convertedPath = Path.Combine(Path.GetTempPath(), $"UCA_IM_{hash}.txt");

            if (!File.Exists(convertedPath))
            {
                Serilog.Log.Debug("Converting binary to text format of {File} to {Target}", filePath, convertedPath);
                var converter = new Binary2TextExec();
                converter.Exec(filePath, convertedPath);
            }
            else
            {
                Serilog.Log.Debug("Converted file already exists at {Target}", convertedPath);
            }    

            return convertedPath;
        }

        /// <summary>
        /// Gets a hash of the file using MD5.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetMD5Hash(string filePath)
        {
            using var md5 = new MD5CryptoServiceProvider();
            return GetHash(filePath, md5);
        }

        /// <summary>
        /// Gets a hash of the file using MD5.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetMD5Hash(Stream s)
        {
            using var md5 = new MD5CryptoServiceProvider();
            return GetHash(s, md5);
        }

        private static string GetHash(string filePath, HashAlgorithm hasher)
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return GetHash(fs, hasher);
        }

        private static string GetHash(Stream s, HashAlgorithm hasher)
        {
            var hash = hasher.ComputeHash(s);
            var hashStr = Convert.ToBase64String(hash);
            //return hashStr.TrimEnd('=');
            var hashStrAlphaNumeric = System.Text.RegularExpressions.Regex.Replace(hashStr, "[^A-Za-z0-9]", "");
            return hashStrAlphaNumeric;
        }

    }

    public enum AxisType
    {
        KeyOrMouseButton = 0,
        MouseMovement = 1,
        JoystickAxis = 2
    };

#pragma warning disable IDE1006

    public class InputAxisMapper
    {
        public int serializedVersion { get; set; }

        public string? m_Name { get; set; }
        public string? descriptiveName { get; set; }
        public string? descriptiveNegativeName { get; set; }
        public string? negativeButton { get; set; }
        public string? positiveButton { get; set; }
        public string? altNegativeButton { get; set; }
        public string? altPositiveButton { get; set; }

        //public float gravity { get; set; }
        //public float dead { get; set; }
        //public float sensitivity { get; set; }
        public string? gravity { get; set; }
        public string? dead { get; set; }
        public string? sensitivity { get; set; }

        //public bool snap { get; set; }
        public int snap { get; set; }
        //public bool invert { get; set; }
        public int invert { get; set; }

        //public AxisType type { get; set; }
        public int type { get; set; }

        public int axis { get; set; }
        public int joyNum { get; set; }
    }

    public class InputAxis
    {
        readonly InputAxisMapper map;

        public InputAxis(InputAxisMapper map)
        {
            this.map = map;
        }

        public int SerializedVersion
        {
            get { return map.serializedVersion; }
            set { map.serializedVersion = value; }
        }

        public string? Name => map.m_Name;
        public string? descriptiveName => map.descriptiveName;
        public string? descriptiveNegativeName => map.descriptiveNegativeName;
        public string? negativeButton => map.negativeButton;
        public string? positiveButton => map.positiveButton;
        public string? altNegativeButton => map.altNegativeButton;
        public string? altPositiveButton => map.altPositiveButton;

        public float gravity => float.Parse(map.gravity);//**--format
        public float dead => float.Parse(map.dead);//**--format
        public float sensitivity => float.Parse(map.sensitivity);//**--format

        public bool snap => map.snap != 0;
        public bool invert => map.invert != 0;

        public AxisType type => (AxisType)map.type;

        public int axis => map.axis;
        public int joyNum => map.joyNum;
    }

    public class InputManagerMapper
    {
        public int m_ObjectHideFlags { get; set; }
        public int serializedVersion { get; set; }
        public int m_UsePhysicalKeys { get; set; }
        public List<InputAxisMapper>? m_Axes { get; set; }
    }

#pragma warning restore IDE1006

    public class InputManager
    {
        readonly InputManagerMapper map;
        readonly List<InputAxis> axes;

        public InputManager(InputManagerMapper map)
        {
            this.map = map;
            this.axes = new List<InputAxis>();

            if (map.m_Axes == null)
            {
                Serilog.Log.Warning($"map.m_Axes is null");
                return;
            }

            foreach (var a in map.m_Axes)
                this.axes.Add(new InputAxis(a));
        }

        public int ObjectHideFlags
        {
            get { return map.m_ObjectHideFlags; }
            set { map.m_ObjectHideFlags = value; }
        }

        public int SerializedVersion
        {
            get { return map.serializedVersion; }
            set { map.serializedVersion = value; }
        }

        public bool UsePhysicalKeys
        {
            get { return map.m_UsePhysicalKeys != 0; }
            set { map.m_UsePhysicalKeys = value ? 1 : 0; }
        }

        /*public List<InputAxisMapper> Axes
        {
            get { return map.m_Axes; }
            set { map.m_Axes = value; }
        }*/
        public List<InputAxis> Axes => axes;
    }

    public class Class13Mapper
    {
        public InputManagerMapper? InputManager { get; set; }
    }
}
