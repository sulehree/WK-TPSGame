using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
    public class vAISetObstaclesLayer : vStateAction
    {
        public override string defaultName
        {
            get
            {
                return "Set Obstacles Layer";
            }
        }

        public vAISetObstaclesLayer()
        {
            executionType = vFSMComponentExecutionType.OnStateEnter;
        }

        public LayerMask newLayer;

        public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
        {
            if (executionType == vFSMComponentExecutionType.OnStateEnter) fsmBehaviour.aiController.SetObstaclesLayer(newLayer);
        }
    }
}