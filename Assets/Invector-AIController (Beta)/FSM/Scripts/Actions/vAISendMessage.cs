using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.vCharacterController.AI.FSMBehaviour
{
#if UNITY_EDITOR
    [vFSMHelpbox("Make sure the AI have a vAIMessageRecever component", UnityEditor.MessageType.Info)]
#endif
    public class vAISendMessage : vStateAction
    {

        public override string defaultName
        {
            get
            {
               return  "SendMessage";
            }
        }

        public vAISendMessage()
        {
            executionType = vFSMComponentExecutionType.OnStateEnter;
        }
        public string listenerName;
        public string message;
        public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
        {
            if (fsmBehaviour.messageReceiver) fsmBehaviour.messageReceiver.Send(listenerName, message);
        }      
    }
}