using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Invector.vCharacterController.AI
{
    [CustomEditor(typeof(vAITester),true)]
    public class vAITesterEditor : Editor
    {
        RaycastHit hit;
        private void OnSceneGUI()
        {
            Event e = Event.current;
            var tester = (target as vAITester);
            Vector2 guiPosition = Event.current.mousePosition;
          
            if (e.type == EventType.MouseDown && e.button ==1)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(guiPosition);
                  
                if(Physics.Raycast(ray,out hit ))
                {
                    if (hit.collider.gameObject == tester.gameObject)
                    { 
                        tester.ai.TakeDamage(new vDamage(1));
                    }
                       
                    else
                    (target as vAITester).MoveToClickPoint(hit.point);
                  
                    e.Use();
                }
            }                  
           
        }
    }

}
