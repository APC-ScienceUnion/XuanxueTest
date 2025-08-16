using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace XuanZhiShiLian
{
    /// <summary>
    /// 祭品台交互脚本 - 处理复杂的多分支对话和条件判断
    /// 专门用于处理国王复活的复杂交互流程
    /// </summary>
    public class SacrificeAltarInteractable : Interactable
    {
        [Header("祭品台交互设置")]
        public string altarName = "祭品之台";
        public int requiredItemId = 9001; // 精灵信物的物品ID
        public string requiredItemName = "精灵女王的宝石手镯"; // 精灵信物的名称
        
        [Header("角色说话者名称")]
        public string limlikaName = "莉姆莉卡";
        public string kingName = "人类国王";
        public string heroName = "无名勇者";
        public string duckCapHeroName = "鸭舌帽勇者";
        public string narratorName = "旁白";
        
        [Header("动画设置")]
        public Animator transitionAnimator;
        public string[] animationNames = { "dead0", "dead-1", "dead-2", "dead-3", "dead-4" };
        
        [Header("特殊物品检查")]
        public int[] threeItemsForOptionC = { 7001, 7002, 7003 }; // 三个特殊物品的ID（需要集齐才能使用如来神掌）
        public int toiletPlungerItemId = 1002; // 马桶搋子的物品ID
        
        [Header("场景切换")]
        public string nextSceneName = "Stage6";
        
        private Stage5Controller stage5Controller;
        private PlayerController nearbyPlayer;
        private bool hasUsedAltar = false; // 防止重复使用
        protected bool playerInRange = false; // 玩家是否在交互范围内
        
        protected override void Start()
        {
            base.Start();
            stage5Controller = FindObjectOfType<Stage5Controller>();
            
            if (transitionAnimator == null)
            {
                // 尝试从Stage5Controller获取动画器
                if (stage5Controller != null)
                {
                    transitionAnimator = stage5Controller.transitionAnimator;
                }
            }
        }
        
        public override void Interact()
        {
            if (!playerInRange || hasUsedAltar) return;
            
            
            // 暂停玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(false);
            }
            
            // 显示初始选择对话
            ShowInitialChoice();
        }
        
        /// <summary>
        /// 显示初始选择：是否放置精灵信物复活国王？
        /// </summary>
        private void ShowInitialChoice()
        {
            List<DialogueOption> options = new List<DialogueOption>
            {
                DialogueSystem.CreateOption("是", () => {
                    StartCoroutine(HandleYesChoice());
                }),
                DialogueSystem.CreateOption("否", () => {
                    StartCoroutine(HandleNoChoice());
                })
            };
            
            DialogueData choiceDialogue = DialogueSystem.CreateDialogueWithOptions(
                altarName,
                "是否放置精灵信物复活国王？",
                options
            );
            
            List<DialogueData> dialogueData = new List<DialogueData> { choiceDialogue };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(dialogueData);
            }
        }
        
        /// <summary>
        /// 处理选择"否"的情况
        /// </summary>
        private IEnumerator HandleNoChoice()
        {
            yield return null; // 等待对话系统处理完选项
            
            List<DialogueData> noDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(limlikaName, "再探索一会吧。")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(noDialogue, OnInteractionComplete);
            }
            else
            {
                OnInteractionComplete();
            }
        }
        
        /// <summary>
        /// 处理选择"是"的情况
        /// </summary>
        private IEnumerator HandleYesChoice()
        {
            yield return null; // 等待对话系统处理完选项
            
            // 检查是否有精灵信物
            if (!HasRequiredItem())
            {
                List<DialogueData> noItemDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateSubtitle(limlikaName, $"你没有{requiredItemName}，无法复活国王。")
                };
                
                if (DialogueSystem.Instance != null)
                {
                    DialogueSystem.Instance.StartDialogue(noItemDialogue, OnInteractionComplete);
                }
                else
                {
                    OnInteractionComplete();
                }
                yield break;
            }
            
            // 标记已使用祭坛
            hasUsedAltar = true;
            
            // 消耗精灵信物
            ConsumeRequiredItem();
            
            // 播放动画并显示国王出现
            yield return StartCoroutine(PlayAnimationAndShowKing());
        }
        
        /// <summary>
        /// 播放动画并显示国王出现的对话
        /// </summary>
        private IEnumerator PlayAnimationAndShowKing()
        {
                         // 播放动画 dead0
             if (transitionAnimator != null)
             {
                 PlayAnimation("dead0");
                 yield return StartCoroutine(WaitForAnimationComplete("dead0"));
             }
            
            // 国王出现后的对话
            string[] kingAppearDialogue = {
                $"{kingName}：我等你们很久。",
                $"{kingName}：快！快！快把伟大的故事献上来吧！",
                $"{kingName}：梵天如果没有故事就会自梦中醒来！",
                $"{limlikaName}：哈？你在说什么东西？",
                $"{kingName}：我活着创造美妙的故事，死后将故事献给梵天。",
                $"{kingName}：这是我一念持续至今的秘诀！这是我永生的必然！",
                $"{kingName}：快！快！快！",
                $"{kingName}：快告诉我，那边的魔族公主其实是一切阴谋的主使！",
                $"{limlikaName}：诶？什么啊？喂……哥哥",
                $"{limlikaName}：你该不会要信这个疯子的话吧？",
                $"{heroName}：不急，跟他耍耍。",
                $"{kingName}：啊？什么？神明大人要精彩的故事！我明明设计的是一个阴谋的大反转剧本！",
                $"{kingName}：难道有什么比一直信任和陪在身侧的女主角才是幕后黑手的故事更有戏剧性效果的结局吗？",
                $"{heroName}：其实这个套路也已经被玩烂了……",
                $"{heroName}：再说了，从逻辑来说这也不太可能，为了节目效果忽视剧情逻辑不就成烂故事了？",
                $"{kingName}：明明整个现实都为了我献给神明的故事而改变了，为什么？为什么结局不是我想的那样？那至少，至少这个时候要有激烈冲突与大战！要有一个英雄诞生！",
                $"{kingName}：你们！你们要成为英雄！然后你们在冒险的过程中从处处设防逐步发展到彼此深深相爱，此时最后决战那个魔族公主要牺牲掉，让故事刻骨铭心！啊，真是美妙的故事！",
                $"{limlikaName}：一直一个人在那里自言自语自说自话，我完全不必要死啊！",
                $"{limlikaName}：而且，而且没有这种烂俗爱情故事就没有什么东西可写的了吗？",
                $"{limlikaName}：真要创新就不能脱离这种玩烂的勇者斗恶龙叙事让公主救勇者吗？",
                $"{heroName}：梵天不会因为你这点故事有无而苏醒，决定这个的是最终的裁决何时到来……",
                $"{heroName}：这当然是个爱情故事，但我的爱，就是要建立在杀死这个恶心的神明之上！",
                $"{limlikaName}：诶？",
                $"{kingName}：不可能的！哪怕你是勇者！无非也是按照神的旨意，出演他想看到的故事！",
                $"{kingName}：你的一切力量就来自于神明！那你能够做什么呢？",
                $"{kingName}：诶，就这样出演一出荒诞喜剧也不错啊！你真是一个演喜剧的天才！"
            };
            
            List<DialogueData> dialogueData = BuildDialogueData(kingAppearDialogue);
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(dialogueData, ShowFirstChoiceOptions);
            }
            else
            {
                ShowFirstChoiceOptions();
            }
        }
        
        /// <summary>
        /// 显示第一组选择选项
        /// </summary>
        private void ShowFirstChoiceOptions()
        {
            List<DialogueOption> options = new List<DialogueOption>
            {
                DialogueSystem.CreateOption("欲使其灭亡，必先使其疯狂。", () => {
                    StartCoroutine(HandleChoiceA());
                }),
                DialogueSystem.CreateOption("反派死于话多，我才是主角！", () => {
                    StartCoroutine(HandleChoiceB());
                })
            };
            
            // 检查是否集齐三个特殊物品
            if (HasAllThreeItems())
            {
                options.Add(DialogueSystem.CreateOption("我有道理，但你不配听！如来神掌！", () => {
                    StartCoroutine(HandleChoiceC());
                }));
            }
            
            // 检查是否有马桶搋子
            if (HasToiletPlunger())
            {
                options.Add(DialogueSystem.CreateOption("使用马桶搋子堵住国王的嘴", () => {
                    StartCoroutine(HandleChoiceD());
                }));
            }
            
            DialogueData choiceDialogue = DialogueSystem.CreateDialogueWithOptions(
                heroName,
                "",
                options
            );
            
            List<DialogueData> dialogueData = new List<DialogueData> { choiceDialogue };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(dialogueData);
            }
        }
        
        /// <summary>
        /// 处理选择A：欲使其灭亡，必先使其疯狂
        /// </summary>
        private IEnumerator HandleChoiceA()
        {
            yield return null;
            
            string[] choiceADialogue = {
                $"{kingName}：哈哈哈！世人笑我太疯癫，我笑世人看不穿！",
                $"{kingName}：梵天赐我神谕，就是为了让我如他所愿设计精彩的故事。",
                $"{kingName}：如同过去几万年他设计的无限的分裂厮杀一样！",
                $"{kingName}：过去如此，现在如此，未来也如此！",
                $"{kingName}：梵天与宇宙同寿，我亦与宇宙同寿！",
                $"{heroName}：你过去不在，未来也不会不在，你只能在现在在。",
                $"{heroName}：梵天曾经一切神谕只是为了有趣的故事，宇宙生灵无论生死也只是其取乐工具。",
                $"{heroName}：如今你为什么会有幻想觉得自己那么特殊，可以有神谕让你与神同寿，让你替他决定什么才是有趣的呢？",
                $"{heroName}：上天不仁，以万物为刍狗。你也没有任何特殊的，反而他让你这么疯狂也是他取乐的一部分。而且一般来说，就是要这种最高潮的时候，你最狂妄的时候让你崩溃，这才是他一贯的作风。",
                $"{kingName}：哈哈，你是说梵天赐我神谕与生死之超脱，只是为了最后毁灭我来取乐？",
                $"{kingName}：那我还怎么为他设计精彩的故事？难道我被诓骗直到最后才醒悟原来我不是剧情的设计者而从一开始也只是结局牺牲的一部分才是最有趣的剧情……吗？",
                $"{kingName}：诶？",
                $"{heroName}：当你意识到这点的时候，你就已经彻底没有机会了哦。",
                $"{kingName}：不，不是……"
            };
            
            List<DialogueData> dialogueData = BuildDialogueData(choiceADialogue);
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(dialogueData, () => {
                    StartCoroutine(PlayDeathAnimationA());
                });
            }
            else
            {
                StartCoroutine(PlayDeathAnimationA());
            }
        }
        
        /// <summary>
        /// 播放选择A的死亡动画
        /// </summary>
        private IEnumerator PlayDeathAnimationA()
        {
                         // 播放动画 dead-1
             if (transitionAnimator != null)
             {
                 PlayAnimation("dead-1");
                 yield return StartCoroutine(WaitForAnimationComplete("dead-1"));
             }
            
            string[] endingADialogue = {
                $"{narratorName}：话音未落，黑暗笼罩了人类国王，霎时间原地空余枯骨。人类国王已被大黑天吞噬，神魂俱灭，你的任务圆满完成了。"
            };
            
            List<DialogueData> dialogueData = BuildDialogueData(endingADialogue);
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(dialogueData, () => {
                    StartCoroutine(DelayAndShowFinalChoice());
                });
            }
            else
            {
                StartCoroutine(DelayAndShowFinalChoice());
            }
        }
        
        /// <summary>
        /// 处理选择B：反派死于话多，我才是主角！
        /// </summary>
        private IEnumerator HandleChoiceB()
        {
            yield return null;
            
            string[] choiceBDialogue = {
                $"{kingName}：你是勇者，我这里也有勇者！！！",
                $"{kingName}：回应我的召唤吧！鸭舌帽勇者！！！"
            };
            
            List<DialogueData> dialogueData = BuildDialogueData(choiceBDialogue);
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(dialogueData, () => {
                    StartCoroutine(PlaySummonAnimationB());
                });
            }
            else
            {
                StartCoroutine(PlaySummonAnimationB());
            }
        }
        
        /// <summary>
        /// 播放选择B的召唤动画
        /// </summary>
        private IEnumerator PlaySummonAnimationB()
        {
                         // 播放动画 dead-2
             if (transitionAnimator != null)
             {
                 PlayAnimation("dead-2");
                 yield return StartCoroutine(WaitForAnimationComplete("dead-2"));
             }
            
            string[] summonDialogue = {
                $"{duckCapHeroName}：啊？你怎么还活着？",
                $"{kingName}：先别管这些有的没的，我命令你应战那位无名勇者！打败他！",
                $"{kingName}：让他知道谁才是这个世界历史的决定者！",
                $"{duckCapHeroName}：勇者的使命是神明决定的，神明同意才会应允召唤。",
                $"{duckCapHeroName}：神明来的时候和我说了一句话：”老而不死是为贼。“",
                $"{duckCapHeroName}：世界的勇者任务是一致的，那位仁兄的任务看来是消灭你，那我的任务也是消灭你。",
                $"{kingName}：啊……不是……不是这样的！梵天给了我任务，祂不会让我死的！",
                $"{kingName}：我……我才是召唤你……"
            };
            
            List<DialogueData> dialogueData = BuildDialogueData(summonDialogue);
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(dialogueData, () => {
                    StartCoroutine(PlayDeathAnimationB());
                });
            }
            else
            {
                StartCoroutine(PlayDeathAnimationB());
            }
        }
        
        /// <summary>
        /// 播放选择B的死亡动画
        /// </summary>
        private IEnumerator PlayDeathAnimationB()
        {
                         // 播放动画 dead-3
             if (transitionAnimator != null)
             {
                 PlayAnimation("dead-3");
                 yield return StartCoroutine(WaitForAnimationComplete("dead-3"));
             }
            
            string[] afterDeathDialogue = {
                $"{heroName}：不是说你那把剑只斩魔族吗？",
                $"{duckCapHeroName}：谁说了？神明想斩谁圣剑就斩谁，只是当年要斩的是魔族而已。",
                $"{heroName}：听到了吗？不会把你怎样的，不要躲在柱子后了。",
                $"{limlikaName}：真……真的吗？",
                $"{duckCapHeroName}：那也不必担心，我任务完成了，我现在就走。",
                $"{duckCapHeroName}：实在怕等我走了再出来就好。"
            };
            
            List<DialogueData> dialogueData = BuildDialogueData(afterDeathDialogue);
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(dialogueData, () => {
                    StartCoroutine(PlayDuckCapHeroLeave());
                });
            }
            else
            {
                StartCoroutine(PlayDuckCapHeroLeave());
            }
        }
        
        /// <summary>
        /// 播放鸭舌帽勇者离开动画
        /// </summary>
        private IEnumerator PlayDuckCapHeroLeave()
        {
            // 播放动画 dead-4
            if (transitionAnimator != null)
            {
                PlayAnimation("dead-4");
                yield return StartCoroutine(WaitForAnimationComplete("dead-4"));
            }
            
            string[] leaveDialogue = {
                $"{heroName}：真帅啊这哥们。",
                $"{limlikaName}：那你可别为了耍帅杀了我。",
                $"{narratorName}：人类国王已死，你的任务圆满完成了。"
            };
            
            List<DialogueData> dialogueData = BuildDialogueData(leaveDialogue);
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(dialogueData, () => {
                    StartCoroutine(DelayAndShowFinalChoice());
                });
            }
            else
            {
                StartCoroutine(DelayAndShowFinalChoice());
            }
        }
        
        /// <summary>
        /// 处理选择C：如来神掌
        /// </summary>
        private IEnumerator HandleChoiceC()
        {
            yield return null;
            // 第一步：只显示“这难道是……”
            string[] firstLine = {
                $"{kingName}：这难道是……"
            };

            List<DialogueData> firstDialogue = BuildDialogueData(firstLine);

            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(firstDialogue, () => {
                    StartCoroutine(PlayDead1ThenShowRestC());
                });
            }
            else
            {
                StartCoroutine(PlayDead1ThenShowRestC());
            }
        }

        /// <summary>
        /// 播放 dead-1 动画后，继续播放剩余对白，并延续原有流程
        /// </summary>
        private IEnumerator PlayDead1ThenShowRestC()
        {
            // 插入动画 dead-1
            if (transitionAnimator != null)
            {
                PlayAnimation("dead-1");
                yield return StartCoroutine(WaitForAnimationComplete("dead-1"));
            }

            // 第二步：播放剩余对白
            string[] restDialogue = {
                $"{narratorName}：话音未落人类国王霎时被打为齑粉，大殿墙壁被打穿，漏出外面的星空",
                $"{narratorName}：人类国王已死，你的任务完成了",
                $"{heroName}：哇，这么厉害！",
                $"{limlikaName}：您的伟力盖世无双，小女子之前态度多有轻慢，望勇者大人原谅。",
                $"{heroName}：倒也不必如此……"
            };

            List<DialogueData> restData = BuildDialogueData(restDialogue);

            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(restData, () => {
                    StartCoroutine(DelayAndShowFinalChoice());
                });
            }
            else
            {
                StartCoroutine(DelayAndShowFinalChoice());
            }
        }
        
        /// <summary>
        /// 处理选择D：马桶搋子
        /// </summary>
        private IEnumerator HandleChoiceD()
        {
            yield return null;
            
            string[] choiceDDialogue = {
                $"{kingName}：呜呜，呜！！",
                $"{kingName}：咕咕嘎嘎！",
                $"{limlikaName}：哈哈哈！太好笑了！",
                $"{limlikaName}：啊哈哈哈！",
                $"{heroName}：这下你就没法给梵天讲故事了。",
                $"{heroName}：让他醒来一个给你看啊？",
                $"{kingName}：咕！乌鲁乌鲁！",
                $"{narratorName}：随着一阵震颤，人类国王迅速化为一阵枯骨，召唤法阵发出一阵白光",
                $"{duckCapHeroName}：鸭舌帽勇者来也！",
                $"{heroName}：该不会还有二阶段BOSS站吧？",
                $"{duckCapHeroName}：不是，神明托我给你带个话：”这确实很好笑。“",
                $"{duckCapHeroName}：我的任务完成了，走了。"
            };
            
            List<DialogueData> dialogueData = BuildDialogueData(choiceDDialogue);
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(dialogueData, () => {
                    StartCoroutine(PlayDuckCapHeroLeaveD());
                });
            }
            else
            {
                StartCoroutine(PlayDuckCapHeroLeaveD());
            }
        }
        
        /// <summary>
        /// 选择D中鸭舌帽勇者离开
        /// </summary>
        private IEnumerator PlayDuckCapHeroLeaveD()
        {
            // 播放动画 dead-4
            if (transitionAnimator != null)
            {
                PlayAnimation("dead-4");
                yield return StartCoroutine(WaitForAnimationComplete("dead-4"));
            }
            
            string[] leaveDDialogue = {
                $"{heroName}：真看戏啊，这神。",
                $"{narratorName}：人类国王已死，你的任务完成了"
            };
            
            List<DialogueData> dialogueData = BuildDialogueData(leaveDDialogue);
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(dialogueData, () => {
                    StartCoroutine(DelayAndShowFinalChoice());
                });
            }
            else
            {
                StartCoroutine(DelayAndShowFinalChoice());
            }
        }
        
        /// <summary>
        /// 延迟2秒后显示最终选择
        /// </summary>
        private IEnumerator DelayAndShowFinalChoice()
        {
            yield return new WaitForSeconds(2f);
            
            string[] finalPreDialogue = {
                $"{heroName}：无论如何，现在我来这里的任务完成了，剩下的事也不是我以现在的身份能完成的了。",
                $"{limlikaName}：诶，这么快就要走吗？",
                $"{heroName}：勇者本就不属于这个世界，完成任务就要回去了，这是既定的规则。",
                $"{heroName}：不过应该时间还够我向你坦白些什么："
            };
            
            List<DialogueData> dialogueData = BuildDialogueData(finalPreDialogue);
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(dialogueData, ShowFinalChoiceOptions);
            }
            else
            {
                ShowFinalChoiceOptions();
            }
        }
        
        /// <summary>
        /// 显示最终选择选项
        /// </summary>
        private void ShowFinalChoiceOptions()
        {
            List<DialogueOption> options = new List<DialogueOption>
            {
                DialogueSystem.CreateOption("我喜欢你，而且我爱你！", () => {
                    StartCoroutine(HandleFinalChoiceA());
                }),
                DialogueSystem.CreateOption("这一路来，真的非常感谢你。", () => {
                    StartCoroutine(HandleFinalChoiceB());
                }),
                DialogueSystem.CreateOption("这一切都是神明的过错，他设计的故事如此稀烂，不是你的过错。", () => {
                    StartCoroutine(HandleFinalChoiceC());
                }),
                DialogueSystem.CreateOption("我知道这一切都只是虚无缥缈的符号，你也同样的虚无。", () => {
                    StartCoroutine(HandleFinalChoiceD());
                })
            };
            
            DialogueData choiceDialogue = DialogueSystem.CreateDialogueWithOptions(
                heroName,
                "",
                options
            );
            
            List<DialogueData> dialogueData = new List<DialogueData> { choiceDialogue };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(dialogueData);
            }
        }
        
        /// <summary>
        /// 最终选择A：我喜欢你，而且我爱你！
        /// </summary>
        private IEnumerator HandleFinalChoiceA()
        {
            yield return null;
            
            string[] finalADialogue = {
                $"{limlikaName}：你以为我会痛哭流涕吗？公主要有公主的矜持！",
                $"{limlikaName}：那你说你喜欢我，那我也喜欢你！",
                $"{limlikaName}：你说你爱我，那我也爱你！",
                $"{limlikaName}：再见了！勇者大人！"
            };
            
            List<DialogueData> dialogueData = BuildDialogueData(finalADialogue);
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(dialogueData, () => {
                    TransitionToNextScene();
                });
            }
            else
            {
                TransitionToNextScene();
            }
        }
        
        /// <summary>
        /// 最终选择B：这一路来，真的非常感谢你
        /// </summary>
        private IEnumerator HandleFinalChoiceB()
        {
            yield return null;
            
            string[] finalBDialogue = {
                $"{limlikaName}：我也喜……诶，没什么",
                $"{limlikaName}：我说我也很感谢你的照顾，谢谢你拯救了我们。",
                $"{limlikaName}：你是我们魔族的伟大勇者……也是我心中永远的英雄！",
                $"{limlikaName}：再见了！奇怪的家伙！"
            };
            
            List<DialogueData> dialogueData = BuildDialogueData(finalBDialogue);
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(dialogueData, () => {
                    TransitionToNextScene();
                });
            }
            else
            {
                TransitionToNextScene();
            }
        }
        
        /// <summary>
        /// 最终选择C：这一切都是神明的过错
        /// </summary>
        private IEnumerator HandleFinalChoiceC()
        {
            yield return null;
            
            string[] finalCDialogue = {
                $"{limlikaName}：我们身为造物，和你不一样，你终究有着自由。",
                $"{limlikaName}：但即便如此，我还是愿意相信在这一切的破败以外。",
                $"{limlikaName}：有着创造最美好结局与满足一切希望的可能性。",
                $"{limlikaName}：我们一定还能再更美好的另一个可能中相遇的！",
                $"{limlikaName}：再见了！英雄！"
            };
            
            List<DialogueData> dialogueData = BuildDialogueData(finalCDialogue);
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(dialogueData, () => {
                    TransitionToNextScene();
                });
            }
            else
            {
                TransitionToNextScene();
            }
        }
        
        /// <summary>
        /// 最终选择D：我知道这一切都只是虚无缥缈的符号
        /// </summary>
        private IEnumerator HandleFinalChoiceD()
        {
            yield return null;
            
            string[] finalDDialogue = {
                $"{limlikaName}：我也知道……但是，我愿意相信另一个奇迹。",
                $"{limlikaName}：相信除此之外，我们还有真正相遇的可能。",
                $"{limlikaName}：无论我此刻的内在如何空虚，但只要坚信，总会有奇迹发生的！",
                $"{limlikaName}：只要有爱的话，整个宇宙都能创造出来的！我也将真正创造我自己！",
                $"{limlikaName}：所以，不要被一时的虚无放弃伟大的真爱了，我们终究还会相遇的！",
                $"{limlikaName}：再见了！大笨蛋！"
            };
            
            List<DialogueData> dialogueData = BuildDialogueData(finalDDialogue);
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(dialogueData, () => {
                    TransitionToNextScene();
                });
            }
            else
            {
                TransitionToNextScene();
            }
        }
        
        /// <summary>
        /// 切换到下一个场景
        /// </summary>
        private void TransitionToNextScene()
        {
            if (stage5Controller != null)
            {
                // 通知Stage5Controller任务完成
                stage5Controller.OnNPCInteractionComplete("祭品台");
            }
            
            // 切换场景
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
        
        /// <summary>
        /// 检查是否有必需的物品（精灵信物）
        /// </summary>
        private bool HasRequiredItem()
        {
            if (requiredItemId <= 0) return true;
            
            if (InventorySystem.Instance != null)
            {
                for (int i = 0; i < InventorySystem.Instance.maxSlots; i++)
                {
                    Item item = InventorySystem.Instance.GetItem(i);
                    if (!item.IsEmpty && item.id == requiredItemId)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 消耗必需的物品（精灵信物）
        /// </summary>
        private void ConsumeRequiredItem()
        {
            if (requiredItemId <= 0) return;
            
            if (InventorySystem.Instance != null)
            {
                for (int i = 0; i < InventorySystem.Instance.maxSlots; i++)
                {
                    Item item = InventorySystem.Instance.GetItem(i);
                    if (!item.IsEmpty && item.id == requiredItemId)
                    {
                        InventorySystem.Instance.RemoveItem(i);
                        break;
                    }
                }
            }
        }
        
        /// <summary>
        /// 检查是否集齐了三个特殊物品（用于如来神掌选项）
        /// </summary>
        private bool HasAllThreeItems()
        {
            if (threeItemsForOptionC == null || threeItemsForOptionC.Length != 3) return false;
            
            if (InventorySystem.Instance == null) return false;
            
            foreach (int itemId in threeItemsForOptionC)
            {
                bool hasItem = false;
                for (int i = 0; i < InventorySystem.Instance.maxSlots; i++)
                {
                    Item item = InventorySystem.Instance.GetItem(i);
                    if (!item.IsEmpty && item.id == itemId)
                    {
                        hasItem = true;
                        break;
                    }
                }
                
                if (!hasItem)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 检查是否有马桶搋子
        /// </summary>
        private bool HasToiletPlunger()
        {
            if (toiletPlungerItemId <= 0) return false;
            
            if (InventorySystem.Instance != null)
            {
                for (int i = 0; i < InventorySystem.Instance.maxSlots; i++)
                {
                    Item item = InventorySystem.Instance.GetItem(i);
                    if (!item.IsEmpty && item.id == toiletPlungerItemId)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 构建对话数据
        /// </summary>
        private List<DialogueData> BuildDialogueData(string[] lines)
        {
            List<DialogueData> dialogueData = new List<DialogueData>();
            
            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line.Trim()))
                {
                    string trimmedLine = line.Trim();
                    
                    // 解析 说话者：对话内容 格式
                    if (trimmedLine.Contains("："))
                    {
                        string[] parts = trimmedLine.Split('：');
                        if (parts.Length >= 2)
                        {
                            string speaker = parts[0].Trim();
                            string text = parts[1].Trim();
                            dialogueData.Add(DialogueSystem.CreateSubtitle(speaker, text));
                        }
                    }
                    else
                    {
                        // 没有说话者标记，使用祭坛名称作为默认说话者
                        dialogueData.Add(DialogueSystem.CreateSubtitle(altarName, trimmedLine));
                    }
                }
            }
            
            return dialogueData;
        }
        
        /// <summary>
        /// 检查动画名称是否有效
        /// </summary>
        private bool IsValidAnimationName(string animationName)
        {
            if (animationNames == null || animationNames.Length == 0)
            {
                return false;
            }
            
            foreach (string validName in animationNames)
            {
                if (validName == animationName)
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 播放指定动画（带验证）
        /// </summary>
        private void PlayAnimation(string animationName)
        {
            if (transitionAnimator == null)
            {
                return;
            }
            
            if (!IsValidAnimationName(animationName))
            {
                return;
            }
            transitionAnimator.Play(animationName);
        }
        
        /// <summary>
        /// 等待动画播放完成
        /// </summary>
        private IEnumerator WaitForAnimationComplete(string animationName)
        {
            if (transitionAnimator == null) yield break;
            
            // 验证动画名称
            if (!IsValidAnimationName(animationName))
            {
                yield break;
            }
            
            // 等待动画开始播放
            yield return new WaitForEndOfFrame();
            
            // 获取动画长度
            float animationTime = 3f; // 默认时长
            AnimatorStateInfo stateInfo = transitionAnimator.GetCurrentAnimatorStateInfo(0);
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
        
        /// <summary>
        /// 玩家进入触发范围（2D碰撞检测）
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
                nearbyPlayer = other.GetComponent<PlayerController>();
                OnPlayerEnter();

                // 关键：将本交互对象注册到玩家，供 PlayerController.Interact() 使用
                if (nearbyPlayer != null)
                {
                    nearbyPlayer.SetCurrentInteractable(this);
                }
            }
        }
        
        /// <summary>
        /// 玩家离开触发范围（2D碰撞检测）
        /// </summary>
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;

                // 关键：从玩家处清除当前交互对象
                var player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.SetCurrentInteractable(null);
                }

                nearbyPlayer = null;
                OnPlayerExit();
            }
        }
        
        /// <summary>
        /// 交互完成回调
        /// </summary>
        private void OnInteractionComplete()
        {
            // 恢复玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(true);
            }
        }
        
        /// <summary>
        /// 公共方法：重置祭坛状态（用于测试或重新开始）
        /// </summary>
        public void ResetAltarState()
        {
            hasUsedAltar = false;
            isInteractable = true;
        }
        
        /// <summary>
        /// 公共方法：获取配置的动画列表信息
        /// </summary>
        public void PrintAnimationInfo()
        {
            if (animationNames == null || animationNames.Length == 0)
            {
                return;
            }
        }
        
        /// <summary>
        /// 公共方法：验证所有配置的动画是否存在
        /// </summary>
        public void ValidateAllAnimations()
        {
            if (transitionAnimator == null)
            {
                return;
            }
            
            if (animationNames == null || animationNames.Length == 0)
            {
                return;
            }
            bool allValid = true;
            
            for (int i = 0; i < animationNames.Length; i++)
            {
                string animName = animationNames[i];
                if (string.IsNullOrEmpty(animName))
                {
                    allValid = false;
                    continue;
                }
            }
            
        }
    }
}