using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace Invector.vCharacterController.AI
{
    public partial class vAIMenuComponent
    {
#if INVECTOR_MELEE
        [MenuItem("Invector/AI Controller/Components/AI MeleeManager")]
        static void AIMeleeManagerMenu()
        {
            if (Selection.activeGameObject && Selection.activeGameObject.GetComponent<vControlAI>() != null)
                Selection.activeGameObject.AddComponent<vMelee.vMeleeManager>();
            else
                Debug.Log("Please select a AI Controller to add the component.");
        }
#endif
#if INVECTOR_SHOOTER
        [MenuItem("Invector/AI Controller/Components/AI ShooterManager")]
        static void AIShooterManagerMenu()
        {
            if (Selection.activeGameObject && Selection.activeGameObject.GetComponent<vControlAI>() != null)
                Selection.activeGameObject.AddComponent<vAIShooterManager>();
            else
                Debug.Log("Please select a AI Controller to add the component.");
        }
#endif
        [MenuItem("Invector/AI Controller/Components/AI Headtrack")]
        static void AIHeadtrackMenu()
        {
            if (Selection.activeGameObject && Selection.activeGameObject.GetComponent<vControlAI>() != null)
                Selection.activeGameObject.AddComponent<vAIHeadtrack>();
            else
                Debug.Log("Please select a AI Controller to add the component.");
        }

        [MenuItem("Invector/AI Controller/Components/AI MessageReceiver")]
        static void AIMessageReceiver()
        {
            if (Selection.activeGameObject && Selection.activeGameObject.GetComponent<vControlAI>() != null)
                Selection.activeGameObject.AddComponent<vAIMessageReceiver>();
            else
                Debug.Log("Please select a AI Controller to add the component.");
        }

        [MenuItem("Invector/AI Controller/Components/AI Spawn System")]
        static void AISpawnerMenu()
        {
            var wp = new GameObject("AI Spawn", typeof(vAISpawn));

            SceneView view = SceneView.lastActiveSceneView;
            if (SceneView.lastActiveSceneView == null)
                throw new UnityException("The Scene View can't be access");

            Vector3 spawnPos = view.camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
            wp.transform.position = spawnPos;
        }

        [MenuItem("Invector/AI Controller/Components/AI Simple Holder")]
        static void AISimpleHolderMenu()
        {
            if (Selection.activeGameObject)
                Selection.activeGameObject.AddComponent<vSimpleHolder>();
            else
                Debug.Log("Please select a GameObject to add this component.");
        }

    }
}

#endif