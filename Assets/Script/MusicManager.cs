using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace XuanZhiShiLian
{
    /// <summary>
    /// 全局音乐管理器：
    /// - 跨场景不销毁
    /// - 按场景切换曲目：Stage5 播放 5Secrets，其余场景播放 sostenuto
    /// - 曲目不变时，过场景不会重头播放
    /// 使用方式：
    /// 1) 将音频文件放入 Assets/Resources/Audio/ 目录，文件名分别为 5Secrets.mp3 和 sostenuto.mp3
    /// 2) 运行游戏时会自动创建并生效，无需手动挂载到场景
    /// </summary>
    public sealed class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance { get; private set; }

        private AudioSource musicSource;
        private string currentClipName = string.Empty;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureExists()
        {
            if (Instance != null) return;

            var go = new GameObject("MusicManager");
            go.AddComponent<MusicManager>();
            DontDestroyOnLoad(go);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            musicSource = gameObject.GetComponent<AudioSource>();
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
            }

            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f; // 2D 音频
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            // 首次场景进来时，确保根据当前场景设置音乐
            UpdateMusicForScene(SceneManager.GetActiveScene().name);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode _)
        {
            UpdateMusicForScene(scene.name);
        }

        private void UpdateMusicForScene(string sceneName)
        {
            string desiredClipName = GetDesiredClipName(sceneName);

            // 若目标曲目与当前一致，且正在播放，则不做任何处理，保持不断播
            if (string.Equals(currentClipName, desiredClipName, StringComparison.OrdinalIgnoreCase))
            {
                if (musicSource.clip != null && musicSource.isPlaying)
                {
                    return;
                }
                // 同一首曲子但未在播放（例如被外部暂停），则继续播放
                if (musicSource.clip != null)
                {
                    musicSource.Play();
                    return;
                }
            }

            // 切换为新曲目
            var nextClip = Resources.Load<AudioClip>("Audio/" + desiredClipName);
            if (nextClip == null)
            {
                return;
            }

            musicSource.clip = nextClip;
            currentClipName = desiredClipName;
            musicSource.Play();
        }

        private static string GetDesiredClipName(string sceneName)
        {
            // 精确匹配 Stage5（忽略大小写）
            if (string.Equals(sceneName, "Stage5", StringComparison.OrdinalIgnoreCase))
            {
                return "5Secrets";
            }

            return "sostenuto";
        }
    }
}

