using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.vCharacterController.AI
{
   
    [vClassHeader("SpawnPoint",helpBoxText ="Need Collider trigger and Rigidbody Kinematics to check if spawn point area is occuped",useHelpBox = true,openClose =false)]
    public class vSpawnPoint : vMonoBehaviour
    {      
        public bool isValid { get { return colliders.Count == 0; } }       
        public  List<Collider> colliders = new List<Collider>();
        void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Untagged") && !colliders.Contains(other)) colliders.Add(other);               
        }
        void OnTriggerExit(Collider other)
        {
            if (colliders.Contains(other)) colliders.Remove(other);
        }
    }
}
