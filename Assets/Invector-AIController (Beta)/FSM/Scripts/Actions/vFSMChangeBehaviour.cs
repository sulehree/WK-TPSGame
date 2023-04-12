using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.vCharacterController.AI.FSMBehaviour
{
    public class vFSMChangeBehaviour : vStateAction
    {
        public override string defaultName
        {
            get
            {
                return "Change Behaviour";
            }
        }
        public vFSMBehaviour newBehaviour;
        public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
        {
           
            fsmBehaviour.ChangeBehaviour(newBehaviour);
            
        }
    }
}