#if ENABLE_HYBRIDCLR
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
#endif
using TEngine.Editor;
using UnityEditor;
using UnityEngine;

public static class BuildDLLCommand
{
    private const string EnableHybridClrScriptingDefineSymbol = "ENABLE_HYBRIDCLR";

    /// <summary>
    /// 禁用HybridCLR宏定义。
    /// </summary>
    [MenuItem("HybridCLR/Define Symbols/Disable HybridCLR", false, 30)]
    public static void Disable()
    {
        ScriptingDefineSymbols.RemoveScriptingDefineSymbol(EnableHybridClrScriptingDefineSymbol);
#if ENABLE_HYBRIDCLR
        HybridCLR.Editor.SettingsUtil.Enable = false;
        UpdateSettingEditor.ForceUpdateAssemblies();
#endif
    }

    /// <summary>
    /// 开启HybridCLR宏定义。
    /// </summary>
    [MenuItem("HybridCLR/Define Symbols/Enable HybridCLR", false, 31)]
    public static void Enable()
    {
        ScriptingDefineSymbols.RemoveScriptingDefineSymbol(EnableHybridClrScriptingDefineSymbol);
        ScriptingDefineSymbols.AddScriptingDefineSymbol(EnableHybridClrScriptingDefineSymbol);
#if ENABLE_HYBRIDCLR
        HybridCLR.Editor.SettingsUtil.Enable = true;
        UpdateSettingEditor.ForceUpdateAssemblies();
#endif
    }
    
    /// <summary>
    /// 强制同步Assemblies。
    /// </summary>
    [MenuItem("HybridCLR/Build/Force UpdateAssemblies", false, 30)]
    public static void ForceUpdateAssemblies()
    {
#if ENABLE_HYBRIDCLR
        UpdateSettingEditor.ForceUpdateAssemblies();
#endif
    }

    [MenuItem("HybridCLR/Build/BuildAssets And CopyTo AssemblyTextAssetPath", false, 31)]
    public static void BuildAndCopyDlls()
    {
#if ENABLE_HYBRIDCLR
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        StripAOTDllCommand.GenerateStripedAOTDlls(target);
        CompileDllCommand.CompileDll(target);
        CopyAOTHotUpdateDlls(target);
#endif
    }

    public static void BuildAndCopyDlls(BuildTarget target)
    {
#if ENABLE_HYBRIDCLR
        StripAOTDllCommand.GenerateStripedAOTDlls(target);
        CompileDllCommand.CompileDll(target);
        CopyAOTHotUpdateDlls(target);
#endif
    }

    public static void CopyAOTHotUpdateDlls(BuildTarget target)
    {
        CopyAOTAssembliesToAssetPath();
        CopyHotUpdateAssembliesToAssetPath();
        AssetDatabase.Refresh();
    }

    public static void CopyAOTAssembliesToAssetPath()
    {
#if ENABLE_HYBRIDCLR
        var target = EditorUserBuildSettings.activeBuildTarget;
        string aotAssembliesSrcDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
        string aotAssembliesDstDir = Application.dataPath +"/"+ TEngine.Settings.UpdateSetting.AssemblyTextAsset2AOTPath;

        foreach (var dll in TEngine.Settings.UpdateSetting.AOTMetaAssemblies)
        {
            string srcDllPath = $"{aotAssembliesSrcDir}/{dll}";
            if (!System.IO.File.Exists(srcDllPath))
            {
                Debug.LogError($"ab中添加AOT补充元数据dll:{srcDllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先构建一次游戏App后再打包。");
                continue;
            }
            string dllBytesPath = $"{aotAssembliesDstDir}/{dll}.bytes";
            System.IO.File.Copy(srcDllPath, dllBytesPath, true);
            Debug.Log($"[CopyAOTAssembliesToStreamingAssets] copy AOT dll {srcDllPath} -> {dllBytesPath}");
        }
#endif
    }

    public static void CopyHotUpdateAssembliesToAssetPath()
    {
#if ENABLE_HYBRIDCLR
        var target = EditorUserBuildSettings.activeBuildTarget;

        string hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
        string hotfixAssembliesDstDir = Application.dataPath +"/"+ TEngine.Settings.UpdateSetting.AssemblyTextAsset2HotFixPath;
        foreach (var dll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
        {
            string dllPath = $"{hotfixDllSrcDir}/{dll}";
            string dllBytesPath = $"{hotfixAssembliesDstDir}/{dll}.bytes";
            System.IO.File.Copy(dllPath, dllBytesPath, true);
            Debug.Log($"[CopyHotUpdateAssembliesToStreamingAssets] copy hotfix dll {dllPath} -> {dllBytesPath}");
        }
#endif
    }
}