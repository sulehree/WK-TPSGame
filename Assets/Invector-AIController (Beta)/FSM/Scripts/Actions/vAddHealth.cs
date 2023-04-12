using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
    public class vAddHealth : vStateAction
    {
        public bool recoverWhenIsDead = false;
        public override string defaultName
        {
            get
            {
                return "Add Health";
            }
        }

        [Header("This action won't work along side with DecisionTimer")]
        public float timeToAdd = 1f;
        public int healthToRecovery = 1;

        public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
        {
            if (fsmBehaviour.aiController.isDead && !recoverWhenIsDead) return;
            AddHealth(fsmBehaviour);
        }

        protected virtual void AddHealth(vIFSMBehaviourController fsmBehaviour)
        {
            if (fsmBehaviour == null) return;
            
            if (InTimer(fsmBehaviour, timeToAdd))
            {                
                fsmBehaviour.aiController.ChangeHealth(healthToRecovery);             
            }            
        }
    }
}