
namespace Invector.vCharacterController.AI.FSMBehaviour
{
    public class vLookAroundAction : vStateAction
    {
        public override string defaultName
        {
            get
            {
                return "Look Around";
            }
        }

        public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
        {
            fsmBehaviour.aiController.LookAround();
        }    
        
    }
}