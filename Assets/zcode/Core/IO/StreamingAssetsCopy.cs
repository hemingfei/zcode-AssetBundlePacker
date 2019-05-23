/***************************************************************
* Author: Zhang Minglin
* Note  : Application.streamingAssetsPath目录下拷贝
***************************************************************/
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace zcode
{
    /// <summary>
    /// Application.streamingAssetsPath目录下拷贝
    /// </summary>
    public class StreamingAssetsCopy
    {
        /// <summary>
        /// 是否结束
        /// </summary>
        public bool isDone { get; private set; }

        /// <summary>
        /// 拷贝结果
        /// </summary>
        public emIOOperateCode resultCode { get; private set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string error { get; private set; }

        /// <summary>
        ///   从Application.streamingAssetsPath目录下拷贝
        /// </summary>
        public IEnumerator Copy(string src, string dest)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_IPHONE
            src = "file:///" + src;
#endif
            SetResult(false, emIOOperateCode.Succeed, null);
            do
            {
                using (UnityWebRequest w = UnityWebRequest.Get(src))
                {
                    yield return w.SendWebRequest();

                    if (!string.IsNullOrEmpty(w.error))
                    {
                        SetResult(true, emIOOperateCode.Fail, w.error);
                    }
                    else
                    {
                        if (w.isDone && w.downloadHandler.data.Length > 0)
                        {
                            var ret = zcode.FileHelper.WriteBytesToFile(dest, w.downloadHandler.data, w.downloadHandler.data.Length);
                            SetResult(true, ret, null);
                        }
                    }
                }
            } while (!isDone);
        }

        /// <summary>
        /// 
        /// </summary>
        void SetResult(bool isDone, emIOOperateCode result, string error)
        {
            this.isDone = isDone;
            this.resultCode = result;
            this.error = error;
        }
    }
}
