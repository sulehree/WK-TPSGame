using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
    public class AICheckTargetDead : vStateDecision
    {
        public override string defaultName
        {
            get
            {
                return "Check Target Dead";
            }
        }

        [Header("For targets without HealthController")]
        public bool simpleTarget;
        public override bool Decide(vIFSMBehaviourController fsmBehaviour)
        {
            return TargetIsDead(fsmBehaviour);
        }

        protected virtual bool TargetIsDead(vIFSMBehaviourController fsmBehaviour)
        {
            if (fsmBehaviour == null) return true;
            Transform target = fsmBehaviour.aiController.currentTarget;
            if (target == null) return true;
            if (!fsmBehaviour.aiController.currentTarget.transform.gameObject.activeSelf) return true;
            if (!simpleTarget && fsmBehaviour.aiController.currentTarget.hasHealthController) return fsmBehaviour.aiController.currentTarget.isDead;
            return false;
        }
    }
}