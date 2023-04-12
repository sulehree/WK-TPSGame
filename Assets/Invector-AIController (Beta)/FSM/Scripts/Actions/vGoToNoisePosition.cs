using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.vCharacterController.AI.FSMBehaviour
{
    public class vGoToNoisePosition : vStateAction
    {
        public bool findNewNoise = false;       
        [vHideInInspector("findNewNoise")]
        public bool specificType;
        [vHideInInspector("findNewNoise;specific")]
        public string noiseType;
        public bool lookToNoisePosition = true;
        public override string defaultName
        {
            get
            {
                return "Go To Noise Position";
            }
        }

        public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
        {
            if (fsmBehaviour.aiController != null)
            {
                if (fsmBehaviour.aiController.HasComponent<vAINoiseListener>())
                {
                    var noiseListener = fsmBehaviour.aiController.GetAIComponent<vAINoiseListener>();
                    vNoise noise = null;
                    if (findNewNoise)
                    {
                        if (specificType) noise = noiseListener.GetNoiseByType(noiseType);
                        else noise = noiseListener.GetNearNoise();
                    }
                    else noise = noiseListener.lastListenedNoise;
                    if(noise!=null)
                    {
                        fsmBehaviour.aiController.MoveTo(noise.position);
                        if (lookToNoisePosition) fsmBehaviour.aiController.LookTo(noise.position);
                    }
                }
            }
        }
    }
}
