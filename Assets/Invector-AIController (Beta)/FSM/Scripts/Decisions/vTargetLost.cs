
namespace Invector.vCharacterController.AI.FSMBehaviour
{
    public class vTargetLost : vStateDecision
    {
        public override string defaultName
        {
            get
            {
                return "Was the target lost?";
            }
        }

        public override bool Decide(vIFSMBehaviourController fsmBehaviour)
        {
            if (fsmBehaviour != null && fsmBehaviour.aiController != null && fsmBehaviour.aiController.currentTarget.transform)
            {
                return fsmBehaviour.aiController.currentTarget.isLost;
            }
            return true;
        }
    }
}