using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
    public class AICheckTargetDistance : vStateDecision
    {
        public override string defaultName
        {
            get
            {
                return "Check Target Distance";
            }
        }

        public enum vCheckValue
        {
            Equals, Less, Greater, NoEqual
        }

        public vCheckValue checkDistance = vCheckValue.NoEqual;

        public float value = 2f;

        public override bool Decide(vIFSMBehaviourController fsmBehaviour)
        {
            return CheckValue(fsmBehaviour);
        }

        protected virtual bool CheckValue(vIFSMBehaviourController fsmBehaviour)
        {
            if (fsmBehaviour == null) return false;
            if (fsmBehaviour.aiController.currentTarget == null) return true;
            switch (checkDistance)
            {
                case vCheckValue.Equals:
                    return fsmBehaviour.aiController.targetDistance == value;
                case vCheckValue.Less:
                    return fsmBehaviour.aiController.targetDistance < value;
                case vCheckValue.Greater:
                    return fsmBehaviour.aiController.targetDistance > value;
                case vCheckValue.NoEqual:
                    return fsmBehaviour.aiController.targetDistance != value;
            }
            return false;
        }
    }
}
