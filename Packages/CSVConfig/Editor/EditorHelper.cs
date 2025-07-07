using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GSDev.CSVConfig.Editor
{
    public static class EditorHelper
    {
        private const string CSVFileExt = ".csv";
        private const string ExcelFileExt = ".xlsx";

        private class Logger : ILogger
        {
            public void LogInfo(string info)
            {
                Debug.Log(info);
            }

            public void LogWarning(string warning)
            {
                Debug.LogWarning(warning);
            }

            public void LogError(string error)
            {
                Debug.LogError(error);
            }
        }

        private static IEnumerable<string> GetSelectedCSVFilePaths()
        {
            var csvPaths = Selection.objects
                .Select(AssetDatabase.GetAssetPath)
                .Where((path) => Path.GetExtension(path).Equals(CSVFileExt, StringComparison.OrdinalIgnoreCase));
            return csvPaths;
        }

        private static void GenerateCode(IEnumerable<string> paths)
        {
            var settings = CSVSettings.GetOrCreateSettings();
            var outputFolderPath = settings.ScriptOutputPath;
            if (string.IsNullOrEmpty(outputFolderPath))
            {
                Debug.LogError("Script output folder is not been set in CSV settings while trying to generate CSV config script.");
                return;
            }

            ScriptGenerator.Logger = new Logger();
            foreach (var path in paths)
            {
                if (path.IndexOf("const", StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    ScriptGenerator.GenerateConst(
                        path, 
                        outputFolderPath);
                    
                }
                else
                {
                    ScriptGenerator.GenerateTable(
                        path,
                        outputFolderPath);
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Create Config Reader Code", false)]
        public static void CreateConfigReaderCode()
        {
            var paths = GetSelectedCSVFilePaths();
            GenerateCode(paths);
        }
        
        [MenuItem("Assets/Create Config Reader Code", true)]
        public static bool CreateConfigReaderCodeValidator()
        {
            var csvPaths = GetSelectedCSVFilePaths();
            return csvPaths.Any();
        }
        
        private static IEnumerable<string> GetSelectedExcelFilePaths()
        {
            var excelPaths = Selection.objects
                .Select(AssetDatabase.GetAssetPath)
                .Where((path) => Path.GetExtension(path).Equals(ExcelFileExt, StringComparison.OrdinalIgnoreCase));
            return excelPaths;
        }
        
        [MenuItem("Assets/Convert Excel to CSV", false)]
        public static void ConvertExcelToCSV()
        {
            var paths = GetSelectedExcelFilePaths();
            var settings = CSVSettings.GetOrCreateSettings();
            
            foreach (var path in paths)
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                var outputPath = Path.Combine(settings.DefaultConfigPath, $"{fileName}{CSVFileExt}");
                var excel = new ExcelUtility(path);
                excel.ConvertToCSV(outputPath, Encoding.UTF8);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        [MenuItem("Assets/Convert Excel to CSV", true)]
        public static bool ConvertExcelToCSVValidator()
        {
            var excelPaths = GetSelectedExcelFilePaths();
            return excelPaths.Any();
        }
    }
}