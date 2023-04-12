using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI
{
    [vClassHeader("AI MESSAGE RECEIVER", "Use this component with the SendMessage Action to call a Event directly from a State of the FSM Behaviour.")]
    public class vAIMessageReceiver : vMonoBehaviour
    {
        public OnReceiveMessageEvent defaultListener;
        public List<AIMessageListener> messagesListeners;

        [System.Serializable]
        public class OnReceiveMessageEvent : UnityEngine.Events.UnityEvent<string> { }

        [System.Serializable]
        public class AIMessageListener
        {
            public string Name;
            public OnReceiveMessageEvent onReceiveMessage;
            public AIMessageListener(string name)
            {
                this.Name = name;
            }
            public AIMessageListener(string name, UnityEngine.Events.UnityAction<string> listener)
            {
                this.Name = name;
                this.onReceiveMessage.AddListener(listener);
            }
        }

        public void AddListener(string name, UnityEngine.Events.UnityAction<string> listener)
        {
            if (messagesListeners.Exists(l => l.Name.Equals(name)))
            {
                var messageListener = messagesListeners.Find(l => l.Name.Equals(name));
                messageListener.onReceiveMessage.AddListener(listener);
            }
            else
            {
                messagesListeners.Add(new AIMessageListener(name, listener));
            }
        }

        public void RemoveListener(string name, UnityEngine.Events.UnityAction<string> listener)
        {
            if (messagesListeners.Exists(l => l.Name.Equals(name)))
            {
                var messageListener = messagesListeners.Find(l => l.Name.Equals(name));
                messageListener.onReceiveMessage.RemoveListener(listener);
            }
        }

        public void Send(string name, string message)
        {
            if (messagesListeners.Exists(l => l.Name.Equals(name)))
            {
                var messageListener = messagesListeners.Find(l => l.Name.Equals(name));
                messageListener.onReceiveMessage.Invoke(message);                
            }
            else defaultListener.Invoke(message);
        }
    }
}