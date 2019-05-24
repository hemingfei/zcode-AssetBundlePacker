/***************************************************************
 * Copyright 2016 By Zhang Minglin
 * Author: Zhang Minglin
 * Create: 2016/01/20
 * Note  : AssetBundle管理窗口
***************************************************************/
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace zcode.AssetBundlePacker
{
    public class AssetBundleBrowseWindow : EditorWindow
    {
        /// <summary>
        /// 
        /// </summary>
        class SelectResultStatus
        {
            /// <summary>
            /// 操作
            /// </summary>
            public enum Operate
            {
                None,
                Compress,
                Native,
                Permanent,
                StartupLoad,
            }

            public Operate Op;
            public bool IsCompress;
            public bool IsNative;
            public bool IsPermanent;
            public bool IsStartupLoad;
        }

        /// <summary>
        /// 
        /// </summary>
        class NodeGroup : GUILayoutMultiSelectGroup.NodeGroup
        {
            /// <summary>
            /// 
            /// </summary>
            public List<Node> Nodes = new List<Node>();

            /// <summary>
            /// 
            /// </summary>
            public override GUILayoutMultiSelectGroup.OperateResult Draw(float width)
            {
                GUILayoutMultiSelectGroup.OperateResult result = null;
                for (int i = 0; i < Nodes.Count; ++i)
                {
                    if (result == null)
                        result = Nodes[i].Draw(width);
                    else
                        Nodes[i].Draw(width);
                }

                return result;
            }

            /// <summary>
            /// 
            /// </summary>
            public override List<GUILayoutMultiSelectGroup.Node> GetRange(int begin, int end)
            {
                List<GUILayoutMultiSelectGroup.Node> temp = new List<GUILayoutMultiSelectGroup.Node>();

                if (begin < 0) begin = 0;
                if (begin >= Nodes.Count) begin = Nodes.Count - 1;
                if (end < 0) end = 0;
                if (end >= Nodes.Count) end = Nodes.Count - 1;

                for (int i = begin; i <= end; ++i)
                {
                    temp.Add(Nodes[i]);
                }

                return temp.Count > 0 ? temp : null;
            }
        }

        /// <summary>
        /// AssetBundle显示节点
        /// </summary>
        class Node : GUILayoutMultiSelectGroup.Node
        {
            /// <summary>
            /// 指向资源
            /// </summary>
            public ResourcesManifestData.AssetBundle AssetBundle;

            /// <summary>
            /// 渲染
            /// </summary>
            public override GUILayoutMultiSelectGroup.OperateResult Draw(float width)
            {
                if (AssetBundle == null)
                    return null;

                var config = AssetBundleBrowseWindow.Instance.Manifest.Data;

                GUI.backgroundColor = IsSelect ? Color.white : new Color(0.8f, 0.8f, 0.8f);
                GUILayout.BeginHorizontal("TextArea", GUILayout.MinHeight(20f));
                GUI.color = IsSelect ? Color.yellow : Color.white;
                GUILayout.Label(Index.ToString(), GUILayout.Width(24f));
                bool toggle = GUILayout.Button(AssetBundle.AssetBundleName, "TextField", GUILayout.Height(20f));
                float size = (float)AssetBundle.Size / 1024f;
                bool toggle_1 = GUILayout.Button(size.ToString("F2") + "KB", "TextField", GUILayout.Width(108f));
                GUILayout.Space(32f);
                bool is_compress = GUILayoutHelper.Toggle(config.IsAllCompress || AssetBundle.IsCompress, "", config.IsAllCompress, GUILayout.Width(24f));
                GUILayout.Space(40f);
                bool is_native = GUILayoutHelper.Toggle(config.IsAllNative || AssetBundle.IsNative, "", config.IsAllNative, GUILayout.Width(24f));
                GUILayout.Space(40f);
                bool is_permanent = GUILayoutHelper.Toggle(AssetBundle.IsPermanent, "", false, GUILayout.Width(24f));
                GUILayout.Space(40f);
                bool is_startup_load = GUILayoutHelper.Toggle(AssetBundle.IsStartupLoad, "", false, GUILayout.Width(24f));
                GUILayout.EndHorizontal();
                GUI.color = Color.white;
                GUI.backgroundColor = Color.white;

                SelectResultStatus.Operate op = SelectResultStatus.Operate.None;
                if(is_compress != AssetBundle.IsCompress)
                    op = SelectResultStatus.Operate.Compress;
                if (is_native != AssetBundle.IsNative)
                    op = SelectResultStatus.Operate.Native;
                if (is_permanent != AssetBundle.IsPermanent)
                    op = SelectResultStatus.Operate.Permanent;
                if (is_startup_load != AssetBundle.IsStartupLoad)
                    op = SelectResultStatus.Operate.StartupLoad;

                if (toggle || toggle_1 || op != SelectResultStatus.Operate.None)
                {

                    return new GUILayoutMultiSelectGroup.OperateResult()
                    {
                        SelectNode = this,
                        Status = new SelectResultStatus()
                        {
                            Op = op,
                            IsCompress = is_compress,
                            IsNative = is_native,
                            IsPermanent = is_permanent,
                            IsStartupLoad = is_startup_load,
                        },
                    };
                }

                return null;
            }
        }

        /// <summary>
        ///   AssetBundle信息描述数据
        /// </summary>
        public ResourcesManifest Manifest;

        /// <summary>
        /// 
        /// </summary>
        private GUILayoutMultiSelectGroup gui_multi_select_;

        /// <summary>
        ///   载入数据
        /// </summary>
        public bool LoadData(ResourcesManifest manifest = null)
        {
            bool result = false;
            if(manifest == null)
            {
                Manifest = new ResourcesManifest();
                result = Manifest.Load(EditorCommon.RESOURCES_MANIFEST_FILE_PATH);
            }
            else
            {
                Manifest = manifest;
                result = true;
            }

            Build();

            return result;
        }

        /// <summary>
        ///   保存数据
        /// </summary>
        private bool SaveData()
        {
            if (Manifest != null)
                return Manifest.Save(EditorCommon.RESOURCES_MANIFEST_FILE_PATH);

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Build()
        {
            NodeGroup group = new NodeGroup();
            int index = 0;
            foreach (var ab in Manifest.Data.AssetBundles.Values)
            {
                //过滤主AssetBundle文件
                if (ab.AssetBundleName == Common.MAIN_MANIFEST_FILE_NAME)
                    continue;

                Node node = new Node()
                {
                    Index = index++,
                    IsSelect = false,
                    AssetBundle = ab,
                };
                group.Nodes.Add(node);
            }

            gui_multi_select_ = new GUILayoutMultiSelectGroup(group);
        }

        /// <summary>
        /// 执行已做的修改
        /// </summary>
        void ExecuteModified()
        {
            ResourcesManifest old_resources_manifest = new ResourcesManifest();
            old_resources_manifest.Load(EditorCommon.RESOURCES_MANIFEST_FILE_PATH);
            
            //压缩AB包
            bool compress = BuildAssetBundle.CompressAssetBundles(old_resources_manifest
                                                                    , Manifest);
            //保存数据
            bool save = compress ? SaveData() : false;
            //拷贝资源
            bool copy = save ? BuildAssetBundle.CopyNativeAssetBundleToStreamingAssets(Manifest) : false;
            bool succeed = compress && copy && save;

            if(succeed)
            {
                //同步数据
                if (AssetBundleBuildWindow.Instance != null)
                {
                    AssetBundleBuildWindow.Instance.SyncConfigForm(Manifest.Data);
                }
                else
                {
                    AssetBundleBuild buildData = new AssetBundleBuild();
                    buildData.Load(EditorCommon.ASSETBUNDLE_BUILD_RULE_FILE_PATH);
                    buildData.SyncConfigFrom(Manifest.Data);
                    buildData.Save(EditorCommon.ASSETBUNDLE_BUILD_RULE_FILE_PATH);
                }
            }

            string title = "执行配置AssetBundle" + (succeed ? "成功" : "失败");
            string compress_desc = "压缩资源 - " + (compress ? "成功" : "失败");
            string save_desc = "保存配置文件 - " + (save ? "成功" : "失败");
            string copy_desc = "拷贝初始资源至安装包目录 - " + (copy ? "成功" : "失败");
            string desc = compress_desc + "\n"
                        + save_desc + "\n"
                        + copy_desc + "\n\n";

            EditorUtility.DisplayDialog(title, desc, "确认");
        }

        #region Select Operate
        /// <summary>
        /// 更新选中操作
        /// </summary>
        void UpdateSelectOperate(GUILayoutMultiSelectGroup.OperateResult result)
        {
            if (result != null)
            {
                SelectResultStatus status = result.Status as SelectResultStatus;
                if (status.Op != SelectResultStatus.Operate.None 
                    && gui_multi_select_.SelectNodes != null)
                {
                    for(int i = 0 ; i < gui_multi_select_.SelectNodes.Count ; ++i)
                    {
                        Node node = gui_multi_select_.SelectNodes[i] as Node;
                        if (status.Op == SelectResultStatus.Operate.Compress)
                            node.AssetBundle.IsCompress = status.IsCompress;
                        else if (status.Op == SelectResultStatus.Operate.Native)
                            node.AssetBundle.IsNative = status.IsNative;
                        else if (status.Op == SelectResultStatus.Operate.Permanent)
                            node.AssetBundle.IsPermanent = status.IsPermanent;
                        else if (status.Op == SelectResultStatus.Operate.Permanent)
                            node.AssetBundle.IsStartupLoad = status.IsStartupLoad;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        ///   
        /// </summary>
        void OnGUI()
        {
            GUILayout.Space(3f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("资源版本号", GUILayout.MaxWidth(200f));
            Manifest.Data.strVersion = EditorGUILayout.TextField(Manifest.Data.strVersion);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("打包全部AssetBundle至安装包", GUILayout.MaxWidth(200f));
            Manifest.Data.IsAllNative = EditorGUILayout.Toggle(Manifest.Data.IsAllNative);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("压缩全部AssetBundle", GUILayout.MaxWidth(200f));
            Manifest.Data.IsAllCompress = EditorGUILayout.Toggle(Manifest.Data.IsAllCompress);
            GUILayout.EndHorizontal();

            GUILayout.Space(3f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("编号", "OL Title", GUILayout.Width(32f));
            GUILayout.Label("资源", "OL Title");
            GUILayout.Label("大小", "OL Title", GUILayout.Width(124f));
            GUILayout.Label("压缩", "OL Title", GUILayout.Width(72f));
            GUILayout.Label("打包到安装包", "OL Title", GUILayout.Width(84f));
            GUILayout.Label("常驻内存", "OL Title", GUILayout.Width(60f));
            GUILayout.Label("启动时加载", "OL Title", GUILayout.Width(60f));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayoutMultiSelectGroup.OperateResult result = gui_multi_select_.Draw(position.size.x);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (result != null)
                UpdateSelectOperate(result);

            bool restore = GUILayout.Button("还原");
            bool execute = GUILayout.Button("执行修改（压缩AssetBundle、保存配置文件、拷贝初始文件至安装包目录）");
            if (restore)
                LoadData();
            if (execute)
                ExecuteModified();
        }

        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            LoadData();
        }

        /// <summary>
        ///   
        /// </summary>
        void OnEnable()
        {
            LoadData();
            Instance = this;
        }

        /// <summary>
        ///   
        /// </summary>
        void Update()
        {
        } 

        /// <summary>
        ///   
        /// </summary>
        void OnInspectorUpdate()
        {
            //Debug.Log("窗口面板的更新");
            //这里开启窗口的重绘，不然窗口信息不会刷新
            this.Repaint();
        }

        [MenuItem("AssetBundle/Windows/AssetBundle Browse Window")]
        public static void Open()
        {
            var win = EditorWindow.GetWindow<AssetBundleBrowseWindow>(false, "AssetBundle Browse", true);
            if(win != null)
            {
                win.Show();
            }
        }

        /// <summary>
        /// 界面单例
        /// </summary>
        public static AssetBundleBrowseWindow Instance = null;
    }
}