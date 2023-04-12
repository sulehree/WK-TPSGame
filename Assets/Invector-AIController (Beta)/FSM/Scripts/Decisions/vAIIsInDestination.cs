using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.vCharacterController.AI.FSMBehaviour
{
    public class vAIIsInDestination : vStateDecision
    {
        public override string defaultName
        {
            get
            {
                return "Is In Destination";
            }
        }

        public override bool Decide(vIFSMBehaviourController fsmBehaviour)
        {
            return fsmBehaviour.aiController.isInDestination;
           
        }       
    }
}