using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
    public class vAICheckDamage : vStateDecision
    {
        public bool lookToDamageSender;

        public override string defaultName
        {
            get
            {
                return "Check Damage";
            }
        }

        public List<string> damageTypeToCheck;


        public override bool Decide(vIFSMBehaviourController fsmBehaviour)
        {
            return (HasDamage(fsmBehaviour));
        }

        protected virtual bool HasDamage(vIFSMBehaviourController fsmBehaviour)
        {
            var hasDamage = fsmBehaviour != null && (fsmBehaviour.aiController.receivedDamage != null && fsmBehaviour.aiController.receivedDamage.sender != null) && (damageTypeToCheck.Count == 0 || damageTypeToCheck.Contains(fsmBehaviour.aiController.receivedDamage.damageType));

            if (hasDamage && lookToDamageSender)
            {                
                //Debug.DrawLine(fsmBehaviour.aiController.transform.position, fsmBehaviour.aiController.receivedDamage.sender.position, Color.cyan, 10f);
                fsmBehaviour.aiController.LookToTarget(fsmBehaviour.aiController.receivedDamage.sender, 5f);                
            }
            if (fsmBehaviour.debugMode)
            {
                fsmBehaviour.SendDebug(Name + " " + (fsmBehaviour.aiController.receivedDamage != null && fsmBehaviour.aiController.receivedDamage.sender != null),this);
            }
            var result = fsmBehaviour != null && (fsmBehaviour.aiController.receivedDamage != null) && (damageTypeToCheck.Count == 0 || damageTypeToCheck.Contains(fsmBehaviour.aiController.receivedDamage.damageType));
            
            return result;
        }
    }

}
