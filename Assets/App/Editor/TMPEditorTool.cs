using UnityEditor;

namespace App.Editor
{
    public static class TMPEditorTool
    {
        [MenuItem("Assets/TMP/Clear Data", false, 0)]
        public static void ClearData()
        {
            var fontAsset = Selection.activeObject as TMPro.TMP_FontAsset;
            if (fontAsset == null)
                return;
            
            fontAsset.ClearFontAssetData(true);
            AssetDatabase.SaveAssets();
        }
        
        [MenuItem("Assets/TMP/Clear Data", true, 0)]
        public static bool ClearDataValidate()
        {
            // if selection is not TMP font asset, return false
            return Selection.activeObject is TMPro.TMP_FontAsset;
        }
    }
}