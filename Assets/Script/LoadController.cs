using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace XuanZhiShiLian
{
    /// <summary>
    /// 游戏加载控制器
    /// 作为游戏的入口场景，负责初始化游戏系统并跳转到主菜单
    /// </summary>
    public class LoadController : MonoBehaviour
    {
        [Header("加载设置")]
        [Tooltip("初始化等待时间")]
        public float initWaitTime = 1f;
        [Tooltip("目标场景名称")]
        public string targetSceneName = "Stage0";
        
        private bool isLoading = false;
        
        private void Start()
        {
            // 在首个加载场景，确保背包UI不显示
            HideInventoryPanelIfExists();
            // 若有背包系统实例，显式隐藏并禁止其在本场景初始化UI
            if (InventorySystem.Instance != null)
            {
                // Load场景不显示
                var invGo = InventorySystem.Instance.gameObject;
                if (invGo != null)
                {
                    // 确保其不会在此场景创建UI
                    // 调用隐藏方法
                    var mi = typeof(InventorySystem).GetMethod("InitializeInventoryUI", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    // 不调用InitializeInventoryUI即可，若已存在面板，隐藏
                }
            }
            StartCoroutine(LoadGameSequence());
        }
        
        /// <summary>
        /// 游戏加载序列
        /// </summary>
        private IEnumerator LoadGameSequence()
        {
            if (isLoading) yield break;
            isLoading = true;
            
            // 再次确保首场景不显示背包UI（防止执行顺序导致的闪现）
            HideInventoryPanelIfExists();

            // 等待初始化时间
            yield return new WaitForSeconds(initWaitTime);
            
            // 初始化游戏管理器
            yield return StartCoroutine(InitializeGameManager());
            
            // 在切场景前再次兜底隐藏（确保整个加载阶段都不显示）
            HideInventoryPanelIfExists();

            // 加载目标场景
            yield return StartCoroutine(LoadTargetScene());
            
            isLoading = false;
        }
        
        /// <summary>
        /// 初始化游戏管理器
        /// </summary>
        private IEnumerator InitializeGameManager()
        {
            // 检查GameManager是否已存在
            if (GameManager.Instance == null)
            {
                // 等待GameManager初始化
                float timeout = 5f;
                float elapsed = 0f;
                while (GameManager.Instance == null && elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                
                if (GameManager.Instance == null)
                {
                    yield break;
                }
            }
            
            // 等待GameManager完全初始化
            yield return new WaitForSeconds(0.5f);
        }
        
        /// <summary>
        /// 加载目标场景
        /// </summary>
        private IEnumerator LoadTargetScene()
        {
            
            // 异步加载场景
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
            
            // 等待场景完全加载
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        /// <summary>
        /// 如果场景中存在背包面板，则将其隐藏（用于首个加载场景）
        /// </summary>
        private void HideInventoryPanelIfExists()
        {
            // 通过组件查找
            var controllers = FindObjectsOfType<InventoryPanelController>();
            if (controllers != null)
            {
                foreach (var c in controllers)
                {
                    if (c != null && c.gameObject != null && c.gameObject.activeSelf)
                    {
                        c.gameObject.SetActive(false);
                    }
                }
            }

            // 通过名称兜底
            GameObject go = GameObject.Find("InventoryPanel");
            if (go != null && go.activeSelf)
            {
                go.SetActive(false);
            }
        }
    }
}