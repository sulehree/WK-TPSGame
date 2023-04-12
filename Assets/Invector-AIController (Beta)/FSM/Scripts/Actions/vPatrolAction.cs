using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
#if UNITY_EDITOR
    [vFSMHelpbox("Make sure that your AI have a WaypointArea assign on the Controller Inspector", UnityEditor.MessageType.Info)]
#endif
    public class vPatrolAction : vStateAction
    {
        [vHelpBox("Character will look around using the Headtrack component and the 'LookAroundAngle' and 'LookAroundSpeed' variables that you can set in the Detection tab at the Controller Inspector")]
        public bool lookAroundOnStay = true;
        public bool debugMode;

        public override string defaultName
        {
            get
            {
                return "Patrol";
            }
        }

        public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
        {
            DoPatrolWaypoints(fsmBehaviour);
        }

        protected virtual void DoPatrolWaypoints(vIFSMBehaviourController fsmBehaviour)
        {
            if (fsmBehaviour == null) return;
            if (fsmBehaviour.aiController.isDead) return;

            if (fsmBehaviour.aiController.waypointArea != null && fsmBehaviour.aiController.waypointArea.waypoints.Count > 0)
            {
                if (fsmBehaviour.aiController.targetWaypoint == null || !fsmBehaviour.aiController.targetWaypoint.isValid)
                {
                    fsmBehaviour.aiController.NextWayPoint();
                }
                else
                {
                    if (Vector3.Distance(fsmBehaviour.aiController.transform.position, fsmBehaviour.aiController.targetWaypoint.position) <
                        fsmBehaviour.aiController.stopingDistance + fsmBehaviour.aiController.targetWaypoint.areaRadius + fsmBehaviour.aiController.changeWaypointDistance &&
                        fsmBehaviour.aiController.targetWaypoint.CanEnter(fsmBehaviour.aiController.transform) &&
                        !fsmBehaviour.aiController.targetWaypoint.IsOnWay(fsmBehaviour.aiController.transform))
                    {
                        fsmBehaviour.aiController.targetWaypoint.Enter(fsmBehaviour.aiController.transform);
                       
                    }
                    else if (Vector3.Distance(fsmBehaviour.aiController.transform.position, fsmBehaviour.aiController.targetWaypoint.position) <
                        fsmBehaviour.aiController.stopingDistance + fsmBehaviour.aiController.targetWaypoint.areaRadius &&
                        (!fsmBehaviour.aiController.targetWaypoint.CanEnter(fsmBehaviour.aiController.transform) ||
                        !fsmBehaviour.aiController.targetWaypoint.isValid))
                    {
                        fsmBehaviour.aiController.NextWayPoint();
                    }

                    if (fsmBehaviour.aiController.targetWaypoint != null &&
                        fsmBehaviour.aiController.targetWaypoint.IsOnWay(fsmBehaviour.aiController.transform) &&
                        Vector3.Distance(fsmBehaviour.aiController.transform.position, fsmBehaviour.aiController.targetWaypoint.position) <=
                        fsmBehaviour.aiController.targetWaypoint.areaRadius+ fsmBehaviour.aiController.changeWaypointDistance)
                    {
                        if (fsmBehaviour.aiController.remainingDistance <= (fsmBehaviour.aiController.stopingDistance + fsmBehaviour.aiController.changeWaypointDistance) || fsmBehaviour.aiController.isInDestination)
                        {
                            var timer = fsmBehaviour.GetTimer("Patrol");
                            if (timer >= fsmBehaviour.aiController.targetWaypoint.timeToStay || !fsmBehaviour.aiController.targetWaypoint.isValid)
                            {
                                fsmBehaviour.aiController.targetWaypoint.Exit(fsmBehaviour.aiController.transform);
                                fsmBehaviour.aiController.visitedWaypoints.Clear();
                                fsmBehaviour.aiController.NextWayPoint();
                                if (debugMode) Debug.Log("Sort new Waypoint");
                                fsmBehaviour.aiController.Stop();
                                fsmBehaviour.SetTimer("Patrol", 0);
                            }
                            else if (timer < fsmBehaviour.aiController.targetWaypoint.timeToStay)
                            {
                                if (debugMode) Debug.Log("Stay");
                                if (fsmBehaviour.aiController.targetWaypoint.rotateTo)
                                {                                  
                                    fsmBehaviour.aiController.LookTo(fsmBehaviour.transform.position + Vector3.up * 1.5f + fsmBehaviour.aiController.targetWaypoint.transform.forward * 10f, 0.1f);
                                    fsmBehaviour.aiController.RotateTo(fsmBehaviour.aiController.targetWaypoint.transform.forward);
                                }
                                else fsmBehaviour.aiController.Stop();
                                if (!fsmBehaviour.aiController.targetInLineOfSight && lookAroundOnStay)
                                {
                                    fsmBehaviour.aiController.LookAround();
                                }

                                fsmBehaviour.SetTimer("Patrol", timer + Time.deltaTime);
                            }
                        }
                    }
                    else
                    {                       
                        fsmBehaviour.aiController.MoveTo(fsmBehaviour.aiController.targetWaypoint.position);
                        if (debugMode) Debug.Log("Go to new Waypoint");
                    }
                }
            }
            else if(fsmBehaviour.aiController.selfStartingPoint)
            {
                if(fsmBehaviour.debugMode)
                    fsmBehaviour.SendDebug("MoveTo SelfStartPosition", this);
                fsmBehaviour.aiController.MoveTo(fsmBehaviour.aiController.selfStartPosition);                
            }
            else if(fsmBehaviour.aiController.customStartPoint)
            {
                if (fsmBehaviour.debugMode)
                    fsmBehaviour.SendDebug("MoveTo CustomStartPosition", this);
                fsmBehaviour.aiController.MoveTo(fsmBehaviour.aiController.customStartPosition);
            }
            else
            {
                if (fsmBehaviour.debugMode)
                    fsmBehaviour.SendDebug("Stop Patrolling", this);
                fsmBehaviour.aiController.Stop();
            }
        }
    }
}