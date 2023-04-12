using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.vCharacterController.AI
{
    public class vAITester : MonoBehaviour
    {
        public vControlAI ai;
        public Transform target;
        
        public void MoveToTarget()
        {
            ai.MoveTo(target.position);
        }

        public void MoveToClickPoint(Vector3 point)
        {
            ai.MoveTo(point);
        }
        public void Stop()
        {
            ai.Stop();
        }      
    }
}