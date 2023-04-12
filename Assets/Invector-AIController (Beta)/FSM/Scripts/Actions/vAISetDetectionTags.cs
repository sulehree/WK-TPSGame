using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
    public class vAISetDetectionTags : vStateAction
    {
        public override string defaultName
        {
            get
            {
                return "Set Detections Tags";
            }
        }

        public vAISetDetectionTags()
        {
            executionType = vFSMComponentExecutionType.OnStateEnter;
        }

        public vTagMask tags;

        public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
        {
            if (executionType == vFSMComponentExecutionType.OnStateEnter) fsmBehaviour.aiController.SetDetectionTags(tags);
        }
    }
}