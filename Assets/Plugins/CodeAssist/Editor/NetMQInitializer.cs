using System;
using UnityEngine;
using UnityEditor;


#nullable enable


namespace Meryel.UnityCodeAssist.Editor
{
    [InitializeOnLoad]
    public static class NetMQInitializer
    {
        public static NetMQPublisher? Publisher;
        
        static NetMQInitializer()
        {
            EditorApplication.quitting += EditorApplication_quitting;
            AssemblyReloadEvents.beforeAssemblyReload += AssemblyReloadEvents_beforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += AssemblyReloadEvents_afterAssemblyReload;

            RunOnceOnUpdate(Initialize);
        }

        public static void Initialize()
        {
            Serilog.Log.Debug("NetMQ initializing");

            AsyncIO.ForceDotNet.Force();

            Serilog.Log.Debug("NetMQ cleaning up (true)");
            NetMQ.NetMQConfig.Cleanup(true);

            Serilog.Log.Debug("NetMQ constructing");
            Publisher = new NetMQPublisher();
            
            RunOnShutdown(OnShutDown);
            Serilog.Log.Debug("NetMQ initialized");
        }

        private static void OnShutDown()
        {
            Serilog.Log.Debug("NetMQ OnShutDown");
            Clear();
        }

        private static void AssemblyReloadEvents_afterAssemblyReload()
        {
            Serilog.Log.Debug("NetMQ AssemblyReloadEvents_afterAssemblyReload");
        }

        //private static void AssemblyReloadEvents_beforeAssemblyReload() => Clear();
        private static void AssemblyReloadEvents_beforeAssemblyReload()
        {
            Serilog.Log.Debug("NetMQ AssemblyReloadEvents_beforeAssemblyReload");

            Clear();
        }

        private static void EditorApplication_quitting()
        {
            Serilog.Log.Debug("NetMQ EditorApplication_quitting");

            Publisher?.SendDisconnect();
            Clear();
        }

        static void Clear() => Publisher?.Clear();


        private static void RunOnceOnUpdate(Action action)
        {
            void callback()
            {
                EditorApplication.update -= callback;
                action();
            }

            EditorApplication.update += callback;
        }

        private static void RunOnShutdown(Action action)
        {
            // Mono on OSX has all kinds of quirks on AppDomain shutdown
            //if (!VisualStudioEditor.IsWindows)
            //return;
#if !UNITY_EDITOR_WIN
            return;
#else
            AppDomain.CurrentDomain.DomainUnload += (_, __) => action();
#endif

        }
    }
}
