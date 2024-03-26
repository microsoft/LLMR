using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;


#nullable enable


//namespace UTJ.UnityCommandLineTools
namespace Meryel.UnityCodeAssist.Editor.Input
{
    // <summary>
    // bin2textをUnityEditorから実行する為のClass
    // programed by Katsumasa.Kimura
    // </summary>
    public class Binary2TextExec : EditorToolExec
    {
        public Binary2TextExec() : base("binary2text") { }

        // <summary>
        // bin2text filePath outPath options
        // /summary>
        public int Exec(string filePath, string outPath, string options)
        {
            var args = string.Format(@"""{0}"" ""{1}"" {2}", filePath, outPath, options);
            return Exec(args);
        }

        public int Exec(string filePath, string outPath, bool detailed = false, bool largeBinaryHashOnly = false, bool hexFloat = false)
        {
            //var args = string.Format(@"""{0}"" ""{1}"" {2}", filePath, outPath, options);
            var args = string.Format(@"""{0}"" ""{1}""", filePath, outPath);

            if (detailed)
                args += " -detailed";
            if (largeBinaryHashOnly)
                args += " -largebinaryhashonly";
            if (hexFloat)
                args += " -hexfloat";

            return Exec(args);
        }
    }

    // <summary>
    // UnityEditorに含まれるコマンドラインツールを実行する為の基底Class
    // programed by Katsumasa.Kimura
    //</summary>
    public class EditorToolExec
    {
        // <value>
        // UnityEditorがインストールされているディレクトリへのパス
        // </value>
        protected string mEditorPath;

        // <value>
        // Toolsディレクトリへのパス
        // </value>
        protected string mToolsPath;

        // <value>
        // 実行ファイル名
        // </value>
        protected string mExecFname;

        // <value>
        // 実行ファイルへのフルパス
        // </value>
        protected string mExecFullPath;

        // <value>
        // 実行結果のOUTPUT
        // </value>
        private string? mOutput;

        // <value>
        // 実行結果のOUTPUT
        // </value>
        public string? Output
        {
            get { return mOutput; }
        }

        // <summary>
        // コンストラクタ
        // <param>
        // mExecFname : 実行ファイル名
        // </param>
        // /summary>
        public EditorToolExec(string mExecFname)
        {
            mEditorPath = Path.GetDirectoryName(EditorApplication.applicationPath);
            mToolsPath = Path.Combine(mEditorPath, @"Data/Tools");
            this.mExecFname = mExecFname;
            //var files = Directory.GetFiles(mToolsPath, mExecFname, SearchOption.AllDirectories);
            var files = Directory.GetFiles(mEditorPath, mExecFname + "*", SearchOption.AllDirectories);

            if (files.Length == 0)
                Serilog.Log.Error("{App} app couldn't be found at {Path}", mExecFname, mEditorPath);

            mExecFullPath = files[0];
        }

        // <summary> 
        // コマンドラインツールを実行する
        // <param> 
        // arg : コマンドラインツールに渡す引数 
        // </param>
        // </summary>
        public int Exec(string arg)
        {
            int exitCode = -1;

            try
            {
                using var process = new Process();
                process.StartInfo.FileName = mExecFullPath;
                process.StartInfo.Arguments = arg;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                mOutput = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                exitCode = process.ExitCode;
                process.Close();
            }
            catch (Exception e)
            {
                //UnityEngine.Debug.Log(e);
                Serilog.Log.Error(e, "Exception while running process at {Scope}.{Location}", nameof(EditorToolExec), nameof(Exec));
            }

            return exitCode;
        }
    }
}
