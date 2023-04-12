
namespace Invector.vCharacterController.AI.FSMBehaviour
{
#if UNITY_EDITOR
    [vFSMHelpbox("Needs FindTarget Action to work", UnityEditor.MessageType.Info)]
#endif
    public class AICanSeeTargetDecision : vStateDecision
    {
        public override string defaultName
        {
            get
            {
                return "Can See Target";
            }
        }

        public override bool Decide(vIFSMBehaviourController fsmBehaviour)
        {
            var cansee = CanSeeTarget(fsmBehaviour);
            return cansee;
        }

        protected virtual bool CanSeeTarget(vIFSMBehaviourController fsmBehaviour)
        { 
            return fsmBehaviour.aiController.targetInLineOfSight;
        }
    }

}
