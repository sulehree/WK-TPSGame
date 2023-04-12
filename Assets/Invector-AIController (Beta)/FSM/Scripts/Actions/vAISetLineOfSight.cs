using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
#if UNITY_EDITOR
    [vFSMHelpbox("If value of property equals -1, the property no will be changed",UnityEditor.MessageType.Info)]
 #endif
    public class vAISetLineOfSight : vStateAction
    {
        public override string defaultName
        {
            get
            {
                return "Set Line Of Sight";
            }
        }

        public vAISetLineOfSight()
        {
            executionType = vFSMComponentExecutionType.OnStateEnter;
            fieldOfView = -1;
            minDistanceToDetect = -1;
            maxDistanceToDetect = -1f;
            lostTargetDistance = -1f;
        }

        public float fieldOfView;
        public float minDistanceToDetect;
        public float maxDistanceToDetect;
        public float lostTargetDistance;

        public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
        {
            if (executionType == vFSMComponentExecutionType.OnStateEnter) fsmBehaviour.aiController.SetLineOfSight(fieldOfView,minDistanceToDetect,maxDistanceToDetect,lostTargetDistance);
        }
    }
}