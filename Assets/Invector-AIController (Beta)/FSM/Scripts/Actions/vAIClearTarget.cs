using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
    public class vAIClearTarget : vStateAction
    {
        public override string defaultName
        {
            get
            {
                return "Clear Target";
            }
        }

        public vAIClearTarget()
        {
            executionType = vFSMComponentExecutionType.OnStateEnter;
        }

        public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
        {
            if (executionType == vFSMComponentExecutionType.OnStateEnter) fsmBehaviour.aiController.RemoveCurrentTarget();
        }
    }
}