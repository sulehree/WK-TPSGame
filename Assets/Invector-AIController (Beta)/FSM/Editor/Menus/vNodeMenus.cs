using UnityEditor;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
    public static class vNodeMenus
    {
        [MenuItem("Invector/AI Controller/Open FSM Behaviour Window")]
        public static void InitNodeEditor()
        {
            vFSMNodeEditorWindow.InitEditorWindow();
        }
    }
}