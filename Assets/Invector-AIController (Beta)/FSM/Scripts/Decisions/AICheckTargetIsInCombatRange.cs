using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
    public class AICheckTargetIsInCombatRange : vStateDecision
    {
        public override string defaultName
        {
            get
            {
                return "Check Target Combat Range";
            }
        }

        public override bool Decide(vIFSMBehaviourController fsmBehaviour)
        {
            return TargetIsInCombatRange(fsmBehaviour.aiController as vIControlAICombat);
        }

        protected virtual bool TargetIsInCombatRange(vIControlAICombat ctrlCombat)
        {
            if (ctrlCombat == null) return false;
            if (ctrlCombat.currentTarget.transform == null) return false;
            if (!ctrlCombat.currentTarget.transform.gameObject.activeSelf) return false;
            return ctrlCombat.targetDistance <= ctrlCombat.combatRange;
        }
    }
}

