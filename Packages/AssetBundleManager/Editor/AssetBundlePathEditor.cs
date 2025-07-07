using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GSDev.AssetBundles.Editor
{
    public class AssetBundlePathEditor : EditorWindow
    {
        private const string ConfigPath = "Assets/Editor/AssetBundle/AssetBundlePathConfig.asset";

        private static AssetBundlePathEditor _window;
        private static AssetBundlePathConfig _config;


        private ReorderableList _ruleList;
        private Vector2 _scrollPosition;

        private SerializedObject _serializedObject;
        private SerializedProperty _serializedProperty;

        private static AssetBundlePathConfig Config
        {
            get
            {
                if (_config == null)
                {
                    _config = EditorHelper.LoadOrCreateScriptableObject<AssetBundlePathConfig>(ConfigPath);
                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = _config;
                }

                return _config;
            }
        }

        private void OnEnable()
        {
            _serializedObject = new SerializedObject(Config);
            _serializedProperty = _serializedObject.FindProperty("RuleList");

            _ruleList = new ReorderableList(_serializedObject, _serializedProperty, true, true, true, true);
            _ruleList.elementHeight = EditorGUIUtility.singleLineHeight + 6f;
            _ruleList.drawElementCallback = OnElementDraw;
            _ruleList.drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "规则列表"); };
            _ruleList.onAddCallback += OnElementAdd;
            _ruleList.onRemoveCallback += OnElementRemove;
        }

        private void OnDisable()
        {
            _ruleList.onAddCallback -= OnElementAdd;
            _ruleList.onRemoveCallback -= OnElementRemove;
            SaveChanges();
        }

        private void OnGUI()
        {
            if(_serializedObject == null)
                OnEnable();
            _serializedObject.Update();
            // EditorGUILayout.PropertyField(_serializedProperty, true);
            EditorGUILayout.BeginVertical();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            _ruleList.DoLayoutList();
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("保存配置"))
                SaveChanges();
            EditorGUILayout.EndVertical();
            _serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("Asset Bundle/路径编辑器", false, 2)]
        private static void WindowShow()
        {
            _window = GetWindow<AssetBundlePathEditor>(false, "AssetBundle路径编辑器");
            _window.CalculateMinSize();
            _window.Show();
        }

        [MenuItem("Asset Bundle/刷新全部路径", false, 2)]
        private static void UpdateAllPaths()
        {
            Config.ApplyAllRules();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void CalculateMinSize()
        {
            minSize = new Vector2(1400, Config.RuleList.Count * (EditorGUIUtility.singleLineHeight + 8) + 80);
        }

        public override void SaveChanges()
        {
            base.SaveChanges();
            EditorUtility.SetDirty(Config);
            AssetDatabase.SaveAssets();
        }

        private void OnElementDraw(
            Rect rect,
            int index,
            bool isActive,
            bool isFocused)
        {
            var element = _serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            var titleStyle = new GUIStyle();
            titleStyle.normal.background = null;
            titleStyle.normal.textColor = new Color(1, 1, 1);
            titleStyle.fontSize = 12;
            ShowSingleRule(element, titleStyle, index, rect);
        }

        private void OnElementAdd(ReorderableList list)
        {
            Config.RuleList.Add(default);
            CalculateMinSize();
        }

        private void OnElementRemove(ReorderableList list)
        {
            Config.RuleList.RemoveAt(list.index);
            CalculateMinSize();
        }

        private void ShowSingleRule(
            SerializedProperty property, 
            GUIStyle titleStyle, 
            int index, 
            Rect defaultRect)
        {
            var param = Config.RuleList[index];
            EditorGUIUtility.labelWidth = 58;
            var rect = new Rect(
                defaultRect.x, 
                defaultRect.y, 
                170, 
                EditorGUIUtility.singleLineHeight);
            
            param.RuleName = EditorGUI.TextField(
                rect,
                new GUIContent("规则名称:"),
                param.RuleName);
            
            rect.x = rect.xMax + 10;
            rect.width = 126;
            param.RuleType = (AssetBundleRuleType)EditorGUI.EnumPopup(
                rect, 
                "资源类型:",
                param.RuleType);
            
            EditorGUIUtility.labelWidth = 58;
            rect.x = rect.xMax + 10;
            rect.width = 200;
            param.Prefix = EditorGUI.TextField(
                rect, 
                "AB前缀:",
                string.IsNullOrEmpty(param.Prefix) ? param.Path : param.Prefix);

            rect.x = rect.xMax + 10;
            rect.width = 200;
            EditorGUIUtility.labelWidth = 38;
            param.Root = EditorGUI.ObjectField(
                rect, 
                "路径:", 
                param.Root, 
                typeof(DefaultAsset)) as DefaultAsset;
            param.Path = param.Root != null 
                ? AssetDatabase.GetAssetPath(param.Root).Substring("Assets/".Length) 
                : "";
            
            rect.x = rect.xMax + 10;
            rect.width = 220;
            GUI.color = Color.green;
            EditorGUI.LabelField(rect, param.Path);
            GUI.color = Color.white;

            if (param.RuleType == AssetBundleRuleType.Folder)
            {
                rect.x = rect.xMax + 10;
                rect.width = 80;
                param.Depth = EditorGUI.IntField(
                    rect,
                    "路径深度:",
                    param.Depth);

                rect.x = rect.xMax + 10;
                rect.width = 200;
                param.ExcludeFolderName = EditorGUI.TextField(
                    rect,
                    "忽略目录:",
                    param.ExcludeFolderName);
            }
            else
            {
                rect.x = rect.xMax + 10;
                rect.width = 200;
                param.SearchPattern = EditorGUI.TextField(
                    rect,
                    "搜索模式:",
                    param.SearchPattern);
                rect.x = rect.xMax + 10;
                rect.width = 80;
                param.IsRecursionFolder = EditorGUI.Toggle(rect, "递归", param.IsRecursionFolder);
            }

            rect.x = rect.xMax + 10;
            rect.width = 100;
            if (GUI.Button(rect, "执行"))
            {
                AssetBundlePathConfig.ApplyRule(param);
                AssetDatabase.SaveAssets();
            }
        }


        //获取选中文件夹的folderImporter
        private AssetImporter GetSelectFolderImporter()
        {
            var obj = Selection.activeObject;
            var path = AssetDatabase.GetAssetPath(obj);
            return AssetImporter.GetAtPath(path);
        }

        //获取选中文件夹的AB路径
        private string GetSelectPath()
        {
            var folderImporter = GetSelectFolderImporter();
            return folderImporter.assetBundleName;
        }
    }
}