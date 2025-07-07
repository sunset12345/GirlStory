using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace JsonConfig.ConfigGenerateEditor
{
    public class ExecutePython : Editor
    {
        public static bool isRunPython = false;

        // [MenuItem("Tools/JsonConfig/Generate All")]
        // public static void ExportAllClass()
        // {
        //     ExportClass();
        // }
        
        public static void ExportClass(string args, string sheetFolder, string jsonFolder)
        {
            var settings = JsonConfigSettings.GetOrCreateSettings();
            var sheetPath = Environment.CurrentDirectory + sheetFolder;
            var outputJsonPath = Environment.CurrentDirectory + jsonFolder;
            var outputScriptPath = Environment.CurrentDirectory + settings.OutputScriptFolder;
            
            var assetPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(nameof(ConfigGenerateEditorWindow))[0]);
            var packagePath = assetPath.Split(new string[] {"/ConfigGenerateWindow"}, System.StringSplitOptions.RemoveEmptyEntries)[0];
            var tmpSplits = packagePath.Split('/');
            var packageName = tmpSplits[tmpSplits.Length - 1];
            var rootPath = string.Empty;

            var txt = System.IO.File.ReadAllText($"{Environment.CurrentDirectory}/Packages/packages-lock.json");
            var dicPackages1 =
                JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, object>>>>(txt)["dependencies"];

            if (dicPackages1.TryGetValue(packageName, out var dicPackages2))
            {
                if (dicPackages2.TryGetValue("hash", out var hash))
                {
                    rootPath = $"Library/PackageCache/{packageName}@{hash.ToString().Substring(0, 10)}";
                }
            }

            if (string.IsNullOrEmpty(rootPath)) rootPath = "Packages/JsonConfig";
            

            var pythonFile = $"{Environment.CurrentDirectory}/{rootPath}/Tools~/export.py";
#if UNITY_EDITOR_WIN
            var cmd = $"python {pythonFile}";
#else
            var runFile = $"{Environment.CurrentDirectory}/{rootPath}/Tools~/run.sh";
            var cmd = $"source {runFile} python {pythonFile}";
#endif

            sheetPath = sheetPath.Replace("\\", "/");
            outputJsonPath = outputJsonPath.Replace("\\", "/");
            outputScriptPath = outputScriptPath.Replace("\\", "/");
            
            
            var settingsPath = $"--path#{settings.HeaderStartRow}#{sheetPath}#{outputJsonPath}#{outputScriptPath}";
            
            if (args == null) RunScript(sheetPath, cmd, settingsPath);
            else RunScript(sheetPath, cmd, settingsPath, args);
        }

        public static void RunScript(string workDirectory, string cmd, params string[] args)
        {
            isRunPython = true;
#if UNITY_EDITOR_WIN
			const string shellApp = "cmd.exe";
#elif UNITY_EDITOR_OSX
            const string shellApp = "bash";
#endif
            var start = new ProcessStartInfo(shellApp);

#if UNITY_EDITOR_OSX
            start.Arguments = "-c";
#elif UNITY_EDITOR_WIN
		    start.Arguments = "/c";
#endif
            
            if (args != null)
            {
                foreach (var arg in args)
                {
                    cmd += " " + arg;
                }
            }
            start.Arguments += (" \"" + cmd + " \"");
            start.CreateNoWindow = false;
            start.ErrorDialog = true;
            start.UseShellExecute = false;
            start.WorkingDirectory = workDirectory;
            start.RedirectStandardOutput = false;
            start.RedirectStandardError = true;
            start.RedirectStandardInput = false;

            var p = new Process {StartInfo = start};
            // p.OutputDataReceived += OnDataReceivedEventHandler;
            p.ErrorDataReceived += OnErrorDataReceived;

            p.Start();
            // p.BeginOutputReadLine(); //开始读取输出数据
            p.BeginErrorReadLine(); //开始读取错误数据
            p.WaitForExit();
            p.CancelErrorRead();
            p.Close();
            UnityEngine.Debug.Log($"<color=cyan>process finish \n cmd:{cmd}</color>");
            isRunPython = false;
            
            AssetDatabase.Refresh();
        }

        public static void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data)) return;
            UnityEngine.Debug.LogError($"error: {e.Data}");
            // UnityEngine.Debug.Log($"<color=cyan>out: {e.Data}</color>");
        }
    }
}