using System;
using System.Collections.Generic;
using System.Linq;
using GSDev.AssetBundles.Editor;
using UnityEditor;
using UnityEditor.Build;

public class JenkinsAdapter {

    /// <summary>
    /// 构建版本号
    /// </summary>
    private static string BuildVersion = (int.Parse(DateTime.Now.ToString("yyMMddHH"))).ToString();

    ////原有的宏定义
    //private static string _oldDefineSymbols = "";
    /// <summary>
    /// 通用设置
    /// </summary>
    private static void CommonSetting(BuildTargetGroup target) {
        //去掉Unity的SplashScreen
        PlayerSettings.SplashScreen.show = false;

        //_oldDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup (target); //所有宏定义 ; 分割

        //删除定义的DEBUG宏
        //PlayerSettings.SetScriptingDefineSymbolsForGroup (target, ""); //写入全部宏,相当于改配置
        //PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", defineSymbols.ToArray()));
        if (target == BuildTargetGroup.Android)
            AssetBundleUpdateHelper.BuildAssetBundles(true, TargetPlatform.Android);
        else if (target == BuildTargetGroup.iOS)
            AssetBundleUpdateHelper.BuildAssetBundles(true, TargetPlatform.iOS);
        else
            AssetBundleUpdateHelper.BuildAssetBundles(true, TargetPlatform.Standalone);
        
        AssetDatabase.Refresh();

        AssetDatabase.Refresh();
        
        // 2021开始必须加这个，否则泛型接口调用报错
        if (target == BuildTargetGroup.Android)
        {
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.Android, Il2CppCodeGeneration.OptimizeSize);
        }
        else if (target == BuildTargetGroup.iOS)
        {
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.iOS, Il2CppCodeGeneration.OptimizeSize);
        }
        else
        {
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.Standalone, Il2CppCodeGeneration.OptimizeSize);
        }

        if (target == BuildTargetGroup.Android)
        {
            SetupAndroidKeystore();
            
            PlayerSettings.Android.bundleVersionCode = int.Parse(BuildVersion);
            //编译方式为Android Gradle
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        }
    }

    //设置还原
    private static void recover(BuildTargetGroup target) {
        // PlayerSettings.SetScriptingDefineSymbolsForGroup (target, _oldDefineSymbols);

        if (target == BuildTargetGroup.Android) {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
            EditorUserBuildSettings.buildAppBundle = false;
        }
    }

    private static void ReplaceScriptingDefineSymbols(BuildTargetGroup targetGroup, params string[] param) {
        var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        for (int i = 0, len = param.Length; i < len; i += 2) {
            symbols = symbols.Replace(param[i], param[i + 1]);
        }
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, symbols);

        // List<string> defineSymbols = new List<string>(ori.Split(';'));
        // PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", defineSymbols.ToArray()));
    }
    
    private static void AddScriptingDefineSymbol(BuildTargetGroup targetGroup, string symbol)
    {
        var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        var index = symbols.IndexOf(symbol, StringComparison.Ordinal);
        if (index >= 0)
            return;
        symbols += ";" + symbol;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, symbols);
        AssetDatabase.Refresh();
    }
    
    private static void RemoveScriptingDefineSymbol(BuildTargetGroup targetGroup, string symbol)
    {
        var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        var index = symbols.IndexOf(symbol, StringComparison.Ordinal);
        if (index < 0)
            return;
        symbols = symbols.Remove(index, symbol.Length);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, symbols);
        AssetDatabase.Refresh();
    }

    private static void RemoveScriptingDefineSymbolsContain(BuildTargetGroup targetGroup, string containPart)
    {
        var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        var split = symbols.Split(';').ToList();
        split.RemoveAll(symbol => symbol.Contains(containPart, StringComparison.InvariantCultureIgnoreCase));
        var replace = "";
        foreach (var symbol in split)
        {
            replace += $"{symbol};";
        }

        // remove last ;
        if (replace.Length > 1)
            replace = replace.Remove(replace.Length - 1, 1);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, replace);
        AssetDatabase.Refresh();
    }

    private static void SetDefineSymbolsForRelease(BuildTargetGroup targetGroup)
    {
        RemoveScriptingDefineSymbolsContain(targetGroup, "DISABLE");
        RemoveScriptingDefineSymbolsContain(targetGroup, "ENABLE");
        AddScriptingDefineSymbol(targetGroup,"ENABLE_PURCHASE_LOG");
        AddScriptingDefineSymbol(targetGroup,"HOTFIX_ENABLE");
        AssetDatabase.Refresh();
    }

    //打包 app bundle 
    [MenuItem("Jenkins/JenkinsBuildAndroid_aab")]
    public static void CommandLineBuildAndroid_aab()
    {
        SetDefineSymbolsForRelease(BuildTargetGroup.Android);
        CommonSetting(BuildTargetGroup.Android);

        EditorUserBuildSettings.development = false;
        EditorUserBuildSettings.buildAppBundle = false;
        CommandLineBuildAndroid();
        EditorUserBuildSettings.buildAppBundle = true;
        EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Debugging;
        var androidPath = GetAndroidPath();
        BuildPipeline.BuildPlayer(GetBuildScenes(), androidPath, BuildTarget.Android, BuildOptions.None);
    }

    [MenuItem("Jenkins/JenkinsBuildAndroid")]
    public static void CommandLineBuildAndroid()
    {
        SetDefineSymbolsForRelease(BuildTargetGroup.Android);
        CommonSetting(BuildTargetGroup.Android);
        EditorUserBuildSettings.development = false;

        //设置SetScriptingBackend为IL2CPP
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;

        var androidPath = GetAndroidPath();
        BuildPipeline.BuildPlayer(GetBuildScenes(), androidPath, BuildTarget.Android, BuildOptions.None);
        Console.WriteLine("Build Complete Path:" + androidPath);
    }

    [MenuItem("Jenkins/JenkinsBuildAndroid_Mono")]
    public static void CommandLineBuildAndroid_Mono() 
    {
        // RemoveScriptingDefineSymbol(BuildTargetGroup.Android, "ENABLE_PURCHASE_LOG");
        // AddScriptingDefineSymbol(BuildTargetGroup.Android, "ENABLE_GM");
        // AddScriptingDefineSymbol(BuildTargetGroup.Android, "HOTFIX_ENABLE");
        // AddScriptingDefineSymbol(BuildTargetGroup.Android, "LUNAR_CONSOLE_ENABLED");
        // LunarConsoleEditorInternal.Installer.EnablePlugin();
        CommonSetting(BuildTargetGroup.Android);

        //设置SetScriptingBackend为IL2CPP
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
        EditorUserBuildSettings.buildAppBundle = false;

        var androidPath = GetAndroidPath("_Test");
        BuildPipeline.BuildPlayer(GetBuildScenes(), androidPath, BuildTarget.Android, BuildOptions.None);
        Console.WriteLine("Build Complete Path:" + androidPath);
    }
    
    [MenuItem("Jenkins/AndroidStudioProject_TEST")]
    public static void CommandLineBuildAndroid_Studio_TEST()
    {
        CommonSetting(BuildTargetGroup.Android);
        RemoveScriptingDefineSymbol(BuildTargetGroup.Android, "ENABLE_PURCHASE_LOG");
        
        // 设置SetScriptingBackend为IL2CPP
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
        
        BuildPipeline.BuildPlayer(GetBuildScenes(), $"build/{PlayerSettings.productName}", BuildTarget.Android, BuildOptions.None);
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
    }

    #if UNITY_ANDROID
    [MenuItem("Jenkins/Setup Keystore")]
    #endif
    private static void SetupAndroidKeystore()
    {
        PlayerSettings.Android.keystoreName = System.IO.Path.Combine(System.Environment.CurrentDirectory, "keystore/OldSofa.keystore");

        PlayerSettings.Android.keystorePass = "OeSamlf$8dog^a{";
        PlayerSettings.Android.keyaliasName = "os";
        PlayerSettings.Android.keyaliasPass = "OeSamlf$8dog^a{";
    }

    [MenuItem("Jenkins/JenkinsBuildIos")]
    public static void CommandLineBuildIos() {
        //打包
        CommonSetting(BuildTargetGroup.iOS);
        
        //设置Build为日期格式
        PlayerSettings.iOS.buildNumber = BuildVersion;

        PlayerSettings.iOS.appleEnableAutomaticSigning = true;

        BuildPipeline.BuildPlayer(GetBuildScenes(), GetIosBuildPath(), BuildTarget.iOS, BuildOptions.None);
        Console.WriteLine("Build Complete Path:" + GetIosBuildPath());
    }
    
    [MenuItem("Jenkins/JenkinsBuildIosRelease")]
    public static void CommandLineBuildIosRelease() {
        //Release settings
        SetDefineSymbolsForRelease(BuildTargetGroup.iOS);
        EditorUserBuildSettings.development = false;
        
        CommandLineBuildIos();
    }

    [MenuItem("Jenkins/JenkinsBuildWindows")]
    public static void CommandLineBuildWin() {
        BuildPipeline.BuildPlayer(GetBuildScenes(), GetWindowsPath(), BuildTarget.StandaloneWindows, BuildOptions.None);
        Console.WriteLine("Build Complete Path:" + GetWindowsPath());
    }

    [MenuItem("Jenkins/Patch/Android")]
    public static void PatchAndroid()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        AssetBundleUpdateHelper.BuildAssetBundles(args.Contains("-inApp"), TargetPlatform.Android);
    }
    
    [MenuItem("Jenkins/Patch/iOS")]
    public static void PatchIos()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        AssetBundleUpdateHelper.BuildAssetBundles(args.Contains("-inApp"), TargetPlatform.iOS);
    }

    [AssetBundleVersionGetter]
    public static int GetAssetBundleVersion()
    {
        return (int)(DateTime.UtcNow - new DateTime(2023, 1, 1)).TotalHours;
    }

    /// <summary>
    /// 获取build Setting 列表里的打勾场景
    /// </summary>
    /// <returns></returns>
    private static string[] GetBuildScenes() {
        List<string> names = new List<string>();

        foreach (var x in EditorBuildSettings.scenes) {
            if (!x.enabled) continue;
            names.Add(x.path);
        }
        return names.ToArray();
    }

    #region Get Build Path 

    private static string GetIosBuildPath() {
        return "build/Ios";
    }

    private static string GetAndroidPath(string subjoin = "") {
        return string.Format("build/{2}_{0}_{1}{3}.apk", PlayerSettings.bundleVersion, BuildVersion, PlayerSettings.productName, subjoin);
    }

    private static string GetWindowsPath() {
        return "build/Win/Win.exe";
    }

    private static string GetMacPath() {
        return "build/mac";
    }

    #endregion

}