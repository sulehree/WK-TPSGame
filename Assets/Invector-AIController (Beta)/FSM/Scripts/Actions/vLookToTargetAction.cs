using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
    public class vLookToTargetAction : vStateAction
    {
        public override string defaultName
        {
            get
            {
                return "Look To Target";
            }
        }

        public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
        {
            if(fsmBehaviour!=null && fsmBehaviour.aiController.currentTarget.transform )
            {
                fsmBehaviour.aiController.LookTo(fsmBehaviour.aiController.lastTargetPosition+Vector3.up*1.5f);
            }
        }       
    }

}
