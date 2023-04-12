using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
    public partial class vFSMBehaviourController : vMonoBehaviour, vIFSMBehaviourController
    {
        public vAIMessageReceiver messageReceiver
        {
            get
            {
                if (_messageReceiver == null && !tryGetMessageReceiver) _messageReceiver = GetComponent<vAIMessageReceiver>();
                if (_messageReceiver == null && !tryGetMessageReceiver) tryGetMessageReceiver = true;

                return _messageReceiver;
            }
        }
        private vAIMessageReceiver _messageReceiver;
        private bool tryGetMessageReceiver;
    }
}