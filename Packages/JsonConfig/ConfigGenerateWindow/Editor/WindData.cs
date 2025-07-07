using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace JsonConfig.ConfigGenerateEditor
{
    public enum PlatformType
    {
        Android,
        Ios
    }
    
    public class WindData
    {
        private readonly ConfigGenerateEditorWindow _win;

        public WindData(ConfigGenerateEditorWindow win)
        {
            _win = win;
        }

        #region Style

        private GUIStyle _styleDocument;
        public GUIStyle StyleDocument
        {
            get
            {
                if (_styleDocument != null) return _styleDocument;
                var style = GUI.skin.label;
                _styleDocument = new GUIStyle(GUI.skin.box)
                {
                    alignment = style.alignment,
                    normal = style.normal,
                    active = style.active,
                    focused = style.focused,
                    hover = style.hover
                };
                return _styleDocument;
            }
        }

        private GUIStyle _styleSeparator;

        public GUIStyle StyleSeparator
        {
            get
            {
                if (_styleSeparator != null) return _styleSeparator;
                _styleSeparator = new GUIStyle(GUI.skin.box)
                {
                    normal = {textColor = Color.cyan}, 
                    // alignment = TextAnchor.MiddleCenter
                };
                return _styleSeparator;
            }
        }

        
        #endregion
        
        #region Icon

        private Texture2D _exportIcon;
        public Texture2D ExportIcon
        {
            get
            {
                if ((UnityEngine.Object) _exportIcon == (UnityEngine.Object) null)
                    _exportIcon = AssetDatabase.LoadAssetAtPath(IconsPath + "undo3.png", typeof(Texture2D)) as Texture2D;
                return _exportIcon;
            }
        }
        private string _iconsPath;
        public string IconsPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_iconsPath)) return _iconsPath;
                //Assets/UltimatePlayerPrefsEditor/Editor/PlayerPrefsEditor.cs
                var path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(_win));

                //Strip PlayerPrefsEditor.cs
                path = path.Substring(0, path.LastIndexOf('/'));

                //Strip Editor/
                path = path.Substring(0, path.LastIndexOf('/') + 1);

                _iconsPath = path + "Editor/Icons/";

                return _iconsPath;
            }
        }

        #endregion
    }
}