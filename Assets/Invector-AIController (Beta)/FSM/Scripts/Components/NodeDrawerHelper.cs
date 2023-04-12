#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
    public static class NodeDrawerHelper
    {
        public static void InitNode(this vFSMState state)
        {
            state.nodeRect.width = 150;
            state.nodeRect.height = 30;
            state.transitions = new List<vStateTransition>();
        }

        /// <summary>
        /// Update Node GUI for Node
        /// </summary>
        /// <param name="state"></param>
        /// <param name="e"></param>
        /// <param name="viewRect"></param>
        /// <param name="viewSkin"></param>
        public static void UpdateNodeGUI(this vFSMState state, Event e, Rect viewRect, GUISkin viewSkin)
        {
            //ProcessEvents(e, viewRect);
            var color = GUI.color;
            GUI.color = state.nodeColor;
            vIFSMBehaviourController fsmBehaviour = (Selection.activeGameObject ? Selection.activeGameObject.GetComponent<vIFSMBehaviourController>() : null);

            if (fsmBehaviour != null)
            {
                var controller = (Selection.activeGameObject ? Selection.activeGameObject.GetComponent<vIControlAI>() : null);
                if (controller != null)
                {
                    var interfaces = controller.GetType().GetInterfaces();
                    bool contains = false;
                    for (int i = 0; i < interfaces.Length; i++)
                    {
                        if (interfaces[i].Equals(state.requiredType))
                        {
                            contains = true;
                            break;
                        }
                    }
                    if (!contains)
                        Debug.Log("REQUIRED TYPE OF CONTROLLER IS " + state.requiredType.Name);
                }
            }
            bool isRunningInPlayMode = fsmBehaviour != null && Application.isPlaying && fsmBehaviour.fsmBehaviour && fsmBehaviour.fsmBehaviour.states.Contains(state) && fsmBehaviour.fsmBehaviour.states.IndexOf(state) == fsmBehaviour.indexOffCurrentState;

            var nodeStyle = isRunningInPlayMode ? viewSkin.GetStyle("NodeCurrent") : state.isSelected ? viewSkin.GetStyle("NodeSelected") : viewSkin.GetStyle("NodeDefault");
            GUI.color = Color.green;
            if (state.parentGraph && state.parentGraph.states[0] && state.parentGraph.states[0].defaultTransition == state)
            {
                var shadowRect = new Rect(state.nodeRect.x - 5, state.nodeRect.y - 5, state.nodeRect.width + 10, state.nodeRect.height + 10);
                GUI.Box(shadowRect, "", viewSkin.GetStyle("Glow"));
            }
            GUI.color = state.nodeColor;
            if (isRunningInPlayMode)
            {
                var shadowRect = new Rect(state.nodeRect.x - 10, state.nodeRect.y - 10, state.nodeRect.width + 20, state.nodeRect.height + 20);
                GUI.Box(shadowRect, "", viewSkin.GetStyle("Glow"));
            }
            GUI.SetNextControlName(state.name);
            GUI.Box(state.nodeRect, "", nodeStyle);
            GUI.Box(new Rect(state.nodeRect.x, state.nodeRect.y, state.nodeRect.width, state.nodeRect.height), "", nodeStyle);

            GUI.color = color;
            GUILayout.BeginArea(state.nodeRect);
            {
                try
                {
                    var style = new GUIStyle(nodeStyle);
                    style.normal.background = null;
                    style.hover.background = null;
                    style.active.background = null;
                    style.alignment = TextAnchor.MiddleCenter;
                    GUILayout.Label(new GUIContent(state.Name, state.description), style, GUILayout.Height(30));
                }
                catch { }

            }
            GUILayout.EndArea();
            state.UpdateStateGUI(e, viewRect, viewSkin);
            EditorUtility.SetDirty(state);

        }

        /// <summary>
        /// Update Node GUI for vFSMState
        /// </summary>
        /// <param name="state"></param>
        /// <param name="e"></param>
        /// <param name="viewRect"></param>
        /// <param name="viewSkin"></param>
        public static void UpdateStateGUI(this vFSMState state, Event e, Rect viewRect, GUISkin viewSkin)
        {
            if ((state.useDecisions) && state.transitions.Count > 0)
            {
                var foldoutRect = new Rect(state.nodeRect.x, state.nodeRect.y + 5, 20, 20);
                state.isOpen = EditorGUI.Foldout(foldoutRect, state.isOpen, "");
            }
            else
                state.isOpen = false;

            if (state.useDecisions)
                state.DrawTransitionHandles(e,viewRect, viewSkin);

            var resizeLeft = state.nodeRect;
            var resizeRight = state.nodeRect;
            resizeLeft.width = 2;
            resizeRight.width = 2;
            resizeRight.x += state.nodeRect.width;
            state.Resize(resizeLeft, e, ref state.resizeLeft, true);
            state.Resize(resizeRight, e, ref state.resizeRight, false);
        }

        public static void Resize(this vFSMState state, Rect rect, Event e, ref bool inResize, bool left = false)
        {
            if (!inResize)
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);
            else EditorGUIUtility.AddCursorRect(new Rect(e.mousePosition.x - 25, e.mousePosition.y - 25, 50, 50), MouseCursor.ResizeHorizontal);
            if (rect.Contains(e.mousePosition))
            {
                if (e.type == EventType.MouseDown)
                {
                    inResize = true;
                }
            }

            if (e.type == EventType.MouseUp)
            {
                inResize = false;
                // resizingPropView = false;
            }
            if (e.type == EventType.MouseDrag)
            {
                if (e.button == 0)
                {
                    if (inResize)
                    {
                        if (state.nodeRect.width <= 400 && state.nodeRect.width >= 100)
                        {
                            if (left)
                            {
                                if ((state.nodeRect.width - e.delta.x) <= 400 && (state.nodeRect.width - e.delta.x) >= 100)
                                {
                                    state.nodeRect.width += -e.delta.x;
                                    state.nodeRect.x += e.delta.x;
                                }
                            }
                            else if ((state.nodeRect.width + e.delta.x) <= 400 && (state.nodeRect.width + e.delta.x) >= 100)
                            {
                                state.nodeRect.width += e.delta.x;
                            }
                        }
                        else if (state.nodeRect.width < 100)
                        {
                            state.nodeRect.width = 100;
                        }
                        else if (state.nodeRect.width > 400)
                        {
                            state.nodeRect.width = 400;
                        }

                        e.Use();
                    }
                }
            }
        }

        static void DrawTransitionHandles(this vFSMState state, Event e,Rect viewRect, GUISkin viewSkin)
        {
            var color = GUI.color;
         
            if (state.transitions.Count > 0)
            {
                Vector2 transitionSize = new Vector2(10, 7.5f);
                float space = state.isOpen ? 10f : 0;
                float height = state.isOpen ? ((((transitionSize.y * 2) * state.transitions.Count)) + (state.transitions.Count > 0 ? space * state.transitions.Count : 0) + 30) : 30;
                state.nodeRect.height = height;
                var labelStyle = new GUIStyle(EditorStyles.whiteMiniLabel);
                labelStyle.alignment = TextAnchor.MiddleCenter;

                for (int i = 0; i < state.transitions.Count; i++)
                {
                    var nullDecisions = state.transitions[i].decisions.FindAll(t => t.decision == null);
                    for (int iNull = 0; iNull < nullDecisions.Count; iNull++) state.transitions[i].decisions.Remove(nullDecisions[iNull]);
                    bool trueRightSide = state.transitions[i].trueState ? state.nodeRect.x > state.transitions[i].trueState.nodeRect.x ? false : true : true;
                    bool falseRightSide = state.transitions[i].falseState ? state.nodeRect.x > state.transitions[i].falseState.nodeRect.x ? false : true : true;
                    state.transitions[i].trueSideRight = trueRightSide;
                    state.transitions[i].falseSideRight = falseRightSide;
                    state.transitions[i].trueRect.width = transitionSize.x;
                    state.transitions[i].trueRect.height = transitionSize.y;
                    state.transitions[i].falseRect.width = transitionSize.x;
                    state.transitions[i].falseRect.height = transitionSize.y;

                    state.transitions[i].trueRect.x = trueRightSide ? (state.nodeRect.x + state.nodeRect.width) : state.nodeRect.x - transitionSize.x;
                    state.transitions[i].trueRect.y = state.isOpen ? ((state.nodeRect.y + ((transitionSize.y * 2) * i)) + (i > 0 ? space * i : 0) + 30) : (state.nodeRect.y + 15) - transitionSize.y;
                    state.transitions[i].falseRect.x = falseRightSide ? (state.nodeRect.x + state.nodeRect.width) : state.nodeRect.x - transitionSize.x;
                    state.transitions[i].falseRect.y = state.isOpen ? ((state.nodeRect.y + (transitionSize.y + ((transitionSize.y * 2) * i))) + (i > 0 ? space * i : 0) + 30) : transitionSize.y + (state.nodeRect.y + 15) - transitionSize.y;
                    GUI.color = state.nodeColor;

                    var decisionRect = new Rect(state.nodeRect.x + state.nodeRect.width * 0.05f, state.transitions[i].trueRect.y, state.nodeRect.width * 0.9f, transitionSize.y * 2);

                    if (state.isOpen)
                    {
                        GUI.enabled = state.selectedDecisionIndex == i;
                        GUILayout.BeginArea(decisionRect, "", EditorStyles.helpBox);
                        {
                            try
                            {
                                GUI.color = color;
                                var text = "";
                                if (trueRightSide && falseRightSide || (!trueRightSide && !falseRightSide) || (!trueRightSide && falseRightSide))
                                {
                                    if (state.transitions[i].transitionType == vTransitionOutputType.Both || state.transitions[i].transitionType == vTransitionOutputType.True) text += (state.transitions[i].trueState ? state.transitions[i].trueState.name : "None");
                                    if (state.transitions[i].transitionType == vTransitionOutputType.Both) text += " || ";
                                    if (state.transitions[i].transitionType == vTransitionOutputType.Both || state.transitions[i].transitionType == vTransitionOutputType.False) text += (state.transitions[i].falseState ? state.transitions[i].falseState.name : "None");
                                }
                                else if (trueRightSide && !falseRightSide)
                                {
                                    if (state.transitions[i].transitionType == vTransitionOutputType.Both || state.transitions[i].transitionType == vTransitionOutputType.False) text += (state.transitions[i].falseState ? state.transitions[i].falseState.name : "None");
                                    if (state.transitions[i].transitionType == vTransitionOutputType.Both) text += " || ";
                                    if (state.transitions[i].transitionType == vTransitionOutputType.Both || state.transitions[i].transitionType == vTransitionOutputType.True) text += (state.transitions[i].trueState ? state.transitions[i].trueState.name : "None");
                                }
                                GUILayout.Space(-transitionSize.y / 2);

                                GUILayout.Label(text, labelStyle);
                            }
                            catch { }
                        }
                        GUILayout.EndArea();
                        GUI.enabled = true;

                        if (GUI.Button(decisionRect, "", GUIStyle.none) && viewRect.Contains(e.mousePosition))
                        {
                            state.parentGraph.onSelectState(state);
                            state.selectedDecisionIndex = i;
                            state.transitions[i].Select();
                        }
                        
                        if (decisionRect.Contains(e.mousePosition) && viewRect.Contains(e.mousePosition))
                        {
                            if (e.button == 1)
                            {
                                GenericMenu menu = new GenericMenu();
                                var transition = new vStateTransition(null);

                                for (int a = 0; a < state.transitions[i].decisions.Count; a++)
                                {
                                    transition.decisions.Add(state.transitions[i].decisions[a].Copy());
                                }
                                int index = i;
                                var transitionToChange = state.transitions[i];
                                menu.AddItem(new GUIContent("Remove"), false, () =>
                                {
                                    transitionToChange.parentState.transitions.RemoveAt(index); e.Use();
                                });

                                menu.AddItem(new GUIContent("Duplicate"), false, () =>
                                {
                                    state.transitions.Add(transition);
                                    SerializedObject serializedObject = new SerializedObject(state);
                                    serializedObject.ApplyModifiedProperties();
                                    e.Use();
                                });

                                menu.ShowAsContext();
                            }
                        }
                    }
                    GUI.enabled = true;
                    GUI.color = color;
                    decisionRect.x = decisionRect.x + decisionRect.width;
                    decisionRect.width = 15;
                    decisionRect.height = 15;
                    GUI.color = state.parentGraph.transitionTrueColor;

                    if (state.transitions[i].useTruState)
                    {
                        GUI.Box(state.transitions[i].trueRect, "", viewSkin.GetStyle("InputButton"));
                        if (state.isOpen && state.transitions[i].trueRect.Contains(e.mousePosition) && e.type == EventType.MouseDown)
                        {
                            if (e.button == 0)
                            {
                                state.parentGraph.wantConnection = true;
                                state.parentGraph.transitionPreview.sideRight = state.transitions[i].trueSideRight;
                                state.parentGraph.transitionPreview.transitionRect = state.transitions[i].trueRect;
                                state.parentGraph.transitionPreview.onValidate = state.transitions[i].SetTrueState;
                            }
                        }
                    }

                    GUI.color = state.parentGraph.transitionFalseColor;
                    if (state.transitions[i].useFalseState)
                    {
                        GUI.Box(state.transitions[i].falseRect, "", viewSkin.GetStyle("InputButton"));
                        if (state.transitions[i].falseRect.Contains(e.mousePosition) && e.type == EventType.MouseDown)
                        {
                            if (e.button == 0)
                            {
                                state.parentGraph.wantConnection = true;
                                state.parentGraph.transitionPreview.sideRight = state.transitions[i].falseSideRight;
                                state.parentGraph.transitionPreview.transitionRect = state.transitions[i].falseRect;
                                state.parentGraph.transitionPreview.onValidate = state.transitions[i].SetFalseState;
                            }
                        }
                    }
                }
            }
            else state.nodeRect.height = 30;
            GUI.color = color;
        }

        static void AddObjectContext(this vFSMState state)
        {
            try
            {
                GenericMenu menu = new GenericMenu();
                menu.AddDisabledItem(new GUIContent("FSM Component"));
                menu.AddSeparator("");
                if (state.useActions)
                    state.AddActionsMenu(ref menu);
                if (state.useDecisions)
                    state.AddTransitionMenu(ref menu);
                menu.ShowAsContext();
            }
            catch { }
        }

        static void AddActionsMenu(this vFSMState state, ref GenericMenu menu)
        {
            for (int i = 0; i < state.parentGraph.actions.Count; i++)
            {
                //if(!parentGraph.actions[i].Equals(parentGraph))
                {
                    if (state.parentGraph.actions[i] && state.parentGraph.actions[i].target)
                    {
                        if (state.parentGraph.actions[i].target.GetType().IsSubclassOf(typeof(vStateAction)) || state.parentGraph.actions[i].target.GetType().Equals(typeof(vStateAction)))
                        {
                            var action = state.parentGraph.actions[i].target as vStateAction;
                            menu.AddItem(new GUIContent("Action/" + state.parentGraph.actions[i].target.name + " " + " (" + state.parentGraph.actions[i].target.GetType().Name + ")"), false, () =>
                            {
                               
                                state.actions.Add(action);
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();
                            });
                        }
                    }
                }
            }
        }

        static void AddTransitionMenu(this vFSMState state, ref GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Transition", "Create a New Empty Transition"), false, () =>
             {
                 state.transitions.Add(new vStateTransition(null));
                 EditorUtility.SetDirty(state);
                 AssetDatabase.SaveAssets();
                 AssetDatabase.Refresh();
             });

        }

        static void AddDecisionsMenu(this vStateTransition transition, ref GenericMenu menu)
        {
            for (int i = 0; i < transition.parentState.parentGraph.decisions.Count; i++)
            {
                //if(!parentGraph.actions[i].Equals(parentGraph))
                {
                    if (transition.parentState.parentGraph.decisions[i] && transition.parentState.parentGraph.decisions[i].target)
                    {
                        if (transition.parentState.parentGraph.decisions[i].target.GetType().IsSubclassOf(typeof(vStateDecision)) || transition.parentState.parentGraph.decisions[i].target.GetType().Equals(typeof(vStateDecision)))
                        {

                            var decision = transition.parentState.parentGraph.decisions[i].target as vStateDecision;
                            menu.AddItem(new GUIContent("Decision/" + transition.parentState.parentGraph.decisions[i].target.name + " " + " (" + transition.parentState.parentGraph.decisions[i].target.GetType().Name + ")"), false, () =>
                            {
                               
                                transition.decisions.Add(new vStateDecisionObject(decision));
                                EditorUtility.SetDirty(transition.parentState);
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();
                            });
                        }
                    }
                }
            }
        }

        public static void DrawPrimaryProperties(this vFSMState state, SerializedObject serializedObject, GUISkin viewSkin)
        {
            SerializedProperty description = serializedObject.FindProperty("description");
            if (description != null) EditorGUILayout.PropertyField(description);
            if (state.canEditColor)
            {
                SerializedProperty color = serializedObject.FindProperty("nodeColor");
                if (color != null) EditorGUILayout.PropertyField(color);
            }
            if (state.canEditName)
            {
                SerializedProperty changeCurrentSpeed = serializedObject.FindProperty("changeCurrentSpeed");
                SerializedProperty customSpeed = serializedObject.FindProperty("customSpeed");
                SerializedProperty resetCurrentDestination = serializedObject.FindProperty("resetCurrentDestination");
                if (changeCurrentSpeed != null) EditorGUILayout.PropertyField(changeCurrentSpeed);
                if (customSpeed != null) EditorGUILayout.PropertyField(customSpeed);
                if (resetCurrentDestination != null) EditorGUILayout.PropertyField(resetCurrentDestination);
            }
        }

        public static void DrawProperties(this vFSMState state, SerializedObject serializedObject, GUISkin viewSkin)
        {
            try
            {
                Event e = Event.current;
                state.actions = state.actions.FindAll(a => a != null);
                state.transitions = state.transitions.FindAll(t => t != null);

                if (state.useActions)
                {
                    GUILayout.Space(10);
                    GUILayout.BeginVertical(viewSkin.GetStyle("NodeDefault"));
                    //Draw Actions
                    {
                        GUILayout.Label("State Actions", viewSkin.GetStyle("LabelHeader"), GUILayout.ExpandWidth(true)); GUILayout.Space(5);
                        if (state.actions.Count > 0 && state.parentGraph.actions.Count > 0)
                        {
                            var actionsToDraw = state.parentGraph.actions.FindAll(a => state.actions.Contains(a.target as vStateAction));

                            var rect = new Rect();
                            bool click = false;
                            for (int i = 0; i < state.actions.Count; i++)
                            {
                                state.actions[i].parentFSM = state.parentGraph;
                            }
                            for (int i = 0; i < actionsToDraw.Count; i++)
                            {
                                if (!(actionsToDraw[i] == null || actionsToDraw[i].target == null))
                                {
                                    actionsToDraw[i].OnInspectorGUI();
                                    rect = GUILayoutUtility.GetLastRect();
                                    rect.x = rect.width - EditorGUIUtility.singleLineHeight * 0.4f;
                                    rect.height = EditorGUIUtility.singleLineHeight;
                                    rect.width = EditorGUIUtility.singleLineHeight;
                                    click = GUI.Button(rect, "-", viewSkin.GetStyle("NodeDefault"));
                                    if (rect.Contains(e.mousePosition) && click)
                                    {
                                        if (e.button == 0)
                                        {
                                            GenericMenu menu = new GenericMenu();
                                            int index = state.actions.IndexOf(actionsToDraw[i].target as vStateAction);
                                            menu.AddItem(new GUIContent("Remove"), false, () => { state.actions.RemoveAt(index); e.Use(); });
                                            menu.ShowAsContext();
                                        }
                                    }
                                    click = false;
                                }
                            }
                        }
                    }
                    GUILayout.EndVertical();

                    /*Add Actions To State*/
                    {
                        var plusButtonRect = GUILayoutUtility.GetLastRect();
                        plusButtonRect.y += plusButtonRect.height;
                        plusButtonRect.x += plusButtonRect.width - EditorGUIUtility.singleLineHeight;
                        plusButtonRect.width = EditorGUIUtility.singleLineHeight;
                        plusButtonRect.height = EditorGUIUtility.singleLineHeight;
                        if (GUI.Button(plusButtonRect, new GUIContent("+", "Add Decision"), viewSkin.GetStyle("NodeDefault")))
                        {
                            GenericMenu menu = new GenericMenu();
                            AddActionsMenu(state, ref menu);
                            menu.ShowAsContext();
                        }
                    }
                }


                if (state.useDecisions && state.transitions.Count > 0)
                {
                    GUILayout.Space(EditorGUIUtility.singleLineHeight * 2);
                    GUILayout.Box("", viewSkin.GetStyle("Separator"), GUILayout.Height(2), GUILayout.ExpandWidth(true));
                    GUILayout.Space(EditorGUIUtility.singleLineHeight);
                    GUILayout.BeginVertical(viewSkin.GetStyle("NodeDefault"));
                    GUILayout.BeginVertical(viewSkin.GetStyle("NodeDefault"));
                    //Draw Transition Selector
                    {
                        GUILayout.Label("State Transitions ", viewSkin.GetStyle("LabelHeader"), GUILayout.ExpandWidth(true)); GUILayout.Space(5);

                        for (int i = 0; i < state.transitions.Count; i++)
                        {
                            GUILayout.BeginVertical("", viewSkin.GetStyle("NodeDefault"));
                            {
                                GUI.enabled = state.selectedDecisionIndex == i;
                                if (!state.transitions[i].parentState) state.transitions[i].parentState = state;

                                state.transitions[i].DrawTransitionSelector(e, viewSkin, (state.selectedDecisionIndex == i));

                                GUI.enabled = true;
                            }
                            GUILayout.EndVertical();
                            var decisionRect = GUILayoutUtility.GetLastRect();
                            if (GUI.Button(decisionRect, "", GUIStyle.none))
                            {
                                if (state.selectedDecisionIndex != i)
                                {
                                    state.selectedDecisionIndex = i;
                                    state.transitions[i].Select();

                                    if (Selection.activeObject != state)
                                        Selection.activeObject = state;
                                }
                                else
                                {
                                    state.transitions[i].Deselect();
                                }
                            }
                        }
                    }

                    GUILayout.EndVertical();
                    GUILayout.BeginVertical();
                    {
                        GUILayout.Space(10);
                        if (state.transitions.Count > 0 && state.selectedDecisionIndex >= 0 && state.selectedDecisionIndex < state.transitions.Count)
                        {
                            state.transitions[state.selectedDecisionIndex].DrawTransitionsProperties(e, viewSkin, true);
                        }
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndVertical();
                }
            }
            catch { }
        }

        public static void UpdateNodeConnections(this vFSMState state,Rect viewRect, Event e)
        {
            if (state.useDecisions) state.UpdateStateConnections(viewRect,e);
            else if (state.defaultTransition)
                DrawNodeCurve(e, state.nodeRect, state.defaultTransition.nodeRect, Color.white);
        }

        static void UpdateStateConnections(this vFSMState state, Rect viewRect, Event e)
        {
            for (int i = state.transitions.Count - 1; i >= 0; i--)
            {
                if (state.transitions[i].trueState && state.transitions[i].useTruState)
                {
                    state.DrawNodeCurve(e, state.transitions[i].trueRect, state.transitions[i].trueState.nodeRect, state.parentGraph.transitionTrueColor, state.transitions[i], true);
                }
                if (state.transitions[i].falseState && state.transitions[i].useFalseState)
                {
                    state.DrawNodeCurve(e, state.transitions[i].falseRect, state.transitions[i].falseState.nodeRect, state.parentGraph.transitionFalseColor, state.transitions[i], false);
                }
            }
            for (int i = 0; i < state.transitions.Count; i++)
            {
                if (state.transitions[i].trueState && state.transitions[i].useTruState)
                {
                    state.DrawNodeCurveSelectable(viewRect, e, state.transitions[i].trueRect, state.transitions[i].trueState.nodeRect, state.parentGraph.transitionTrueColor, state.transitions[i], true);
                }
                if (state.transitions[i].falseState && state.transitions[i].useFalseState)
                {
                    state.DrawNodeCurveSelectable(viewRect, e, state.transitions[i].falseRect, state.transitions[i].falseState.nodeRect, state.parentGraph.transitionFalseColor, state.transitions[i], false);
                }
            }
        }

        static void DrawNodeCurveSelectable(this vFSMState state,Rect viewRect, Event e, Rect start, Rect end, Color color, vStateTransition transition, bool value)
        {
            Handles.BeginGUI();
            Vector3 startPos = Vector3.zero;
            Vector3 endPos = Vector3.zero;
            Vector3 startTan = Vector3.zero;
            Vector3 endTan = Vector3.zero;
            CalculateBezier(start, end, transition, value, ref startPos, ref startTan, ref endPos, ref endTan);
            var dist = (endPos - startPos).magnitude;
            var points = Handles.MakeBezierPoints(startPos, endPos, startTan, endTan, (int)(Mathf.Clamp(dist, 2, 100)));
            var length = (uint)points.Length;
            var transitionCount = state.transitions.FindAll(t => state.transitions.IndexOf(t) > state.transitions.IndexOf(transition) && ((value && t.trueState && t.trueState == transition.trueState) || (!value && t.falseState && t.falseState == transition.falseState)));

            if (!state.isOpen && transitionCount.Count > 0)
            {
                length = (uint)Mathf.Clamp((points.Length - (points.Length * .15f) * (1 + transitionCount.Count)), 2, points.Length);
            }
            #region Debug Selector
            //for (int i = 0; i < length; i++)
            //{
            //    var rect = new Rect(points[i].x - ((i == length - 1) ? 5 : 2.5f), points[i].y - ((i == length - 1) ? 5 : 2.5f), ((i == length - 1) ? 10 : 5f), ((i == length - 1) ? 10 : 5f));

            //    GUI.Box(rect, "-");
            //}
            #endregion
            if (e.type == EventType.MouseDown && !state.parentGraph.overNode && viewRect.Contains(e.mousePosition))
            {
                var buttom = e.button;
                var selected = false;

                for (int i = 0; i < length; i++)
                {
                    var rect = new Rect(points[i].x - ((i == length - 1) ? 10 : 2.5f), points[i].y - ((i == length - 1) ? 10 : 2.5f), ((i == length - 1) ? 20 : 5f), ((i == length - 1) ? 20 : 5f));
                    if (rect.Contains(e.mousePosition))
                    {
                        selected = true;
                        state.isSelected = true;
                        Selection.activeObject = state;
                        state.parentGraph.selectedNode = state;
                        state.selectedDecisionIndex = state.transitions.IndexOf(transition);
                        state.parentGraph.DeselectAllExcludinCurrent();
                    }
                }

                if (selected)
                {
                    if (buttom == 1)
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Delete"), false, () => { if (value) transition.trueState = null; else transition.falseState = null; });
                        menu.ShowAsContext();
                    }
                    else
                    {
                        if (value)
                        {
                            transition.Select(false, true);
                        }
                        else
                        {
                            transition.Select(true, false);
                        }
                        EditorUtility.SetDirty(state);
                    }
                    e.Use();
                }
            }
            Handles.EndGUI();
        }

        static void DrawNodeCurve(this vFSMState state, Event e, Rect start, Rect end, Color color, vStateTransition transition, bool value)
        {
            //if(decision!=null)
            {
                Handles.BeginGUI();
                Vector3 startPos = Vector3.zero;
                Vector3 endPos = Vector3.zero;
                Vector3 startTan = Vector3.zero;
                Vector3 endTan = Vector3.zero;
                CalculateBezier(start, end, transition, value, ref startPos, ref startTan, ref endPos, ref endTan);
                var dist = (endPos - startPos).magnitude;

                var points = Handles.MakeBezierPoints(startPos, endPos, startTan, endTan, (int)(Mathf.Clamp(dist, 2, 100)));
                var length = (uint)0;
                var transitionCount = state.transitions.FindAll(t => state.transitions.IndexOf(t) > state.transitions.IndexOf(transition) && ((value && t.trueState && t.trueState == transition.trueState) || (!value && t.falseState && t.falseState == transition.falseState)));

                if (!state.isOpen && transitionCount.Count > 0)
                {
                    length = (uint)Mathf.Clamp((points.Length - (points.Length * .15f) * (1 + transitionCount.Count)), 2, points.Length);
                    var pRef = new Vector3[length];
                    for (int i = 0; i < length; i++)
                    {
                        pRef[i] = points[i];
                    }
                    points = pRef;
                }
                if (transition.decisions.Count == 0) color = Color.grey;
                var lineWidth = 3;
                var isSelectedLine = (value) ? transition.selectedTrue : transition.selectedFalse;
                if (isSelectedLine)
                {
                    color = color + Color.cyan;
                    lineWidth = 5;
                }

                var _color = Handles.color;
                Handles.color = color;
                Handles.DrawAAPolyLine(Resources.Load("line") as Texture2D, lineWidth, points);
                DrawArrow(points[Mathf.Clamp(points.Length - 10, 0, points.Length)], points[points.Length - 1], lineWidth, color);
                Handles.color = _color;
                Handles.EndGUI();
            }
        }

        static void DrawNodeCurve(Event e, Rect start, Rect end, Color color)
        {
            //if(decision!=null)
            {
                Handles.BeginGUI();

                Vector3 startPos = new Vector3(start.x + start.width / 2, start.y + start.height / 2, 0);
                Vector3 endPos = new Vector3(end.x + end.width * 0.5f, end.y + end.height * 0.5f, 0);
                var dist = (endPos - startPos).magnitude;
                Bounds bound = new Bounds(end.center, end.size);
                endPos = bound.ClosestPoint(startPos) - (dist < 10 ? Vector3.zero : (endPos - startPos).normalized * 10);
                var magniture = Mathf.Clamp(((endPos - startPos).magnitude / 200f) - 0.5f, 0f, 1f);
                Vector3 startTan = startPos;
                var endTanDir = -(endPos - startPos).normalized;
                Vector3 endTan = endPos + endTanDir * (100 * magniture);
                var lineWidth = 4;
                Handles.DrawBezier(startPos, endPos, startTan, endTan, color, Resources.Load("line") as Texture2D, lineWidth);
                DrawArrow(startPos, endPos, lineWidth, color);
                Handles.EndGUI();
            }
        }

        static void CalculateBezier(Rect start, Rect end, vStateTransition transition, bool value, ref Vector3 refStart, ref Vector3 refStartTan, ref Vector3 refEnd, ref Vector3 refEndTan)
        {
            Handles.BeginGUI();
            var sideRight = value ? transition.trueSideRight : transition.falseSideRight;
            Vector3 startPos = new Vector3(start.x + (sideRight ? start.width : 0), start.y + start.height / 2, 0);
            Vector3 endPos = new Vector3(end.x + end.width * 0.5f, end.y + end.height * 0.5f, 0);
            Bounds bound = new Bounds(value ? transition.trueState.nodeRect.center : transition.falseState.nodeRect.center, value ? transition.trueState.nodeRect.size : transition.falseState.nodeRect.size);
            endPos = bound.ClosestPoint(startPos) + (startPos - endPos).normalized * 20f;
            var magniture = Mathf.Clamp(((endPos - startPos).magnitude / 200) - 0.5f, 0f, 1f);
            Vector3 startTan = startPos + (sideRight ? Vector3.right : Vector3.left) * (200 * magniture);
            var endTanDir = -(endPos - startPos).normalized;
            Vector3 endTan = endPos + endTanDir * (100 * magniture);

            refStart = startPos;
            refStartTan = startTan;
            refEnd = endPos;
            refEndTan = endTan;
            Handles.EndGUI();
        }

        static void DrawArrow(Vector3 start, Vector3 end, float lineWidth, Color color)
        {
            var forward = Quaternion.identity * (new Vector3(end.x, 0, end.y) - new Vector3(start.x, 0, start.y));
            Matrix4x4 m = GUI.matrix;
            var _color = GUI.color;

            GUI.color = color;

            var texture = Resources.Load("line_arrow") as Texture2D;
            var width = 8 + lineWidth * 2;
            var pos = new Vector3(end.x - width * 0.5f, end.y - width * 0.5f, 0);

            var angle = new Vector3(0, -(Quaternion.LookRotation(forward).eulerAngles.y - 90), 0).NormalizeAngle().y;

            GUIUtility.RotateAroundPivot(angle, end);
            GUI.DrawTexture(new Rect(pos.x, pos.y, width, width), texture);
            GUI.matrix = m;
            GUI.color = _color;
        }

        public static void OnDrag(this vFSMState state, Vector2 delta)
        {
            state.nodeRect.x += delta.x;
            state.nodeRect.y += delta.y;
        }

        public static void DrawTransitionSelector(this vStateTransition transition, Event e, GUISkin viewSkin, bool isSelected = false)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                {
                    var color = GUI.color;
                    transition.DrawConnectionText(true, true);
                    GUI.color = color;

                    if (GUILayout.Button(new GUIContent("X", "Remove Transition"), viewSkin.GetStyle("NodeDefault"), GUILayout.Width(EditorGUIUtility.singleLineHeight), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                    {
                        GenericMenu menu = new GenericMenu();
                        int index = transition.parentState.transitions.IndexOf(transition);
                        menu.AddItem(new GUIContent("Remove"), false, () =>
                        {
                           
                            transition.parentState.transitions.RemoveAt(index); e.Use();
                        });
                        menu.ShowAsContext();
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Output", EditorStyles.whiteMiniLabel);
                if (transition.decisions.Count > 0)
                {
                    transition.transitionType = (vTransitionOutputType)EditorGUILayout.EnumPopup("", transition.transitionType, viewSkin.GetStyle("DropDown"), GUILayout.Width(50), GUILayout.Height(EditorGUIUtility.singleLineHeight * 0.9f));
                }
                else
                {
                    transition.transitionType = vTransitionOutputType.True;
                    GUILayout.Box(new GUIContent(transition.transitionType.ToString(), "If transition dont have decisions, this value is ever true"), viewSkin.GetStyle("DropDown"), GUILayout.Width(50), GUILayout.Height(EditorGUIUtility.singleLineHeight * 0.9f));
                }
                GUILayout.FlexibleSpace();
                GUILayout.Label("Transition Delay", EditorStyles.whiteMiniLabel);
                transition.transitionDelay = EditorGUILayout.FloatField("", transition.transitionDelay, GUILayout.Width(50));
                if (GUI.changed) EditorUtility.SetDirty(transition.parentState);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        public static void DrawTransitionsProperties(this vStateTransition transition, Event e, GUISkin viewSkin, bool isSelected = false)
        {
            if (isSelected)
            {
                GUILayout.Space(10);
                GUILayout.Box("", viewSkin.GetStyle("Separator"), GUILayout.Height(2), GUILayout.ExpandWidth(true));
                GUILayout.Space(10);
                GUILayout.BeginVertical(viewSkin.GetStyle("NodeDefault"));
                {
                    var stl = new GUIStyle(EditorStyles.helpBox);
                    stl.alignment = TextAnchor.MiddleCenter;
                    GUILayout.Label("Decisions", stl, GUILayout.ExpandWidth(true), GUILayout.Height(EditorGUIUtility.singleLineHeight));

                    if (transition.decisions.Count > 0)
                    {
                        EditorGUILayout.HelpBox("Decisions has True or False option to validade", MessageType.Info);
                        var rect = new Rect();
                        vIFSMBehaviourController fsmBehaviour = (Selection.activeGameObject && Application.isPlaying ? Selection.activeGameObject.GetComponent<vIFSMBehaviourController>() : null);
                        bool isRunningInPlayMode = fsmBehaviour != null && fsmBehaviour.fsmBehaviour;// && fsmBehaviour.fsmController.states.Contains(transition.parentState) && fsmBehaviour.fsmController.states.IndexOf(transition.parentState) == fsmBehaviour.indexOffCurrentState;
                        bool click = false;
                        for (int i = 0; i < transition.decisions.Count; i++)
                        {
                            if (transition.decisions[i].decision)
                            {
                                transition.decisions[i].decision.parentFSM = transition.parentState.parentGraph;
                            }

                            var color = GUI.color;
                            if (isRunningInPlayMode)
                            {
                                GUI.color = transition.decisions[i].isValid ? Color.green : Color.red;
                            }
                            transition.decisions[i].DrawDecisionEditor();
                            GUI.color = color;
                            rect = GUILayoutUtility.GetLastRect();
                            rect.x = rect.width - 50;
                            rect.width = 44;
                            rect.y += 2;
                            rect.height = EditorGUIUtility.singleLineHeight;
                            rect.height -= 4;
                            transition.decisions[i].trueValue = (EditorGUI.Popup(rect, transition.decisions[i].trueValue ? 0 : 1, new string[] { "true", "false" }, viewSkin.GetStyle("DropDown")) == 0 ? true : false);

                            rect.y -= 2;
                            rect.height += 4;
                            rect.x += 44;// rect.width - EditorGUIUtility.singleLineHeight * 0.4f;
                            rect.width = EditorGUIUtility.singleLineHeight;
                            click = GUI.Button(rect, "-", viewSkin.GetStyle("NodeDefault"));
                            if (rect.Contains(e.mousePosition) && click)
                            {
                                if (e.button == 0)
                                {                                 
                                    transition.decisions.RemoveAt(i);
                                }
                            }
                            click = false;
                        }
                    }
                }
                GUILayout.EndVertical();

                /*Add Decisions To Transition*/
                {
                    var plusButtonRect = GUILayoutUtility.GetLastRect();
                    plusButtonRect.y += plusButtonRect.height;
                    plusButtonRect.x += plusButtonRect.width - EditorGUIUtility.singleLineHeight;
                    plusButtonRect.width = EditorGUIUtility.singleLineHeight;
                    plusButtonRect.height = EditorGUIUtility.singleLineHeight;
                    if (GUI.Button(plusButtonRect, new GUIContent("+", "Add Decision"), viewSkin.GetStyle("NodeDefault")))
                    {
                        GenericMenu menu = new GenericMenu();
                        transition.AddDecisionsMenu(ref menu);
                        e.Use();
                        menu.ShowAsContext();
                    }
                }
            }
        }

        public static void DrawConnectionText(this vStateTransition transition, bool ignoreTrue = false, bool ignoreFalse = false)
        {
            GUILayout.BeginVertical();
            var fontTransitionStyle = new GUIStyle(UnityEditor.EditorStyles.whiteMiniLabel);
            if (transition.useTruState && (transition.selectedTrue || ignoreTrue))
                GUILayout.Label("True: " + transition.parentState.name + "   >>>  " + (transition.trueState ? transition.trueState.name : "None"), fontTransitionStyle);
            if (transition.useFalseState && (transition.selectedFalse || ignoreFalse))
                GUILayout.Label("False: " + transition.parentState.name + "   >>>  " + (transition.falseState ? transition.falseState.name : "None"), fontTransitionStyle);
            GUILayout.EndVertical();
        }

        public static void Deselect(this vStateTransition transition)
        {
            if (!transition.parentState) return;
            for (int i = 0; i < transition.parentState.transitions.Count; i++)
            {
                transition.parentState.transitions[i].selectedFalse = false;
                transition.parentState.transitions[i].selectedTrue = false;
            }
            transition.selectedTrue = false;
            transition.selectedFalse = false;
        }

        public static void Select(this vStateTransition transition, bool selectFalse = true, bool selectTrue = true)
        {
            if (!transition.parentState) return;

            for (int i = 0; i < transition.parentState.transitions.Count; i++)
            {
                transition.parentState.transitions[i].selectedFalse = false;
                transition.parentState.transitions[i].selectedTrue = false;
            }
            transition.selectedTrue = selectTrue ? true : false;
            transition.selectedFalse = selectFalse ? true : false;
        }
    }
}
#endif