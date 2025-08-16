using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace XuanZhiShiLian
{
    public class AnimationInterface : MonoBehaviour
    {
        [Header("相机控制")]
        public Camera sceneCamera; // 场景相机引用（如果为空则使用Camera.main）
        public GameObject player;
        
        [Header("Stage5设置")]
        public TextMeshProUGUI storyTitleText; // 故事标题显示文本
        
        // 莉姆莉卡问答对话状态跟踪
        private bool hasSelectedQAOption = false; // 是否已经选择过问答选项（用于控制属性值变化）
        
        /// <summary>
        /// 动画接口：设置相机位置到Stage5-2的固定位置（瞬移）
        /// </summary>
        public void SetCamPos5_2()
        {
            Camera targetCamera = sceneCamera != null ? sceneCamera : Camera.main;
            if (targetCamera != null)
            {
                Vector3 targetPosition = new Vector3(-6.103516e-05f, -16.27f, -10f);
                targetCamera.transform.position = targetPosition;
            }
            player.transform.position = new Vector3(-6.59f, -18.24999f, 0f);
        }

        public void SetCamPos5_3_1()
        {
            Camera targetCamera = sceneCamera != null ? sceneCamera : Camera.main;
            if (targetCamera != null)
            {
                Vector3 targetPosition = new Vector3(21.55f, -16.61999f, -10f);
                targetCamera.transform.position = targetPosition;
            }
            player.transform.position = new Vector3(13.03f, -15.66999f, 0f);
        }

        public void SetCamPos5_3_3()
        {
            Camera targetCamera = sceneCamera != null ? sceneCamera : Camera.main;
            if (targetCamera != null)
            {
                Vector3 targetPosition = new Vector3(47.71f, -16.61999f, -10f);
                targetCamera.transform.position = targetPosition;
            }
            player.transform.position = new Vector3(42.43f, -16.39999f, 0f);
        }

        public void SetCamPos5_3_4()
        {
            Camera targetCamera = sceneCamera != null ? sceneCamera : Camera.main;
            if (targetCamera != null)
            {
                Vector3 targetPosition = new Vector3(72.16f, -16.61999f, -10f);
                targetCamera.transform.position = targetPosition;
            }
            player.transform.position = new Vector3(64.93f, -15f, 0f);
        }
        public void SetCamPos5_4_1()
        {
            Camera targetCamera = sceneCamera != null ? sceneCamera : Camera.main;
            if (targetCamera != null)
            {
                Vector3 targetPosition = new Vector3(0f, -34.02f, -10f);
                targetCamera.transform.position = targetPosition;
            }
            player.transform.position = new Vector3(3.78f, -34.04f, 0f);
        }

        public void SetCamPos5_4_11()
        {
            Camera targetCamera = sceneCamera != null ? sceneCamera : Camera.main;
            if (targetCamera != null)
            {
                Vector3 targetPosition = new Vector3(21.55f, -34.02f, -10f);
                targetCamera.transform.position = targetPosition;
            }
            player.transform.position = new Vector3(13.03f, -33.07f, 0f);
        }
        public void SetCamPos5_4_2()
        {
            Camera targetCamera = sceneCamera != null ? sceneCamera : Camera.main;
            if (targetCamera != null)
            {
                Vector3 targetPosition = new Vector3(46.7f, -34.02f, -10f);
                targetCamera.transform.position = targetPosition;
            }
            player.transform.position = new Vector3(47.34f, -37.55f, 0f);
        }
        public void SetCamPos5_4_3()
        {
            Camera targetCamera = sceneCamera != null ? sceneCamera : Camera.main;
            if (targetCamera != null)
            {
                Vector3 targetPosition = new Vector3(0f, -52.92f, -10f);
                targetCamera.transform.position = targetPosition;
            }
            player.transform.position = new Vector3(0.42f, -52.94f, 0f);
        }
        public void SetCamPos5_5_1()
        {
            Camera targetCamera = sceneCamera != null ? sceneCamera : Camera.main;
            if (targetCamera != null)
            {
                Vector3 targetPosition = new Vector3(21.55f, -52.92f, -10f);
                targetCamera.transform.position = targetPosition;
            }
            player.transform.position = new Vector3(28.59f, -51.7f, 0f);
        }
        public void SetCamPos5_5_3()
        {
            Camera targetCamera = sceneCamera != null ? sceneCamera : Camera.main;
            if (targetCamera != null)
            {
                Vector3 targetPosition = new Vector3(74.79f, -52.92f, -10f);
                targetCamera.transform.position = targetPosition;
            }
            player.transform.position = new Vector3(75.43f, -56.45f, 0f);
        }
        
        /// <summary>
        /// 动画接口：开启场景3租界入口的故事标题对话
        /// </summary>
        public void StartStage5Scene3Opening()
        {
            StartCoroutine(PlayStage5Scene3OpeningDialogue());
        }
        
        /// <summary>
        /// 动画接口：开启场景3-子场景1的背景介绍对话
        /// </summary>
        public void StartStage5_3_1Opening()
        {
            StartCoroutine(PlayStage5_3_1OpeningDialogue());
        }
        
        /// <summary>
        /// 动画接口：开启莉姆莉卡的问答对话（支持重复询问）
        /// </summary>
        public void StartLimlikaQADialogue()
        {
            StartCoroutine(PlayLimlikaQADialogue());
        }
        
        /// <summary>
        /// 动画接口：车夫过场动画中的对话
        /// 在购买车票播放"离开本场景"动画时调用
        /// </summary>
        public void StartCarriageTransitionDialogue()
        {
            StartCoroutine(PlayCarriageTransitionDialogue());
        }
        
        /// <summary>
        /// 动画接口：船上路线规划对话序列
        /// 包含魔将玄武的地图展示和路线说明，中间穿插动画
        /// </summary>
        public void StartRouteplanningDialogue()
        {
            StartCoroutine(PlayRouteplanningDialogue());
        }
        
        /// <summary>
        /// 动画接口：精灵王国禁城讨论对话序列
        /// 包含魔将玄武的历史回顾和战略分析，穿插多个地图动画
        /// </summary>
        public void StartElfKingdomDiscussion()
        {
            StartCoroutine(PlayElfKingdomDiscussionDialogue());
        }
        
        /// <summary>
        /// 动画接口：精灵女王大殿觐见对话序列
        /// 包含与契卡的对话、觐见女王、背包操作和多段动画
        /// </summary>
        public void StartElfQueenAudienceDialogue()
        {
            StartCoroutine(PlayElfQueenAudienceDialogue());
        }

        /// <summary>
        /// 动画接口：开启场景5-5-1的背景介绍对话
        /// </summary>
        public void StartStage5_5_1Opening()
        {
            StartCoroutine(PlayStage5_5_1OpeningDialogue());
        }
        
        /// <summary>
        /// 动画接口：开启Stage5-5对话序列
        /// 包含关于阿撒托斯笛声的神秘对话
        /// </summary>
        public void Start5_5Dialogue()
        {
            StartCoroutine(PlayStage5_5Dialogue());
        }
        
        /// <summary>
        /// 动画接口：与魔将玄武永别的对话序列
        /// 包含无名勇者的直觉和决心
        /// </summary>
        public void StartFarewellXuanwuDialogue()
        {
            StartCoroutine(PlayFarewellXuanwuDialogue());
        }
        
        /// <summary>
        /// 动画接口：王都怪异感受对话序列
        /// 包含莉姆莉卡的疑问和玩家的四个选择
        /// </summary>
        public void StartCapitalStrangenessDialogue()
        {
            StartCoroutine(PlayCapitalStrangenessDialogue());
        }
        
        private IEnumerator PlayStage5Scene3OpeningDialogue()
        {
            // 暂停玩家移动
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(false);
            }
            
            // 获取Stage5Controller来管理名称
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string playerName = stage5Controller != null ? stage5Controller.GetNPCName("我") : "无名勇者";
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            // 第一段对话
            List<DialogueData> openingDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(playerName, "按理来说，这个故事不应该有个名字什么的在刚才显示出来吗？"),
                DialogueSystem.CreateSubtitle(princessName, "故事吗？还没完成的事怎么能叫故事呢？不过，完成了也总要起个名字的。"),
                DialogueSystem.CreateSubtitle(princessName, "你觉得……这个故事该是什么标题呢？")
            };
            
            // 创建选项
            List<DialogueOption> storyTitleOptions = new List<DialogueOption>
            {
                DialogueSystem.CreateOption("《无名勇者与莉姆莉卡的冒险谭~反勇者与世界拯救的童话》", () => OnStoryTitleOptionA_Selected()),
                DialogueSystem.CreateOption("《爱、死亡与RPG——世界尽头与冷酷魅魔》", () => OnStoryTitleOptionB_Selected()),
                DialogueSystem.CreateOption("《无名勇者的无名故事》", () => OnStoryTitleOptionC_Selected()),
                DialogueSystem.CreateOption("《不过如此的小游戏》", () => OnStoryTitleOptionD_Selected())
            };
            
            // 创建带选项的对话
            DialogueData optionDialogue = DialogueSystem.CreateDialogueWithOptions(
                princessName,
                "选择一个你喜欢的标题：",
                storyTitleOptions
            );
            
            openingDialogue.Add(optionDialogue);
            
            // 开始对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(openingDialogue);
                // 等待对话完成由选项回调处理
            }
            else
            {
                // 如果对话系统不存在，直接恢复玩家移动
                if (playerController != null)
                {
                    playerController.SetCanMove(true);
                }
            }
            
            yield return null; // 添加这行来满足IEnumerator的返回值要求
        }
        
        private void OnStoryTitleOptionA_Selected()
        {
            string selectedTitle = "《无名勇者与莉姆莉卡的冒险谭~反勇者与世界拯救的童话》";
            UpdateStoryTitle(selectedTitle);
            
            // 修改属性：爱欲值+1，善恶值+1
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            if (stage5Controller != null)
            {
                stage5Controller.ModifyAttribute("lovedesire", 1);
                stage5Controller.ModifyAttribute("goodevil", 1);
            }
            
            // 显示后续对话
            StartCoroutine(ShowStoryTitleOptionAFollowUp());
        }
        
        private void OnStoryTitleOptionB_Selected()
        {
            string selectedTitle = "《爱、死亡与RPG——世界尽头与冷酷魅魔》";
            UpdateStoryTitle(selectedTitle);
            
            // 修改属性：爱欲值+1，真理值+1
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            if (stage5Controller != null)
            {
                stage5Controller.ModifyAttribute("lovedesire", 1);
                stage5Controller.ModifyAttribute("truth", 1);
            }
            
            // 显示后续对话
            StartCoroutine(ShowStoryTitleOptionBFollowUp());
        }
        
        private void OnStoryTitleOptionC_Selected()
        {
            string selectedTitle = "《无名勇者的无名故事》";
            UpdateStoryTitle(selectedTitle);
            
            // 修改属性：真理值+1，爱欲值-1
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            if (stage5Controller != null)
            {
                stage5Controller.ModifyAttribute("truth", 1);
                stage5Controller.ModifyAttribute("lovedesire", -1);
            }
            
            // 显示后续对话
            StartCoroutine(ShowStoryTitleOptionCFollowUp());
        }
        
        private void OnStoryTitleOptionD_Selected()
        {
            string selectedTitle = "《不过如此的小游戏》";
            UpdateStoryTitle(selectedTitle);
            
            // 修改属性：爱欲值-1，善恶值-1
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            if (stage5Controller != null)
            {
                stage5Controller.ModifyAttribute("lovedesire", -1);
                stage5Controller.ModifyAttribute("goodevil", -1);
            }
            
            // 显示后续对话
            StartCoroutine(ShowStoryTitleOptionDFollowUp());
        }
        
        private void UpdateStoryTitle(string title)
        {
            // 更新AnimationInterface中的标题文本（如果设置了）
            if (storyTitleText != null)
            {
                storyTitleText.text = title;
            }
            
            // 更新Stage5Controller中的标题文本
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            if (stage5Controller != null)
            {
                stage5Controller.SetStoryTitle(title);
            }
        }
        
        private IEnumerator ShowStoryTitleOptionAFollowUp()
        {
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            List<DialogueData> followUpDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(princessName, "这个也太普通了吧，不过我们是拯救世界的主角诶，就这样吧。")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(followUpDialogue, OnStoryTitleDialogueComplete);
            }
            else
            {
                OnStoryTitleDialogueComplete();
            }
            
            yield return null;
        }
        
        private IEnumerator ShowStoryTitleOptionBFollowUp()
        {
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            List<DialogueData> followUpDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(princessName, "我们确实是从世界尽头出发的但是……难道说我是冷酷魅魔吗？"),
                DialogueSystem.CreateSubtitle(princessName, "我对待你太冷酷了吗？")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(followUpDialogue, OnStoryTitleDialogueComplete);
            }
            else
            {
                OnStoryTitleDialogueComplete();
            }
            
            yield return null;
        }
        
        private IEnumerator ShowStoryTitleOptionCFollowUp()
        {
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            List<DialogueData> followUpDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(princessName, "感觉好平庸的标题，你喜欢这样的吗？"),
                DialogueSystem.CreateSubtitle(princessName, "而且标题里没有我诶~")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(followUpDialogue, OnStoryTitleDialogueComplete);
            }
            else
            {
                OnStoryTitleDialogueComplete();
            }
            
            yield return null;
        }
        
        private IEnumerator ShowStoryTitleOptionDFollowUp()
        {
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            List<DialogueData> followUpDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(princessName, "完全不知所云……你认真的吗？")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(followUpDialogue, OnStoryTitleDialogueComplete);
            }
            else
            {
                OnStoryTitleDialogueComplete();
            }
            
            yield return null;
        }
        
        private void OnStoryTitleDialogueComplete()
        {
            // 继续播放结尾对话
            StartCoroutine(ShowEndingDialogue());
        }
        
        private IEnumerator ShowEndingDialogue()
        {
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string playerName = stage5Controller != null ? stage5Controller.GetNPCName("我") : "无名勇者";
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            List<DialogueData> endingDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(playerName, "如果是大制作这个时候应该有片头曲了。"),
                DialogueSystem.CreateSubtitle(princessName, "打住！打住！魔王城已经把最后一点资源用来召唤你了，没有余钱搞其他的了。")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(endingDialogue, OnCompleteOpening);
            }
            else
            {
                OnCompleteOpening();
            }
            
            yield return null;
        }
        
        private void OnCompleteOpening()
        {
            // 恢复玩家移动
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(true);
            }
        }
        
        private IEnumerator PlayStage5_3_1OpeningDialogue()
        {
            // 暂停玩家移动
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(false);
            }
            
            // 获取Stage5Controller来管理名称
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string playerName = stage5Controller != null ? stage5Controller.GetNPCName("我") : "无名勇者";
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            // 第一段对话（动画前）
            List<DialogueData> firstPartDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(princessName, "这里就是矮人的城市了，矮人基本聚居在北部的山区，他们擅长开凿矿石、冶炼金属、以及编写仇恨之书。"),
                DialogueSystem.CreateSubtitle(playerName, "不知道为甚么，感觉我好像其实知道这些。"),
                DialogueSystem.CreateSubtitle(princessName, "那难道你以前来到过这个世界？"),
                DialogueSystem.CreateSubtitle(playerName, "那种时空轮回的俗套故事应该也不是送我来此的神明想看到的。"),
                DialogueSystem.CreateSubtitle(princessName, "也是，召唤法术都是向神要人，神明同意才会送你过来的……"),
                DialogueSystem.CreateSubtitle(princessName, "所以你是怎么过来的？神明给你说什么了吗？"),
                DialogueSystem.CreateSubtitle(playerName, "我不知道，但我知道我不知道，"),
                DialogueSystem.CreateSubtitle(princessName, "啊……真没意思……"),
                DialogueSystem.CreateSubtitle(princessName, "算了，继续和你介绍到底发生了什么吧……")
            };
            
            // 播放第一段对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(firstPartDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 播放动画Stage5-3-2
            yield return StartCoroutine(PlayStage5_3_2Animation());
            
            // 第二段对话（动画后）
            List<DialogueData> secondPartDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(princessName, "魔王波旬，是我的祖父。他统一了南北魔族，建立了强大的军队，侵略了人类的领土，"),
                DialogueSystem.CreateSubtitle(princessName, "一开始，进军非常顺利，可以说差一点魔物军就可以攻下人类王都。"),
                DialogueSystem.CreateSubtitle(princessName, "然后人类召唤了勇者，就是那个鸭舌帽勇者，有神给他的武器，一人成军，所向披靡。"),
                DialogueSystem.CreateSubtitle(princessName, "然后祖父就死在了勇者剑下，魔物军几乎溃败，只有十分之一逃回了本土。"),
                DialogueSystem.CreateSubtitle(princessName, "勇者当时写了一句诗，说什么‘宜将剩勇追穷寇，不可沽名学霸王’。"),
                DialogueSystem.CreateSubtitle(princessName, "然后他们追到本土，最后把我们赶到了小岛上，然后鸭舌帽勇者完成任务离开了这个世界。"),
                DialogueSystem.CreateSubtitle(princessName, "凭借剩余的力量我们支撑残岛抵御了几十年，终于是抵挡不住了。"),
                DialogueSystem.CreateSubtitle(princessName, "所以最后我们决定也召唤一个勇者来，恢复魔族的往日荣光。")
            };
            
            // 播放第二段对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(secondPartDialogue, OnStage5_3_1DialogueComplete);
            }
            
            yield return null;
        }
        
        private IEnumerator PlayStage5_3_2Animation()
        {
            // 查找Stage5Controller来播放动画
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            if (stage5Controller != null && stage5Controller.transitionAnimator != null)
            {
                // 播放指定动画
                stage5Controller.transitionAnimator.Play("Stage5-3-2");
                
                // 等待动画播放完成
                yield return StartCoroutine(WaitForAnimationComplete(stage5Controller.transitionAnimator, "Stage5-3-2"));
            }
            else
            {
                // 如果没有动画器，等待一小段时间作为替代
                yield return new WaitForSeconds(2f);
            }
        }
        
        private IEnumerator WaitForAnimationComplete(Animator animator, string animationName)
        {
            // 等待动画开始播放
            yield return new WaitForEndOfFrame();
            
            // 获取动画长度
            float animationTime = 3f; // 默认时长
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName(animationName))
            {
                animationTime = stateInfo.length;
            }
            
            // 等待动画播放完成
            float elapsedTime = 0f;
            while (elapsedTime < animationTime)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 确保动画完全播放完成
            yield return new WaitForEndOfFrame();
        }
        
        public void OnStage5_3_1DialogueComplete()
        {
            // 第二段对话结束后，播放Stage5-3-3动画
            // StartCoroutine(PlayStage5_3_3AnimationAndFinish()); // 已注释：改为在动画中手动控制
            
            // 恢复玩家移动
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(true);
            }
        }

        /// <summary>
        /// 播放Stage5-3-3动画并在完成后触发莉姆莉卡问答对话
        /// </summary>
        public void FUCK()
        {
            StartCoroutine(PlayStage5_3_3AnimationWithQADialogue());
        }
        
        /// <summary>
        /// 播放Stage5-3-3动画，完成后自动触发莉姆莉卡问答对话
        /// </summary>
        private IEnumerator PlayStage5_3_3AnimationWithQADialogue()
        {
            // 暂停玩家移动
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(false);
            }
            
            // 查找Stage5Controller来播放动画
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            if (stage5Controller != null && stage5Controller.transitionAnimator != null)
            {
                // 播放指定动画
                stage5Controller.transitionAnimator.Play("Stage5-3-3");
                
                // 等待动画播放完成
                yield return StartCoroutine(WaitForAnimationComplete(stage5Controller.transitionAnimator, "Stage5-3-3"));
            }
            else
            {
                // 如果没有动画器，等待一小段时间作为替代
                yield return new WaitForSeconds(2f);
            }
            
            // 动画播放完成后，自动触发莉姆莉卡问答对话
            yield return StartCoroutine(PlayLimlikaQADialogue());
        }
        
        private IEnumerator PlayStage5_3_3AnimationAndFinish()
        {
            // 查找Stage5Controller来播放动画
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            if (stage5Controller != null && stage5Controller.transitionAnimator != null)
            {
                // 播放指定动画
                stage5Controller.transitionAnimator.Play("Stage5-3-3");
                
                // 等待动画播放完成
                yield return StartCoroutine(WaitForAnimationComplete(stage5Controller.transitionAnimator, "Stage5-3-3"));
            }
            else
            {
                // 如果没有动画器，等待一小段时间作为替代
                yield return new WaitForSeconds(2f);
            }
            
            // 动画播放完成后，恢复玩家移动
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(true);
            }
        }
        
        private IEnumerator PlayLimlikaQADialogue()
        {
            // 防重复触发：若当前已有对话正在进行（可能来自动画事件并发触发），则不再启动新的问答对话
            if (DialogueSystem.Instance != null && DialogueSystem.Instance.isDialogueActive)
            {
                yield break;
            }

            // 暂停玩家移动
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(false);
            }
            
            // 获取Stage5Controller来管理名称
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            // 主问答对话
            List<DialogueData> qaDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(princessName, "还有什么要我解释的吗？")
            };
            
            // 创建选项
            List<DialogueOption> qaOptions = new List<DialogueOption>
            {
                DialogueSystem.CreateOption("为什么你不叫作波勺，反而叫莉姆莉卡。", () => OnQAOptionA_Selected()),
                DialogueSystem.CreateOption("鸭舌帽勇者真的已经回去了吗？", () => OnQAOptionB_Selected()),
                DialogueSystem.CreateOption("所以我们去哪里赚钱？", () => OnQAOptionC_Selected()),
                DialogueSystem.CreateOption("我的有些问题，现在还问不出口。你觉得这个计划怎么样？", () => OnQAOptionD_Selected()),
                DialogueSystem.CreateOption("没有什么好问的了。", () => OnQAOptionE_Selected())
            };
            
            // 创建带选项的对话
            DialogueData optionDialogue = DialogueSystem.CreateDialogueWithOptions(
                princessName,
                "选择一个问题：",
                qaOptions
            );
            
            qaDialogue.Add(optionDialogue);
            
            // 开始对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(qaDialogue);
                // 等待对话完成由选项回调处理
            }
            else
            {
                // 如果对话系统不存在，直接恢复玩家移动
                if (playerController != null)
                {
                    playerController.SetCanMove(true);
                }
            }
            
            yield return null;
        }
        
        private void OnQAOptionA_Selected()
        {
            // 只在首次选择时修改属性
            if (!hasSelectedQAOption)
            {
                Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
                if (stage5Controller != null)
                {
                    stage5Controller.ModifyAttribute("lovedesire", 1);
                    stage5Controller.ModifyAttribute("truth", 1);
                }
                hasSelectedQAOption = true;
            }
            
            // 显示后续对话
            StartCoroutine(ShowQAOptionAFollowUp());
        }
        
        private void OnQAOptionB_Selected()
        {
            // 只在首次选择时修改属性
            if (!hasSelectedQAOption)
            {
                Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
                if (stage5Controller != null)
                {
                    stage5Controller.ModifyAttribute("truth", 1);
                }
                hasSelectedQAOption = true;
            }
            
            // 显示后续对话
            StartCoroutine(ShowQAOptionBFollowUp());
        }
        
        private void OnQAOptionC_Selected()
        {
            // 只在首次选择时修改属性
            if (!hasSelectedQAOption)
            {
                Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
                if (stage5Controller != null)
                {
                    stage5Controller.ModifyAttribute("goodevil", -1);
                    stage5Controller.ModifyAttribute("truth", -1);
                }
                hasSelectedQAOption = true;
            }
            
            // 显示后续对话
            StartCoroutine(ShowQAOptionCFollowUp());
        }
        
        private void OnQAOptionD_Selected()
        {
            // 只在首次选择时修改属性
            if (!hasSelectedQAOption)
            {
                Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
                if (stage5Controller != null)
                {
                    stage5Controller.ModifyAttribute("truth", 1);
                    stage5Controller.ModifyAttribute("goodevil", 1);
                }
                hasSelectedQAOption = true;
            }
            
            // 显示后续对话
            StartCoroutine(ShowQAOptionDFollowUp());
        }
        
        private void OnQAOptionE_Selected()
        {
            // 只在首次选择时修改属性
            if (!hasSelectedQAOption)
            {
                Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
                if (stage5Controller != null)
                {
                    stage5Controller.ModifyAttribute("truth", -2);
                    stage5Controller.ModifyAttribute("goodevil", -1);
                }
                hasSelectedQAOption = true;
            }
            
            // 显示后续对话
            StartCoroutine(ShowQAOptionEFollowUp());
        }
        
        private IEnumerator ShowQAOptionAFollowUp()
        {
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            List<DialogueData> followUpDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(princessName, "因为波勺已经在撤离途中失踪了，他是我哥哥。"),
                DialogueSystem.CreateSubtitle(princessName, "而且儿子继承父亲名号，女儿继承母亲名号不是常识么？"),
                DialogueSystem.CreateSubtitle(princessName, "不然世界上的姓氏会越来越少的，这怎么可以嘛？"),
                DialogueSystem.CreateSubtitle(princessName, "以及防止你多问，曾祖父的名字叫做波甸。")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(followUpDialogue, () => StartCoroutine(PlayLimlikaQADialogue()));
            }
            else
            {
                StartCoroutine(PlayLimlikaQADialogue());
            }
            
            yield return null;
        }
        
        private IEnumerator ShowQAOptionBFollowUp()
        {
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            List<DialogueData> followUpDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(princessName, "不知道，反正自那以后没人再见过他了。"),
                DialogueSystem.CreateSubtitle(princessName, "也没人知道他是怎么离开的，毕竟我也不懂召唤到底是怎么一回事。"),
                DialogueSystem.CreateSubtitle(princessName, "再说了，他要是还在我们几十年前就没有啦。")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(followUpDialogue, () => StartCoroutine(PlayLimlikaQADialogue()));
            }
            else
            {
                StartCoroutine(PlayLimlikaQADialogue());
            }
            
            yield return null;
        }
        
        private IEnumerator ShowQAOptionCFollowUp()
        {
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            string playerName = stage5Controller != null ? stage5Controller.GetNPCName("我") : "无名勇者";
            
            // 获取玩家当前金钱数
            int currentMoney = stage5Controller != null ? stage5Controller.money : 0;
            
            List<DialogueData> followUpDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(princessName, "去街上找一些看上去人傻钱多的，然后交给我。"),
                DialogueSystem.CreateSubtitle(princessName, "或者你很蠢的话还可以去矿区搭把手，用体力换钱。"),
                DialogueSystem.CreateSubtitle(princessName, "我身上可是一分钱都没有带，你应该有带钱吧？"),
                DialogueSystem.CreateSubtitle(playerName, $"……{currentMoney}G"),
                DialogueSystem.CreateSubtitle(princessName, "唉……没办法，谁叫我们落魄了呢？")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(followUpDialogue, () => StartCoroutine(PlayLimlikaQADialogue()));
            }
            else
            {
                StartCoroutine(PlayLimlikaQADialogue());
            }
            
            yield return null;
        }
        
        private IEnumerator ShowQAOptionDFollowUp()
        {
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            List<DialogueData> followUpDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(princessName, "无非是成王败寇的游戏罢了，只是神明不愿看到无聊的结局。"),
                DialogueSystem.CreateSubtitle(princessName, "再往前魔族的分裂也是人类入侵导致的。"),
                DialogueSystem.CreateSubtitle(princessName, "魔族一直内耗人类才得有几百年的好时代。"),
                DialogueSystem.CreateSubtitle(princessName, "再再往前，既有魔族主导的时期，也有精灵主导的时期，还有矮人主导的时期，只是比较短。"),
                DialogueSystem.CreateSubtitle(princessName, "如果没有神明的旨意，我们也只是这个循环往复过程中的一粒沙而已。"),
                DialogueSystem.CreateSubtitle(princessName, "一直到最早世界诞生之初，神明就设下了四种族的先祖，约定了谁也不可杀死谁。"),
                DialogueSystem.CreateSubtitle(princessName, "所以世界始终无法统一，这既是神明的仁慈，也是祂的无上残忍。"),
                DialogueSystem.CreateSubtitle(princessName, "啊……不好意思，说多了~你不在意吧。")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(followUpDialogue, () => StartCoroutine(PlayLimlikaQADialogue()));
            }
            else
            {
                StartCoroutine(PlayLimlikaQADialogue());
            }
            
            yield return null;
        }
        
        private IEnumerator ShowQAOptionEFollowUp()
        {
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            List<DialogueData> followUpDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(princessName, "那就这样吧，我们在镇子里逛一逛。")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(followUpDialogue, OnQADialogueComplete);
            }
            else
            {
                OnQADialogueComplete();
            }
            
            yield return null;
        }
        
        private void OnQADialogueComplete()
        {
            // 恢复玩家移动
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(true);
            }
        }
        
        /// <summary>
        /// 车夫过场动画对话序列
        /// </summary>
        private IEnumerator PlayCarriageTransitionDialogue()
        {
            // 暂停玩家移动（动画期间通常已经暂停，但为了保险起见）
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(false);
            }
            
            // 获取Stage5Controller来管理名称
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string playerName = stage5Controller != null ? stage5Controller.GetNPCName("我") : "无名勇者";
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            // 车夫过场对话
            List<DialogueData> carriageDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(princessName, "马车真是颠簸啊~"),
                DialogueSystem.CreateSubtitle(playerName, "接下来是要靠北部航线去矮人国吗？"),
                DialogueSystem.CreateSubtitle(playerName, "说起来我还不知道这个世界的地图是什么样的。"),
                DialogueSystem.CreateSubtitle("魔将玄武", "来自异世界的勇者不了解也正常。"),
                DialogueSystem.CreateSubtitle("魔将玄武", "这人多，我不方便说话，到船上我来介绍。")
            };
            
            // 播放对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(carriageDialogue, OnCarriageTransitionDialogueComplete);
            }
            else
            {
                OnCarriageTransitionDialogueComplete();
            }
            
            yield return null;
        }
        
        /// <summary>
        /// 车夫过场动画对话完成回调
        /// </summary>
        private void OnCarriageTransitionDialogueComplete()
        {
            // 恢复玩家移动
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(true);
            }
            
            // 这里可以添加其他完成后的逻辑，比如：
            // - 场景切换
            // - 传送到码头
            // - 播放下一段动画等
        }
        
        /// <summary>
        /// 船上路线规划对话序列
        /// 包含地图展示动画和路线说明
        /// </summary>
        private IEnumerator PlayRouteplanningDialogue()
        {
            // 暂停玩家移动
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(false);
            }
            
            // 获取Stage5Controller来管理名称
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string playerName = stage5Controller != null ? stage5Controller.GetNPCName("我") : "无名勇者";
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            // 第一段对话（动画4_0前）
            List<DialogueData> firstPartDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("魔将玄武", "将军莫忧，且看此图。")
            };
            
            // 播放第一段对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(firstPartDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 播放动画4_0
            yield return StartCoroutine(PlayAnimationById("4_0"));
            
            // 第二段对话（动画4_0后，动画4_1前）
            List<DialogueData> secondPartDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("魔将玄武", "我们的目标是前往人类王都，阻止国王的赶尽杀绝。"),
                DialogueSystem.CreateSubtitle("魔将玄武", "也就是说，我们的路线应该是这样的。")
            };
            
            // 播放第二段对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(secondPartDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 播放动画4_6
            yield return StartCoroutine(PlayAnimationById("4_6"));
            
            // 第三段对话（动画4_1后，完整的路费讨论）
            List<DialogueData> thirdPartDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("魔将玄武", "从北部航线，我们绕过了人类的占领区。"),
                DialogueSystem.CreateSubtitle("魔将玄武", "然后向南穿越矮人国的山脉进入中央精灵领。"),
                DialogueSystem.CreateSubtitle("魔将玄武", "从精灵领穿越到王国腹地，取最短的路线混入王都。"),
                DialogueSystem.CreateSubtitle(playerName, "那就这么办吧。"),
                DialogueSystem.CreateSubtitle(princessName, "说起来，路费怎么办呢？"),
                DialogueSystem.CreateSubtitle(playerName, "让玄武假装预言水晶，骗人占卜赚钱就好。"),
                DialogueSystem.CreateSubtitle(princessName, "可是他就是一块石板了，怎么看都不是水晶。"),
                DialogueSystem.CreateSubtitle(playerName, "他当底座，放个玻璃球在上面就好了，没人在意到底是哪部分在起作用。"),
                DialogueSystem.CreateSubtitle("魔将玄武", "唉……"),
                DialogueSystem.CreateSubtitle("魔将玄武", "虎落平阳啊……"),
                DialogueSystem.CreateSubtitle(princessName, "精灵应该不会不准我们入境吧，哥哥。"),
                DialogueSystem.CreateSubtitle(playerName, "我不知道，车到山前必有路吧。"),
                DialogueSystem.CreateSubtitle("魔将玄武", "……"),
                DialogueSystem.CreateSubtitle("魔将玄武", "我只是货物。"),
                DialogueSystem.CreateInteractionHint("现在点击魔将玄武可以赚钱了！")
            };
            stage5Controller.SetCanUseMJXW(true);
            // 播放动画ClearCG
            yield return StartCoroutine(PlayAnimationById("ClearCG"));
            
            // 播放第三段对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(thirdPartDialogue, OnRouteplanningDialogueComplete);
            }
            else
            {
                OnRouteplanningDialogueComplete();
            }
            
            yield return null;
        }
        
        /// <summary>
        /// 通用动画播放方法
        /// </summary>
        private IEnumerator PlayAnimationById(string animationId)
        {
            // 查找Stage5Controller来播放动画
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            if (stage5Controller != null && stage5Controller.transitionAnimator != null)
            {
                // 播放指定动画
                stage5Controller.transitionAnimator.Play(animationId);
                
                // 等待动画播放完成
                yield return StartCoroutine(WaitForAnimationComplete(stage5Controller.transitionAnimator, animationId));
            }
            else
            {
                // 如果没有动画器，等待一小段时间作为替代
                yield return new WaitForSeconds(2f);
            }
        }
        
        /// <summary>
        /// 路线规划对话完成回调
        /// </summary>
        private void OnRouteplanningDialogueComplete()
        {
            // 恢复玩家移动
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(true);
            }
            
            // 这里可以添加其他完成后的逻辑，比如：
            // - 更新游戏状态
            // - 解锁新的交互选项
            // - 场景切换等
        }
        
        /// <summary>
        /// 精灵王国禁城讨论对话序列
        /// 包含魔将玄武的历史回顾和战略分析，穿插多个地图动画
        /// </summary>
        private IEnumerator PlayElfKingdomDiscussionDialogue()
        {
            // 暂停玩家移动
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(false);
            }
            
            // 获取Stage5Controller来管理名称
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string playerName = stage5Controller != null ? stage5Controller.GetNPCName("我") : "无名勇者";
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            // 第一段对话（动画4_2前）
            List<DialogueData> firstPartDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("魔将玄武", "前面就是禁城了"),
                DialogueSystem.CreateSubtitle("魔将玄武", "精灵女王就在里面"),
                DialogueSystem.CreateSubtitle("魔将玄武", "现在得想个由头见到她"),
                DialogueSystem.CreateSubtitle(princessName, "为什么啊？暴露自己吗？"),
                DialogueSystem.CreateSubtitle("魔将玄武", "我听说人类和精灵的关系急剧恶化了"),
                DialogueSystem.CreateSubtitle("魔将玄武", "这个时候表明我们的立场，可以争取她的支持"),
                DialogueSystem.CreateSubtitle("魔将玄武", "她活了几百年了，识得大体"),
                DialogueSystem.CreateSubtitle(playerName, "何为大体？"),
                DialogueSystem.CreateSubtitle("魔将玄武", "天下。"),
                DialogueSystem.CreateSubtitle(princessName, "说人话。"),
                DialogueSystem.CreateSubtitle("魔将玄武", "公主莫急，且看此图。")
            };
            
            // 播放第一段对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(firstPartDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 播放动画4_2（魔族统一前地图）
            yield return StartCoroutine(PlayAnimationById("4_2"));
            
            // 第二段对话（动画4_2后，动画4_3前）
            List<DialogueData> secondPartDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("魔将玄武", "这是200年前波旬魔王尚未上位，波甸北魔王在位时的地图。")
            };
            
            // 播放第二段对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(secondPartDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 播放动画4_3（显示魔族统一后地图）
            yield return StartCoroutine(PlayAnimationById("4_3"));
            
            // 第三段对话（动画4_3后，动画4_4前）
            List<DialogueData> thirdPartDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("魔将玄武", "波旬魔王统一了南北魔族之后，为生存空间，发动了远征。")
            };
            
            // 播放第三段对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(thirdPartDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 播放动画4_4（显示进军图）
            yield return StartCoroutine(PlayAnimationById("4_4"));
            
            // 第四段对话（动画4_4后，动画4_5前）
            List<DialogueData> fourthPartDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("魔将玄武", "对人类的攻势原本十分顺利，但是在第三阶段，精灵背弃了不干扰约定，主动出击。"),
                DialogueSystem.CreateSubtitle("魔将玄武", "这大大打乱了我们的部署，导致人类王都久攻不下，也给了他们召唤勇者的时间。")
            };
            
            // 播放第四段对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(fourthPartDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 播放动画4_5（显示退却图）
            yield return StartCoroutine(PlayAnimationById("4_5"));
            
            // 第五段对话（最终对话，动画4_5后）
            List<DialogueData> finalPartDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("魔将玄武", "之后的事，就是兵败如山倒，波旬被勇者杀死，波句失踪，最后被反攻本土，固守离岛。"),
                DialogueSystem.CreateSubtitle(playerName, "不干扰约定？"),
                DialogueSystem.CreateSubtitle("魔将玄武", "魔族与精灵矮人约定，此次进军只涉及人类。"),
                DialogueSystem.CreateSubtitle("魔将玄武", "我们不会攻击他们，他们要不要妨碍我们。"),
                DialogueSystem.CreateSubtitle("魔将玄武", "现在想想，真是太愚蠢了，人类一旦灭亡，精灵矮人便唇亡齿寒，"),
                DialogueSystem.CreateSubtitle("魔将玄武", "此后精灵就找了借口说我们误伤了哨兵，对我们发动了蓄谋已久的偷袭。"),
                DialogueSystem.CreateSubtitle(playerName, "……"),
                DialogueSystem.CreateSubtitle(playerName, "我有很多话想说，但现在不是时候。"),
                DialogueSystem.CreateSubtitle(playerName, "那这和识大体，又有什么关系呢？"),
                DialogueSystem.CreateSubtitle("魔将玄武", "当初女王为了防止一族独大，以免威胁自己，于是愿意用兵"),
                DialogueSystem.CreateSubtitle("魔将玄武", "如今人类独大，与精灵摩擦日益提高，那么协助魔族的复苏以制衡人类"),
                DialogueSystem.CreateSubtitle("魔将玄武", "自然就是一个很好的选项。"),
                DialogueSystem.CreateSubtitle("魔将玄武", "毕竟魔族复苏还没那没快，但人类的贪欲缺不会停止一天。"),
                DialogueSystem.CreateSubtitle(playerName, "就先相信你的判断，那我们要如何见女王呢？"),
                DialogueSystem.CreateSubtitle("魔将玄武", "船到桥头自然直嘛……"),
                DialogueSystem.CreateSubtitle(princessName, "真让人担心……")
            };

            yield return StartCoroutine(PlayAnimationById("ClearCG"));
            
            // 播放最终对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(finalPartDialogue, OnElfKingdomDiscussionComplete);
            }
            else
            {
                OnElfKingdomDiscussionComplete();
            }
            
            yield return null;
        }
        
        /// <summary>
        /// 精灵王国禁城讨论对话完成回调
        /// </summary>
        private void OnElfKingdomDiscussionComplete()
        {
            // 恢复玩家移动
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(true);
            }
            
            // 这里可以添加其他完成后的逻辑，比如：
            // - 更新游戏状态
            // - 解锁新的交互选项
            // - 进入精灵王国等
        }
        
        /// <summary>
        /// 精灵女王大殿觐见对话序列实现
        /// 包含复杂的对话流程、动画插入和背包操作
        /// </summary>
        private IEnumerator PlayElfQueenAudienceDialogue()
        {
            // 暂停玩家移动
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(false);
            }
            
            // 获取Stage5Controller来管理名称
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string playerName = stage5Controller != null ? stage5Controller.GetNPCName("我") : "无名勇者";
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            // 第一段对话：与契卡的对话
            List<DialogueData> firstPartDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("契卡", "你们真是帮了我大忙了！"),
                DialogueSystem.CreateSubtitle("契卡", "真不知道怎么感谢你们才好！"),
                DialogueSystem.CreateSubtitle(princessName, "不用在意，您引荐我见女王我也不知道怎么感谢您了。"),
                DialogueSystem.CreateSubtitle("契卡", "说起来，莉姆莉卡阁下器宇不凡，想必身份不凡。"),
                DialogueSystem.CreateSubtitle("契卡", "不知可否为我介绍您的光荣身份，我好为女王介绍。"),
                DialogueSystem.CreateSubtitle(princessName, "落魄的贵族罢了，好在善人提携，才有这次光荣的机会觐见女王。"),
                DialogueSystem.CreateSubtitle("契卡", "哈哈，您客气了。"),
                DialogueSystem.CreateSubtitle("契卡", "和您的交流如此愉快，时间过得飞快。"),
                DialogueSystem.CreateSubtitle("契卡", "你看，这里就是女王殿了。"),
                DialogueSystem.CreateSubtitle("契卡", "等会我先进去介绍，然后你们带着礼物进来就好。"),
                DialogueSystem.CreateSubtitle("契卡", "我们都是贵族，不用行跪拜礼，不过你的这位随从要注意礼节。"),
                DialogueSystem.CreateSubtitle(playerName, "好……好的。"),
                DialogueSystem.CreateSceneDescription("契卡独自一人进入了女王殿。"),
                DialogueSystem.CreateSubtitle("魔将玄武", "怎……怎么办？"),
                DialogueSystem.CreateSubtitle(princessName, "船到桥头自然直，车到山前必有路~"),
                DialogueSystem.CreateSubtitle("魔将玄武", "唉……为了大局，万死不辞……"),
                DialogueSystem.CreateSubtitle("殿前侍卫", "莉姆莉卡阁下，女王殿下召见！")
            };
            
            // 播放第一段对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(firstPartDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 播放进入大殿动画
            yield return StartCoroutine(PlayEnterThroneRoomAnimation());
            
            // 继续后续流程
            yield return StartCoroutine(PlaySecondPartAudienceDialogue());
            
            yield return null;
        }
        
        /// <summary>
        /// 第二部分对话：进入大殿后的对话流程
        /// </summary>
        private IEnumerator PlaySecondPartAudienceDialogue()
        {
            // 获取Stage5Controller来管理名称
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            // 第二段对话：进入大殿后的对话
            List<DialogueData> secondPartDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(princessName, "在下魔族公主，莉姆莉卡·莉莉姆·莉莉卡·莉莉丝见过女王大人~"),
                DialogueSystem.CreateSubtitle("契卡", "啊？你……你是……？！"),
                DialogueSystem.CreateSubtitle(princessName, "如契卡阁下所言，我等为女王大人进献秘宝——"),
                DialogueSystem.CreateSubtitle(princessName, "拥有说话与预言能力的石板~"),
                DialogueSystem.CreateSubtitle(princessName, "望您笑纳~"),
                DialogueSystem.CreateSubtitle("契卡", "啊~完……完蛋……了"),
                DialogueSystem.CreateSceneDescription("契卡晕过去了"),
                DialogueSystem.CreateSubtitle("精灵女王", "来人，把契卡送去太医院。"),
                DialogueSystem.CreateSubtitle("精灵女王", "他惊吓过度，要好生伺候。"),
                DialogueSystem.CreateSubtitle("殿前侍卫", "诺！")
            };
            
            // 播放第二段对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(secondPartDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 播放抬走契卡动画
            yield return StartCoroutine(PlayCarryAwayChikaAnimation());
            
            // 继续第三部分
            yield return StartCoroutine(PlayThirdPartAudienceDialogue());
        }
        
        /// <summary>
        /// 第三部分对话：献出魔将玄武的流程
        /// </summary>
        private IEnumerator PlayThirdPartAudienceDialogue()
        {
            // 获取Stage5Controller来管理名称
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            // 第三段对话：抬走契卡后的对话
            List<DialogueData> thirdPartDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("精灵女王", "莉姆莉卡，久仰大名了，劳烦魔族的公主亲自送礼，这个面子我可不敢不要。"),
                DialogueSystem.CreateSubtitle("精灵女王", "把秘宝拿过来吧。"),
                DialogueSystem.CreateSubtitle(princessName, "请过目~")
            };
            
            // 播放第三段对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(thirdPartDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 从背包中移除魔将玄武
            RemoveXuanwuFromInventory();
            
            // 第四段对话：献出魔将玄武
            List<DialogueData> fourthPartDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("魔将玄武", "你……你好啊？"),
                DialogueSystem.CreateSubtitle("精灵女王", "呵~真是劳烦你们这么有心了，那我也应该拿出相应规格对待了……"),
                DialogueSystem.CreateSubtitle("精灵女王", "左右，所有卫兵侍从都先退下，守住门口，没有命令一只鸟也不能进来。"),
                DialogueSystem.CreateSubtitle("精灵女王", "以及今天这里发生的事，一个字也不能泄露出去。"),
                DialogueSystem.CreateSubtitle("殿前侍卫", "诺！")
            };
            
            // 播放第四段对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(fourthPartDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 播放侍卫退场动画
            yield return StartCoroutine(PlayGuardsExitAnimation());
            
            // 继续选项对话部分
            yield return StartCoroutine(PlayOptionsPartAudienceDialogue());
        }
        
        /// <summary>
        /// 选项对话部分：包含四个选项的对话
        /// </summary>
        private IEnumerator PlayOptionsPartAudienceDialogue()
        {
            // 第五段对话：侍卫退场后，包含选项
            List<DialogueData> fifthPartDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("精灵女王", "现在……你们可以直说了。")
            };
            
            // 创建选项
            List<DialogueOption> audienceOptions = new List<DialogueOption>
            {
                DialogueSystem.CreateOption("介绍复兴魔族的大计", () => OnAudienceOptionA_Selected()),
                DialogueSystem.CreateOption("介绍当前精人的局势", () => OnAudienceOptionB_Selected()),
                DialogueSystem.CreateOption("要求魔将玄武劝说", () => OnAudienceOptionC_Selected()),
                DialogueSystem.CreateOption("提示莉姆莉卡解释", () => OnAudienceOptionD_Selected())
            };
            
            // 创建带选项的对话
            DialogueData optionDialogue = DialogueSystem.CreateDialogueWithOptions(
                "精灵女王",
                "你们想要说什么？",
                audienceOptions
            );
            
            fifthPartDialogue.Add(optionDialogue);
            
            // 播放第五段对话（包含选项）
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(fifthPartDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 选项处理完成后，继续最终对话
            yield return StartCoroutine(PlayFinalAudienceDialogue());
        }
        
        /// <summary>
        /// 最终对话序列
        /// </summary>
        private IEnumerator PlayFinalAudienceDialogue()
        {
            // 获取Stage5Controller来管理名称
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            // 最终对话
            List<DialogueData> finalDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("精灵女王", "你们说的我都知道了。但这一切都是贪欲所起：魔王贪求不属于他的土地，人王贪求不属于他的寿命。一个造就了你们困守离岛，一个导致了如今世界疯狂。"),
                DialogueSystem.CreateSubtitle("精灵女王", "……"),
                DialogueSystem.CreateSubtitle("精灵女王", "我亦贪也，如今报应不爽……你们还有你们的使命完成，这块石头拿回去，他不该留在这里。"),
                DialogueSystem.CreateSubtitle(princessName, "您的打算究竟是？"),
                DialogueSystem.CreateSubtitle("精灵女王", "我知道你们想要精人矛盾公开化，但是那样完全就是弃精灵四地百万生灵于不顾。"),
                DialogueSystem.CreateSubtitle("精灵女王", "而且这种大动作不是点一个选项就能完成的，最后的后果也并非以辩说的结果为必然。"),
                DialogueSystem.CreateSubtitle("精灵女王", "……"),
                DialogueSystem.CreateSubtitle("精灵女王", "我只能帮你们送到领土的边界，剩下的事情你们自己去办吧。你身后也是勇者，这都是神的意思，我无法阻止什么。")
            };
            
            // 播放最终对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(finalDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 将魔将玄武添加回背包
            AddXuanwuBackToInventory();
            
            // 继续对话
            List<DialogueData> returnItemDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(princessName, "谢过女王殿下。"),
                DialogueSystem.CreateSubtitle(princessName, "……"),
                DialogueSystem.CreateSubtitle(princessName, "我等告退了~"),
                DialogueSystem.CreateSubtitle("精灵女王", "慢着，虽然算不上什么，这个你们拿着，会有用的。")
            };
            
            // 播放对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(returnItemDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 添加精灵女王的宝石手镯到背包
            AddQueenBraceletToInventory();
            
            // 最后的对话
            List<DialogueData> endingDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(princessName, "再次谢过女王。"),
                DialogueSystem.CreateSubtitle("精灵女王", "你们走吧，我安排人直接把你们送到边境。"),
                DialogueSystem.CreateSubtitle("精灵女王", "祝你们一切顺利……")
            };
            
            // 播放结尾对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(endingDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 播放最终动画
            yield return StartCoroutine(PlayAnimationById("Stage5-4-3"));
            
            // 完成整个对话序列
            OnElfQueenAudienceComplete();
        }
        
        /// <summary>
        /// 进入王座大殿动画
        /// </summary>
        private IEnumerator PlayEnterThroneRoomAnimation()
        {
            // 这里可以调用具体的动画，目前使用通用方法
            // 根据需要可以替换为具体的动画名称
            yield return StartCoroutine(PlayAnimationById("EnterThroneRoom"));
        }
        
        /// <summary>
        /// 抬走契卡动画
        /// </summary>
        private IEnumerator PlayCarryAwayChikaAnimation()
        {
            // 这里可以调用具体的动画
            yield return StartCoroutine(PlayAnimationById("CarryAwayChika"));
        }
        
        /// <summary>
        /// 侍卫退场动画
        /// </summary>
        private IEnumerator PlayGuardsExitAnimation()
        {
            // 这里可以调用具体的动画
            yield return StartCoroutine(PlayAnimationById("GuardsExit"));
        }
        
        /// <summary>
        /// 从背包中移除魔将玄武
        /// </summary>
        public void RemoveXuanwuFromInventory()
        {
            if (InventorySystem.Instance != null)
            {
                // 查找魔将玄武在背包中的位置并移除
                for (int i = 0; i < InventorySystem.Instance.maxSlots; i++)
                {
                    Item item = InventorySystem.Instance.GetItem(i);
                    if (!item.IsEmpty && item.id == 6001) // 魔将玄武的ID
                    {
                        InventorySystem.Instance.RemoveItem(i);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 从背包中移除魔将玄武
        /// </summary>
        public void RemoveLaLaiYeFromInventory()
        {
            if (InventorySystem.Instance != null)
            {
                // 查找魔将玄武在背包中的位置并移除
                for (int i = 0; i < InventorySystem.Instance.maxSlots; i++)
                {
                    Item item = InventorySystem.Instance.GetItem(i);
                    if (!item.IsEmpty && item.id == 8001) // 魔将玄武的ID
                    {
                        InventorySystem.Instance.RemoveItem(i);
                        return;
                    }
                }
            }
        }
        
        /// <summary>
        /// 选项A回调：介绍复兴魔族的大计
        /// </summary>
        private void OnAudienceOptionA_Selected()
        {
            // 选项结果都相同，不需要额外处理
        }
        
        /// <summary>
        /// 选项B回调：介绍当前精人的局势
        /// </summary>
        private void OnAudienceOptionB_Selected()
        {
            // 选项结果都相同，不需要额外处理
        }
        
        /// <summary>
        /// 选项C回调：要求魔将玄武劝说
        /// </summary>
        private void OnAudienceOptionC_Selected()
        {
            // 选项结果都相同，不需要额外处理
        }
        
        /// <summary>
        /// 选项D回调：提示莉姆莉卡解释
        /// </summary>
        private void OnAudienceOptionD_Selected()
        {
            // 选项结果都相同，不需要额外处理
        }
        
        /// <summary>
        /// 将魔将玄武添加回背包
        /// </summary>
        private void AddXuanwuBackToInventory()
        {
            bool success = InventorySystem.AddItemToInventory(6001); // 魔将玄武的ID
        }
        
        /// <summary>
        /// 添加精灵女王的宝石手镯到背包
        /// </summary>
        private void AddQueenBraceletToInventory()
        {
            // 创建精灵女王的宝石手镯
            // 使用一个临时ID，因为物品表中没有这个物品
            bool success = InventorySystem.AddItemToInventory(9001, "精灵女王的宝石手镯", "有着充沛的奇异的生命魔力");
        }
        
        /// <summary>
        /// 精灵女王大殿觐见对话完成回调
        /// </summary>
        private void OnElfQueenAudienceComplete()
        {
            // 恢复玩家移动
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(true);
            }
        }

        private IEnumerator PlayStage5_5_1OpeningDialogue()
        {
            // 暂停玩家移动
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(false);
            }
            
            // 获取Stage5Controller来管理名称
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string playerName = stage5Controller != null ? stage5Controller.GetNPCName("我") : "无名勇者";
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            // 创建对话序列
            List<DialogueData> openingDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(princessName, "总觉得……王都的天空变色了……真的变得灰蒙蒙的了。"),
                DialogueSystem.CreateSubtitle(playerName, "走吧，马上就要结束了。"),
                DialogueSystem.CreateSubtitle("魔将玄武", "总觉得……有什么东西要来了……")
            };
            
            // 播放对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(openingDialogue, OnStage5_5_1DialogueComplete);
            }
            else
            {
                OnStage5_5_1DialogueComplete();
            }
            
            yield return null;
        }
        
        /// <summary>
        /// Stage5场景5-1对话完成回调
        /// </summary>
        private void OnStage5_5_1DialogueComplete()
        {
            // 恢复玩家移动
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(true);
            }
        }
        
        /// <summary>
        /// Stage5-5对话序列实现
        /// 包含关于阿撒托斯笛声的神秘对话
        /// </summary>
        private IEnumerator PlayStage5_5Dialogue()
        {
            // 暂停玩家移动
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(false);
            }
            
            // 获取Stage5Controller来管理名称
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string playerName = stage5Controller != null ? stage5Controller.GetNPCName("我") : "无名勇者";
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            // 创建对话序列
            List<DialogueData> stage5_5Dialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(princessName, "总觉得，一到这里就灰蒙蒙的……"),
                DialogueSystem.CreateSubtitle(playerName, "你能感觉到，我们前进的方向有什么奇怪的味道吗？"),
                DialogueSystem.CreateSubtitle(princessName, "嗯？没有啊？什么奇怪的味道？"),
                DialogueSystem.CreateSubtitle(playerName, "闻起来很像阿撒托斯的笛声。"),
                DialogueSystem.CreateSubtitle(princessName, "那是什么？"),
                DialogueSystem.CreateSubtitle(playerName, "我们继续前进吧。")
            };
            
            // 播放对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(stage5_5Dialogue, OnStage5_5DialogueComplete);
            }
            else
            {
                OnStage5_5DialogueComplete();
            }
            
            yield return null;
        }
        
        /// <summary>
        /// Stage5-5对话完成回调
        /// </summary>
        private void OnStage5_5DialogueComplete()
        {
            // 恢复玩家移动
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(true);
            }
            
            // 这里可以添加其他完成后的逻辑，比如：
            // - 更新游戏状态
            // - 解锁新的交互选项
            // - 触发后续事件等
        }
        
        /// <summary>
        /// 与魔将玄武永别的对话序列实现
        /// 包含无名勇者的直觉和与莉姆莉卡的对话
        /// </summary>
        private IEnumerator PlayFarewellXuanwuDialogue()
        {
            // 暂停玩家移动
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(false);
            }
            
            // 获取Stage5Controller来管理名称
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string playerName = stage5Controller != null ? stage5Controller.GetNPCName("我") : "无名勇者";
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            // 创建对话序列
            List<DialogueData> farewellDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(playerName, "嗯……这么看来……"),
                DialogueSystem.CreateSubtitle(playerName, "魔将玄武和我们永别了。"),
                DialogueSystem.CreateSubtitle(princessName, "诶？你这是什么意思？"),
                DialogueSystem.CreateSubtitle(playerName, "一点直觉罢了，我们快点结束这一切吧。")
            };
            
            // 播放对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(farewellDialogue, OnFarewellXuanwuDialogueComplete);
            }
            else
            {
                OnFarewellXuanwuDialogueComplete();
            }
            
            yield return null;
        }
        
        /// <summary>
        /// 与魔将玄武永别对话完成回调
        /// </summary>
        private void OnFarewellXuanwuDialogueComplete()
        {
            // 恢复玩家移动
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(true);
            }
            
            // 这里可以添加其他完成后的逻辑，比如：
            // - 更新游戏状态
            // - 解锁新的交互选项
            // - 触发后续事件等
        }
        
        /// <summary>
        /// 王都怪异感受对话序列实现
        /// 包含莉姆莉卡的疑问和玩家的四个选择
        /// </summary>
        private IEnumerator PlayCapitalStrangenessDialogue()
        {
            // 暂停玩家移动
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(false);
            }
            
            // 获取Stage5Controller来管理名称
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            // 创建主对话
            List<DialogueData> mainDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(princessName, "你不觉得这里哪里怪怪的吗？")
            };
            
            // 创建选项
            List<DialogueOption> capitalOptions = new List<DialogueOption>
            {
                DialogueSystem.CreateOption("大概只是这里的正常与别处不同而已。", () => OnCapitalOptionA_Selected()),
                DialogueSystem.CreateOption("他们说话莫名其妙的。", () => OnCapitalOptionB_Selected()),
                DialogueSystem.CreateOption("春天，十个海子全部复活！", () => OnCapitalOptionC_Selected()),
                DialogueSystem.CreateOption("前面应该还有更怪的。", () => OnCapitalOptionD_Selected())
            };
            
            // 创建带选项的对话
            DialogueData optionDialogue = DialogueSystem.CreateDialogueWithOptions(
                princessName,
                "你觉得怎么样？",
                capitalOptions
            );
            
            mainDialogue.Add(optionDialogue);
            
            // 开始对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(mainDialogue);
                // 等待对话完成由选项回调处理
            }
            else
            {
                // 如果对话系统不存在，直接恢复玩家移动
                if (playerController != null)
                {
                    playerController.SetCanMove(true);
                }
            }
            
            yield return null;
        }
        
        /// <summary>
        /// 选项A回调：大概只是这里的正常与别处不同而已。
        /// </summary>
        private void OnCapitalOptionA_Selected()
        {
            StartCoroutine(ShowCapitalOptionAFollowUp());
        }
        
        /// <summary>
        /// 选项B回调：他们说话莫名其妙的。
        /// </summary>
        private void OnCapitalOptionB_Selected()
        {
            StartCoroutine(ShowCapitalOptionBFollowUp());
        }
        
        /// <summary>
        /// 选项C回调：春天，十个海子全部复活！
        /// </summary>
        private void OnCapitalOptionC_Selected()
        {
            StartCoroutine(ShowCapitalOptionCFollowUp());
        }
        
        /// <summary>
        /// 选项D回调：前面应该还有更怪的。
        /// </summary>
        private void OnCapitalOptionD_Selected()
        {
            StartCoroutine(ShowCapitalOptionDFollowUp());
        }
        
        /// <summary>
        /// 选项A的后续对话
        /// </summary>
        private IEnumerator ShowCapitalOptionAFollowUp()
        {
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            
            List<DialogueData> followUpDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(princessName, "你说话还是挺幽默的。"),
                DialogueSystem.CreateSubtitle(princessName, "谢谢你，我不紧张了。")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(followUpDialogue, OnCapitalStrangenessDialogueComplete);
            }
            else
            {
                OnCapitalStrangenessDialogueComplete();
            }
            
            yield return null;
        }
        
        /// <summary>
        /// 选项B的后续对话
        /// </summary>
        private IEnumerator ShowCapitalOptionBFollowUp()
        {
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            string playerName = stage5Controller != null ? stage5Controller.GetNPCName("我") : "无名勇者";
            
            List<DialogueData> followUpDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(princessName, "对啊对啊，感觉王都都是精神有问题的家伙。"),
                DialogueSystem.CreateSubtitle(playerName, "人人都有病，所谓正常只是病的普遍的那种形式。"),
                DialogueSystem.CreateSubtitle(princessName, "总觉得有问题，但不知道怎么反驳……")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(followUpDialogue, OnCapitalStrangenessDialogueComplete);
            }
            else
            {
                OnCapitalStrangenessDialogueComplete();
            }
            
            yield return null;
        }
        
        /// <summary>
        /// 选项C的后续对话
        /// </summary>
        private IEnumerator ShowCapitalOptionCFollowUp()
        {
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            string playerName = stage5Controller != null ? stage5Controller.GetNPCName("我") : "无名勇者";
            
            List<DialogueData> followUpDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(princessName, "诶……别开玩笑了……这真的很吓人的。"),
                DialogueSystem.CreateSubtitle(princessName, "喂。"),
                DialogueSystem.CreateSubtitle(playerName, "没有，只是情绪使然念一句诗而已。"),
                DialogueSystem.CreateSubtitle(princessName, "呃……这真的是诗吗？")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(followUpDialogue, OnCapitalStrangenessDialogueComplete);
            }
            else
            {
                OnCapitalStrangenessDialogueComplete();
            }
            
            yield return null;
        }
        
        /// <summary>
        /// 选项D的后续对话
        /// </summary>
        private IEnumerator ShowCapitalOptionDFollowUp()
        {
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            string princessName = stage5Controller != null ? stage5Controller.GetNPCName("魔族公主") : "莉姆莉卡";
            string playerName = stage5Controller != null ? stage5Controller.GetNPCName("我") : "无名勇者";
            
            List<DialogueData> followUpDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(princessName, "你又在冒充伟大的先知了。"),
                DialogueSystem.CreateSubtitle(playerName, "你从哪学的这句话？"),
                DialogueSystem.CreateSubtitle(princessName, "不知道，随口一说。")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(followUpDialogue, OnCapitalStrangenessDialogueComplete);
            }
            else
            {
                OnCapitalStrangenessDialogueComplete();
            }
            
            yield return null;
        }
        
        /// <summary>
        /// 王都怪异感受对话完成回调
        /// </summary>
        private void OnCapitalStrangenessDialogueComplete()
        {
            // 恢复玩家移动
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(true);
            }
            
            // 这里可以添加其他完成后的逻辑，比如：
            // - 更新游戏状态
            // - 解锁新的交互选项
            // - 触发后续事件等
        }
    }
} 