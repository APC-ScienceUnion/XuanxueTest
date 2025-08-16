using UnityEngine;

namespace XuanZhiShiLian
{
    public class SceneAreaInteractable : Interactable
    {
        public int sceneIndex;
        public Stage5Controller stage5Controller;
        
        public override void Interact()
        {
            if (stage5Controller != null)
            {
                stage5Controller.OnSceneAreaInteraction(sceneIndex);
            }
        }
    }
} 

