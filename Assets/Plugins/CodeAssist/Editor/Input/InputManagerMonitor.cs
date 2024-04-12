using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


#nullable enable


namespace Meryel.UnityCodeAssist.Editor.Input
{

    public class InputManagerMonitor
    {
        private static readonly Lazy<InputManagerMonitor> _instance = new Lazy<InputManagerMonitor>(() => new InputManagerMonitor());
        public static InputManagerMonitor Instance => _instance.Value;

        //UnityInputManager inputManager;
        readonly string inputManagerFilePath;
        DateTime previousTagManagerLastWrite;

        public InputManagerMonitor()
        {
            EditorApplication.update += Update;
            inputManagerFilePath = CommonTools.GetInputManagerFilePath();

            try
            {
                previousTagManagerLastWrite = System.IO.File.GetLastWriteTime(inputManagerFilePath);
            }
            catch (Exception ex)
            {
                Serilog.Log.Debug(ex, "Exception at {Location}", nameof(System.IO.File.GetLastWriteTime));
            }
        }

        void Update()
        {
#if !ENABLE_LEGACY_INPUT_MANAGER
            return;
#endif

#pragma warning disable CS0162
#pragma warning disable IDE0035

            var currentInputManagerLastWrite = previousTagManagerLastWrite;
            try
            {
                currentInputManagerLastWrite = System.IO.File.GetLastWriteTime(inputManagerFilePath);
            }
            catch (Exception ex)
            {
                Serilog.Log.Debug(ex, "Exception at {Location}", nameof(System.IO.File.GetLastWriteTime));
            }
            if (currentInputManagerLastWrite != previousTagManagerLastWrite)
            {
                previousTagManagerLastWrite = currentInputManagerLastWrite;
                Bump();
            }

#pragma warning restore CS0162
#pragma warning restore IDE0035
        }

        public void Bump()
        {
#if !ENABLE_LEGACY_INPUT_MANAGER
            return;
#endif
#pragma warning disable CS0162
#pragma warning disable IDE0035

            Serilog.Log.Debug("InputMonitor {Event}", nameof(Bump));

            var inputManager = new UnityInputManager();
            inputManager.ReadFromPath(inputManagerFilePath);
            inputManager.SendData();


#pragma warning restore CS0162
#pragma warning restore IDE0035
        }

    }


    public static partial class Extensions
    {
        public static string GetInfo(this List<InputAxis> axes, string? name)
        {
            if (name == null || string.IsNullOrEmpty(name))
                return string.Empty;

            //axis.descriptiveName
            var axesWithName = axes.Where(a => a.Name == name);

            int threshold = 80;

            var sb = new System.Text.StringBuilder();

            foreach (var axis in axesWithName)
                if (!string.IsNullOrEmpty(axis.descriptiveName))
                    sb.Append($"{axis.descriptiveName} ");

            if (sb.Length > threshold)
                return sb.ToString();

            foreach (var axis in axesWithName)
                if (!string.IsNullOrEmpty(axis.descriptiveNegativeName))
                    sb.Append($"{axis.descriptiveNegativeName} ");

            if (sb.Length > threshold)
                return sb.ToString();

            foreach (var axis in axesWithName)
                if (!string.IsNullOrEmpty(axis.positiveButton))
                    sb.Append($"[{axis.positiveButton}] ");

            if (sb.Length > threshold)
                return sb.ToString();

            foreach (var axis in axesWithName)
                if (!string.IsNullOrEmpty(axis.altPositiveButton))
                    sb.Append($"{{{axis.altPositiveButton}}} ");

            if (sb.Length > threshold)
                return sb.ToString();

            foreach (var axis in axesWithName)
                if (!string.IsNullOrEmpty(axis.negativeButton))
                    sb.Append($"-[{axis.negativeButton}] ");

            if (sb.Length > threshold)
                return sb.ToString();

            foreach (var axis in axesWithName)
                if (!string.IsNullOrEmpty(axis.altNegativeButton))
                    sb.Append($"-{{{axis.altNegativeButton}}} ");

            return sb.ToString();
        }
    }

}
