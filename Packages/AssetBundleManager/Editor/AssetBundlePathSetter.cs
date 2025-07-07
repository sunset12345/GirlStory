using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class AssetBundlePathSetter
{
    public static void UpdatePathByFolder(
        string path,
        string prefix,
        int depth = 0,
        string exceptFolderName = "")
    {
        var fullPath = Path.Combine(Application.dataPath, path);
        var directories = Directory.GetDirectories(fullPath);
        foreach (var dir in directories)
        {
            var folderName = Path.GetFileNameWithoutExtension(dir);
            if (string.IsNullOrEmpty(exceptFolderName) || folderName != exceptFolderName)
            {
                UpdateFolderAssetBundleName(
                    depth, 
                    path, 
                    folderName, 
                    prefix);
            }
        }
    }

    public static void UpdatePathByAsset(
        string path,
        string prefix,
        string searchPattern = "*",
        bool isRecursionFolder=true)
    {
        var fullPath = Path.Combine(Application.dataPath, path);
        var directories = Directory.GetFiles(
            fullPath, 
            searchPattern,
            isRecursionFolder ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        foreach (var dir in directories)
        {
            if (dir.Contains(".meta"))
                continue;
            try
            {
                var fileName = Path.GetFileName(dir);
                var fileDir = Path.GetDirectoryName(dir);
                fileDir = fileDir.Remove(0, Application.dataPath.Length + 1);
                var finalPrefix = $"{prefix}";
                if (isRecursionFolder)
                    finalPrefix += Path.GetDirectoryName(dir).Remove(0, fullPath.Length);
                
                UpdateFileAssetBundleName(
                    fileDir, 
                    fileName, 
                    finalPrefix);
            }
            catch (Exception e)
            {
                Debug.LogError($"Update asset bundle path failed for asset at {dir} with an exception:{e}");
            }
        }
    }

    private static void UpdateFolderAssetBundleName(int depth,string path,string folderName,string prefix)
    {
        if (depth > 0)
        {
            UpdatePathByFolder(
                Path.Combine(path, folderName),
                $"{prefix}/{folderName.ToLowerInvariant()}",
                depth - 1);
        }
        else
        {
            var assetBundleName = $"{prefix}/{folderName.ToLower()}";
            var newDir = $"Assets/{path}/{folderName}";
            var folderImporter = AssetImporter.GetAtPath(newDir);
            folderImporter.assetBundleName = assetBundleName;
        }
    }

    private static void UpdateFileAssetBundleName(string path, string fileName, string prefix)
    {
        var assetName = Path.GetFileNameWithoutExtension(fileName);
        var assetBundleName = $"{prefix}/{assetName.ToLower()}";
        var newDir = $"Assets/{path}/{fileName}";
        var folderImporter = AssetImporter.GetAtPath(newDir);
        folderImporter.SetAssetBundleNameAndVariant(assetBundleName, "");
    }
}