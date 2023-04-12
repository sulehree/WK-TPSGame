using UnityEngine;
using System.Collections.Generic;

namespace Invector.vCharacterController.AI
{
    [SelectionBase]
    [vClassHeader("AI BASIC CONTROLLER", iconName = "AI-icon")]
    public class vControlAI : vAIMotor, vIControlAI
    {
        #region Inspector Variables
        [vEditorToolbar("Start")]
        public bool disableAgentOnStart = true;

        [vEditorToolbar("Agent", order = 5)]
        [SerializeField] protected bool useNavMeshAgent = true;
        [SerializeField] protected vAIUpdateQuality updatePathQuality = vAIUpdateQuality.Medium;
        [SerializeField] [Range(1f, 10f)] protected float aceleration = 8f;
        [SerializeField] [Range(0.05f, 10f)] protected float _stopingDistance = 0.2f;
        [Header("Increase StopingDistance by speed")]
        [SerializeField] [Range(0.05f, 10f)] protected float _walkingStopingDistance = 0.0f;
        [SerializeField] [Range(0.05f, 10f)] protected float _runningStopingDistance = 0.1f;
        [SerializeField] [Range(0.05f, 10f)] protected float _sprintingStopingDistance = 0.15f;

        [vEditorToolbar("Waypoint", order = 6)]
        [vHelpBox("You can create a new WaypointArea at the Invector/AIController/Components/Create new WaypointArea", vHelpBoxAttribute.MessageType.Info)]
        [SerializeField] protected vWaypointArea _waypointArea;
        [SerializeField] protected float _changeWaypointDistance;
        [SerializeField] protected bool _randomStartingPoint = true;
        [SerializeField] protected bool _randomWaypoint = true;
        [SerializeField] protected bool _selfStartingPoint;
        [SerializeField] protected Transform _customStartingPoint;

        [vEditorToolbar("Detection", order = 7)]
        [vHelpBox("Use a empty trasform inside the headBone transform as reference to the character Eyes", vHelpBoxAttribute.MessageType.None)]
        public Transform detectionPointReference;
        [SerializeField, vEnumFlag] public vAISightMethod sightMethod = vAISightMethod.Center | vAISightMethod.Top;
        [SerializeField] protected vAIUpdateQuality findTargetUpdateQuality = vAIUpdateQuality.High;
        [SerializeField] protected vAIUpdateQuality canseeTargetUpdateQuality = vAIUpdateQuality.Medium;
        [SerializeField, Tooltip("find target with current target found")] protected bool findOtherTarget = false;
        [SerializeField] protected float _changeTargetDelay = 2f;
        [SerializeField] protected bool findTargetByDistance = true;
        [SerializeField] protected float _fieldOfView = 90f;
        [SerializeField] protected float _minDistanceToDetect = 3f;
        [SerializeField] protected float _maxDistanceToDetect = 6f;
        [SerializeField] [vReadOnly] protected bool _targetIsLost;
        [SerializeField] [vReadOnly] protected bool _targetInLineOfSight;
        [vHelpBox("Considerer maxDistanceToDetect value + lostTargetDistance", vHelpBoxAttribute.MessageType.None)]
        [SerializeField] protected float _lostTargetDistance = 4f;
        [SerializeField] protected float _timeToLostWithoutSight = 5f;

        [Header("--- Layers to Detect ----")]
        [SerializeField] protected LayerMask _detectLayer;
        [SerializeField] protected vTagMask _detectTags;
        [SerializeField] protected LayerMask _obstacles = 1 << 0;

        [Header("--- Debug Options ---")]
        [SerializeField] protected bool _debugVisualDetection;
        [SerializeField] protected bool _debugRaySight;
        [SerializeField] protected bool _debugLastTargetPosition;
        [SerializeField] protected vAITarget _currentTarget;

        internal vAIHeadtrack _headtrack;

        protected Vector3 _lastTargetPosition;
        protected int _currentWaypoint;

        private vDamage _receivedDamage;
        private float lostTargetTime;
        private Vector3 lastValidDestination;
        private UnityEngine.AI.NavMeshHit navHit;
        private float changeTargetTime;

        public virtual void CreatePrimaryComponents()
        {
            if (GetComponent<Rigidbody>() == null)
            {
                gameObject.AddComponent<Rigidbody>();
                var rigidbody = GetComponent<Rigidbody>();
                rigidbody.mass = 50f;
                rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            }

            if (GetComponent<CapsuleCollider>() == null)
            {
                var capsuler = gameObject.AddComponent<CapsuleCollider>();
                animator = GetComponent<Animator>();
                if (animator)
                {
                    var foot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                    var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                    var height = (float)System.Math.Round(Vector3.Distance(foot.position, hips.position) * 2f, 2);
                    capsuler.height = height;
                    capsuler.center = new Vector3(0, (float)System.Math.Round(capsuler.height * 0.5f, 2), 0);
                    capsuler.radius = (float)System.Math.Round(capsuler.height * 0.15f, 2);
                }
            }
            if (GetComponent<UnityEngine.AI.NavMeshAgent>() == null) gameObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
        }

        public virtual void CreateSecundaryComponents()
        {

        }

        protected bool isWaypointStarted;
        #endregion

        #region NavMeshAgent Variables

        protected Vector3 _destination;
        protected Vector3 lasDestination;
        protected Vector3 temporaryDirection;
        [HideInInspector] public UnityEngine.AI.NavMeshAgent navMeshAgent;
        protected UnityEngine.AI.NavMeshHit navMeshHit;
        protected float updatePathTime;
        protected float updateFindTargetTime;
        protected float canseeTargetUpdateTime;
        protected float temporaryDirectionTime;
        protected float timeToResetOutDistance;
        protected float forceUpdatePathTime;
        protected bool isOutOfDistance;
        private int findAgentDestinationRadius;
        #endregion

        #region OVERRIDE METHODS. AI

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            if (_debugLastTargetPosition)
            {
                if (currentTarget.transform && _targetIsLost)
                {
                    var color = _targetInLineOfSight ? Color.green : Color.red;
                    color.a = 0.2f;
                    Gizmos.color = color;
                    Gizmos.DrawLine(transform.position + Vector3.up * 1.5f, lastTargetPosition + Vector3.up * 1.5f);
                    color.a = 1;
                    Gizmos.color = color;
                    Gizmos.DrawLine(lastTargetPosition, lastTargetPosition + Vector3.up * 1.5f);
                    var forward = (lastTargetPosition - transform.position).normalized;
                    forward.y = 0;
                    var right = Quaternion.AngleAxis(90, Vector3.up) * forward;
                    var p1 = lastTargetPosition + Vector3.up * 1.5f - forward;
                    var p2 = lastTargetPosition + Vector3.up * 1.5f + forward * 0.5f + right * 0.25f;
                    var p3 = lastTargetPosition + Vector3.up * 1.5f + forward * 0.5f - right * 0.25f;
                    Gizmos.DrawLine(p1, p2);
                    Gizmos.DrawLine(p1, p3);
                    Gizmos.DrawLine(p3, p2);
                    Gizmos.DrawSphere(lastTargetPosition + Vector3.up * 1.5f, 0.1f);
                }
            }
        }

        protected override void Start()
        {
            changeWaypointDistance = _changeWaypointDistance;
            selfStartPosition = (!_selfStartingPoint && _customStartingPoint) ? _customStartingPoint.position : transform.position;
            _destination = transform.position;
            lasDestination = _destination;
            navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (!navMeshAgent) return;
            navMeshAgent.updatePosition = false;
            navMeshAgent.updateRotation = false;
            if (isOnNavMesh) navMeshAgent.enabled = true;
            RotateTo(transform.forward);
            animatorStateInfos = new vEventSystems.vAnimatorStateInfos(GetComponent<Animator>());
            ClearDamage();
            if (currentTarget != null) currentTarget.InitTarget(currentTarget.transform);
            _headtrack = GetComponent<vAIHeadtrack>();
            base.Start();
            aiComponents = new Dictionary<System.Type, vIAIComponent>();
            var _aiComponents = GetComponents<vIAIComponent>();
            for (int i = 0; i < _aiComponents.Length; i++)
            {
                if (!aiComponents.ContainsKey(_aiComponents[i].ComponentType))
                {
                    aiComponents.Add(_aiComponents[i].ComponentType, _aiComponents[i]);
                }
            }
        }

        protected override void UpdateAI()
        {
            base.UpdateAI();
            CalcMovementDirection();
            UpdateMovementSpeed();
            HandleTarget();
        }

        public override void ResetRagdoll()
        {
            base.ResetRagdoll();
            if (_headtrack) _headtrack.canLook = true;
        }

        public override void EnableRagdoll()
        {
            base.EnableRagdoll();
            if (_headtrack) _headtrack.canLook = false;
        }

        protected override void RemoveComponents()
        {
            base.RemoveComponents();
            Destroy(navMeshAgent);
        }

        protected override void OnAnimatorMove()
        {
            if (useNavMeshAgent && navMeshAgent && navMeshAgent.enabled)
            {
                navMeshAgent.velocity = ((animator.deltaPosition) / Time.deltaTime) * Mathf.Clamp(remainingDistanceWithoutAgent - stopingDistance, 0, 1f);
                //navMeshAgent.speed = Mathf.Clamp((float)System.Math.Round((double)(animator.deltaPosition / Time.deltaTime).magnitude , 2), 0.5f, maxSpeed);              
                navMeshAgent.speed = Mathf.Lerp(navMeshAgent.speed, maxSpeed, aceleration * Time.deltaTime);
                navMeshAgent.nextPosition = animator.rootPosition;
            }

            base.OnAnimatorMove();
        }

        public override void Stop()
        {
            base.Stop();
            if (useNavMeshAgent && navMeshAgent && navMeshAgent.isOnNavMesh && !navMeshAgent.isStopped)
            {
                _turnOnSpotDirection = transform.forward;
                temporaryDirection = transform.forward;
                navMeshAgent.isStopped = true;
                this.destination = transform.position;
                navMeshAgent.ResetPath();
            }
        }

        public override void DisableAIController()
        {
            if (disableAgentOnStart && navMeshAgent)
                navMeshAgent.enabled = false;
            base.DisableAIController();
        }

        #endregion

        #region METHODS. AIAgent/Interfaces

        #region Protected methods

        protected virtual Dictionary<System.Type, vIAIComponent> aiComponents { get; set; }

        protected virtual vWaypoint GetWaypoint()
        {
            if (waypointArea == null) return null;
            var waypoints = waypointArea.GetValidPoints();

            if (_randomWaypoint)
                _currentWaypoint = Random.Range(0, waypoints.Count);
            else if (isWaypointStarted)
                _currentWaypoint++;
            else _currentWaypoint = 0;

            if (!isWaypointStarted)
            {
                isWaypointStarted = true;
                visitedWaypoints = new List<vWaypoint>();
            }

            if (_currentWaypoint >= waypoints.Count)
                _currentWaypoint = 0;

            if (waypoints.Count == 0)
                return null;

            if (visitedWaypoints.Count == waypoints.Count)
                visitedWaypoints.Clear();

            if (visitedWaypoints.Contains(waypoints[_currentWaypoint]))
                return null;

            return waypoints[_currentWaypoint];
        }

        protected float GetUpdateTimeFromQuality(vAIUpdateQuality quality)
        {
            return quality == vAIUpdateQuality.VeryLow ? 2 : quality == vAIUpdateQuality.Low ? 1f : quality == vAIUpdateQuality.Medium ? 0.75f : quality == vAIUpdateQuality.High ? .25f : 0.1f;
        }

        protected virtual Vector3 destination
        {
            get
            {
                return _destination;
            }
            set
            {
                _destination = value;
            }
        }

        protected virtual void UpdateAgentPath()
        {
            updatePathTime -= Time.deltaTime;
            if (updatePathTime > 0 && forceUpdatePathTime <= 0f && navMeshAgent.hasPath) return;
            forceUpdatePathTime -= Time.deltaTime;
            updatePathTime = GetUpdateTimeFromQuality(updatePathQuality);

            if (!isDead && !isJumping && isGrounded)
            {
                var destin = _destination;

                if ((movementSpeed != vAIMovementSpeed.Idle && destin != lasDestination) || !navMeshAgent.hasPath)
                {
                    if (navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
                    {
                        if (UnityEngine.AI.NavMesh.SamplePosition(destin, out navHit, _capsuleCollider.radius + findAgentDestinationRadius, navMeshAgent.areaMask) && (navHit.position - navMeshAgent.destination).magnitude > stopingDistance)
                        {
                            navMeshAgent.destination = (navHit.position);
                            lasDestination = destin;
                        }
                        else if ((navHit.position - navMeshAgent.destination).magnitude > stopingDistance)
                        {
                            findAgentDestinationRadius++;
                            if (findAgentDestinationRadius >= 10)
                            {
                                findAgentDestinationRadius = 0;
                            }
                        }
                    }
                }
            }
        }

        protected virtual void CalcMovementDirection()
        {
            if (isDead || isJumping) return;

            if (useNavMeshAgent && navMeshAgent)
            {
                ControlNavMeshAgent();
                UpdateAgentPath();
            }

            bool forceMovement = !navMeshAgent.hasPath && remainingDistanceWithoutAgent > navMeshAgent.stoppingDistance + _capsuleCollider.radius;
            var dir = !forceMovement && navMeshAgent != null && navMeshAgent.enabled && useNavMeshAgent ? navMeshAgent.desiredVelocity * (!isInDestination ? 1 : 0) :
                ((new Vector3(destination.x, transform.position.y, destination.z) - transform.position).normalized * Mathf.Clamp(remainingDistanceWithoutAgent - stopingDistance, 0, 1f));
            //Convert Direction to Input
            var movementInput = transform.InverseTransformDirection(dir);

            if (useNavMeshAgent && navMeshAgent.enabled)
            {
                var data = navMeshAgent.currentOffMeshLinkData;
                if (navMeshAgent.isOnOffMeshLink)
                {
                    dir = (data.endPos - transform.position);
                    movementInput = transform.InverseTransformDirection(dir);
                }
            }

            if (movementInput.magnitude > 0.1f)
            {
                if (temporaryDirectionTime <= 0 || isStrafing == false)
                    SetMovementInput(movementInput, aceleration);
                else
                    SetMovementInput(movementInput, temporaryDirection, aceleration);
            }
            else if (temporaryDirectionTime > 0 && temporaryDirection.magnitude >= 0.1f && movementInput.magnitude < 0.2f)
            {
                TurnOnSpot(temporaryDirection);
            }
            else input = Vector3.zero;
            if (!isGrounded || isJumping || isRolling) navMeshAgent.enabled = false;
            temporaryDirectionTime -= Time.deltaTime;
        }

        protected virtual void CheckAgentDistanceFromAI()
        {
            if (!useNavMeshAgent || !navMeshAgent || !navMeshAgent.enabled) return;
            if (Vector3.Distance(transform.position, navMeshAgent.nextPosition) > stopingDistance * 1.5f && !isOutOfDistance)
            {
                timeToResetOutDistance = 3f;
                isOutOfDistance = true;
            }
            if (isOutOfDistance)
            {
                timeToResetOutDistance -= Time.deltaTime;
                if (timeToResetOutDistance <= 0)
                {
                    isOutOfDistance = false;
                    if (Vector3.Distance(transform.position, navMeshAgent.nextPosition) > stopingDistance)
                        navMeshAgent.enabled = false;
                }
            }
        }

        protected virtual void ControlNavMeshAgent()
        {
            if (isDead) return;
            if (useNavMeshAgent && navMeshAgent)
                navMeshAgent.stoppingDistance = stopingDistance;
            if (Time.deltaTime == 0 || navMeshAgent.enabled == false)
            {
                if (!ragdolled && !isJumping && isGrounded && !navMeshAgent.enabled && isOnNavMesh)
                {
                    navMeshAgent.enabled = true;
                }
            }

            if (navMeshAgent.enabled && isOnJumpLink && !isJumping && isGrounded)
            {
                var jumpTarget = navMeshAgent.currentOffMeshLinkData.endPos;
                JumpTo(jumpTarget);
            }

            if (isJumping || !isGrounded || ragdolled)
                navMeshAgent.enabled = false;
            CheckAgentDistanceFromAI();
        }

        protected virtual void UpdateMovementSpeed()
        {
            switch (movementSpeed)
            {
                case vAIMovementSpeed.Idle:
                    Stop();
                    break;
                case vAIMovementSpeed.Walking:
                    Walk();
                    break;
                case vAIMovementSpeed.Running:
                    Run();
                    break;
                case vAIMovementSpeed.Sprinting:
                    Sprint();
                    break;
            }
        }

        protected virtual bool CheckCanSeeTarget()
        {
            if (currentTarget != null && currentTarget.transform != null && currentTarget.collider == null && InFOVAngle(currentTarget.transform.position, _fieldOfView))
            {
                if (sightMethod == 0) return true;
                var eyesPoint = detectionPointReference ? detectionPointReference.position : transform.position + Vector3.up * (selfCollider.bounds.size.y * 0.8f);
                if (!Physics.Linecast(eyesPoint, currentTarget.transform.position, _obstacles))
                {
                    if (_debugRaySight)
                        Debug.DrawLine(eyesPoint, currentTarget.transform.position, Color.green, GetUpdateTimeFromQuality(canseeTargetUpdateQuality));
                    return true;
                }
                else
                {
                    if (_debugRaySight)
                        Debug.DrawLine(eyesPoint, currentTarget.transform.position, Color.red, GetUpdateTimeFromQuality(canseeTargetUpdateQuality));
                }
            }
            else if (currentTarget.collider) return CheckCanSeeTarget(currentTarget.collider);

            return false;
        }

        protected virtual bool CheckCanSeeTarget(Collider target)
        {
            if (target != null && InFOVAngle(target.bounds.center, _fieldOfView))
            {
                if (sightMethod == 0) return true;
                var detectionPoint = detectionPointReference ? detectionPointReference.position : transform.position + Vector3.up * (selfCollider.bounds.size.y * 0.8f);
                if (sightMethod.Contains<vAISightMethod>(vAISightMethod.Center))
                    if (!Physics.Linecast(detectionPoint, target.bounds.center, _obstacles))
                    {
                        if (_debugRaySight) Debug.DrawLine(detectionPoint, target.bounds.center, Color.green, GetUpdateTimeFromQuality(canseeTargetUpdateQuality));
                        return true;
                    }
                    else
                    {
                        if (_debugRaySight) Debug.DrawLine(detectionPoint, target.bounds.center, Color.red, GetUpdateTimeFromQuality(canseeTargetUpdateQuality));
                    }
                if (sightMethod.Contains<vAISightMethod>(vAISightMethod.Top))
                    if (!Physics.Linecast(detectionPoint, target.transform.position + Vector3.up * target.bounds.size.y * 0.9f, _obstacles))
                    {
                        if (_debugRaySight) Debug.DrawLine(detectionPoint, target.transform.position + Vector3.up * target.bounds.size.y * 0.9f, Color.green, GetUpdateTimeFromQuality(canseeTargetUpdateQuality));
                        return true;
                    }
                    else
                    {
                        if (_debugRaySight) Debug.DrawLine(detectionPoint, target.transform.position + Vector3.up * target.bounds.size.y * 0.9f, Color.red, GetUpdateTimeFromQuality(canseeTargetUpdateQuality));
                    }
                if (sightMethod.Contains<vAISightMethod>(vAISightMethod.Bottom))
                    if (!Physics.Linecast(detectionPoint, target.transform.position + Vector3.up * target.bounds.size.y * 0.1f, _obstacles))
                    {
                        if (_debugRaySight) Debug.DrawLine(detectionPoint, target.transform.position + Vector3.up * target.bounds.size.y * 0.1f, Color.green, GetUpdateTimeFromQuality(canseeTargetUpdateQuality));
                        return true;
                    }
                    else
                    {
                        if (_debugRaySight) Debug.DrawLine(detectionPoint, target.transform.position + Vector3.up * target.bounds.size.y * 0.1f, Color.red, GetUpdateTimeFromQuality(canseeTargetUpdateQuality));
                    }
            }
            return false;
        }

        protected virtual bool InFOVAngle(Vector3 viewPoint, float fieldOfView)
        {
            if (Vector3.Distance(transform.position, viewPoint) < _minDistanceToDetect) return true;
            if (Vector3.Distance(transform.position, viewPoint) > _maxDistanceToDetect) return false;
            var rot = Quaternion.LookRotation(viewPoint - transform.position, Vector3.up);
            var detectionAngle = detectionPointReference ? detectionPointReference.eulerAngles : transform.eulerAngles;
            var newAngle = rot.eulerAngles - detectionAngle;
            var fovAngleY = newAngle.NormalizeAngle().y;
            var fovAngleX = newAngle.NormalizeAngle().x;
            if (fovAngleY <= (fieldOfView * 0.5f) && fovAngleY >= -(fieldOfView * 0.5f) && fovAngleX <= (fieldOfView * 0.5f) && fovAngleX >= -(fieldOfView * 0.5f))
                return true;

            return false;
        }

        protected virtual void HandleTarget()
        {
            if (_targetIsLost && currentTarget.transform) lastTargetPosition = currentTarget.transform.position;
            canseeTargetUpdateTime -= Time.deltaTime;
            if (canseeTargetUpdateTime > 0) return;
            if (currentTarget != null && currentTarget.transform)
            {
                _targetInLineOfSight = CheckCanSeeTarget();
                if (!_targetInLineOfSight || targetDistance >= (_maxDistanceToDetect + _lostTargetDistance))
                {
                    if (lostTargetTime < Time.time)
                    {
                        _targetIsLost = false;
                        lostTargetTime = Time.time + _timeToLostWithoutSight;
                    }
                }
                else
                {
                    lostTargetTime = Time.time + _timeToLostWithoutSight;
                    _targetIsLost = true;
                    currentTarget.isLost = false;
                }
            }
            else
            {
                _targetInLineOfSight = false;
                _targetIsLost = false;
            }
            HandleLostTarget();
            canseeTargetUpdateTime = GetUpdateTimeFromQuality(canseeTargetUpdateQuality);
        }

        protected virtual void HandleLostTarget()
        {
            if (currentTarget != null && currentTarget.transform != null)
            {
                if (currentTarget.hasHealthController && (currentTarget.isDead || targetDistance > (_maxDistanceToDetect + _lostTargetDistance) || !targetInLineOfSight))
                {
                    if (currentTarget.isFixedTarget)
                        currentTarget.isLost = true;
                    else
                        currentTarget.ClearTarget();
                }
                else if (!currentTarget.hasHealthController && (currentTarget.transform == null || !currentTarget.transform.gameObject.activeSelf || targetDistance > (_maxDistanceToDetect + _lostTargetDistance) || !_targetIsLost))
                {
                    if (currentTarget.isFixedTarget)
                        currentTarget.isLost = true;
                    else
                        currentTarget.ClearTarget();
                }
            }
        }

        protected static bool IsInLayerMask(int layer, LayerMask layermask)
        {
            return layermask == (layermask | (1 << layer));
        }

        #endregion

        #region Public methods 

        public virtual void SetDetectionLayer(LayerMask mask)
        {
            _detectLayer = mask;
        }

        public virtual void SetDetectionTags(List<string> tags)
        {
            _detectTags = tags;
        }

        public virtual void SetObstaclesLayer(LayerMask mask)
        {
            _obstacles = mask;
        }

        public virtual void SetLineOfSight(float fov = -1, float minDistToDetect = -1, float maxDistToDetect = -1, float lostTargetDistance = -1)
        {
            if (fov != -1) _fieldOfView = fov;
            if (minDistToDetect != -1) _minDistanceToDetect = minDistToDetect;
            if (maxDistToDetect != -1) _maxDistanceToDetect = maxDistToDetect;
            if (lostTargetDistance != -1) _lostTargetDistance = lostTargetDistance;
        }

        public virtual vDamage receivedDamage { get { return _receivedDamage; } protected set { _receivedDamage = value; } }

        public virtual bool targetInLineOfSight { get { return _targetInLineOfSight; } }

        public virtual vAITarget currentTarget { get { return _currentTarget; } protected set { _currentTarget = value; } }

        public virtual Vector3 lastTargetPosition
        {
            get
            {
                return _lastTargetPosition;
            }
            protected set
            {
                _lastTargetPosition = value;
            }
        }

        public virtual float targetDistance
        {
            get
            {
                if (currentTarget == null || currentTarget.isDead) return Mathf.Infinity;
                return Vector3.Distance(currentTarget.transform.position, transform.position);
            }
        }

        public virtual void FindTarget()
        {
            if (updateFindTargetTime > Time.time) return;
            updateFindTargetTime = Time.time + GetUpdateTimeFromQuality(findTargetUpdateQuality);
            if (!findOtherTarget && currentTarget.transform) return;
            if (currentTarget.transform && currentTarget.isFixedTarget && !findOtherTarget) return;
            var targets = Physics.OverlapSphere(transform.position + transform.up, _maxDistanceToDetect, _detectLayer);
            Transform target = currentTarget != null && _targetIsLost ? currentTarget.transform : null;
            var _targetDistance = target && targetInLineOfSight ? targetDistance : Mathf.Infinity;

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] != null && targets[i].transform != transform && _detectTags.Contains(targets[i].gameObject.tag) && CheckCanSeeTarget(targets[i]))
                {
                    //Debug.Log(targets[i].name);
                    if (findTargetByDistance)
                    {
                        var newTargetDistance = Vector3.Distance(targets[i].transform.position, transform.position);
                        var character = targets[i].GetComponent<vIHealthController>();
                        if (character != null && !character.isDead && newTargetDistance < _targetDistance)
                        {
                            target = targets[i].transform;
                            _targetDistance = newTargetDistance;
                        }
                    }
                    else
                    {
                        var character = targets[i].GetComponent<vIHealthController>();
                        if (character != null && !character.isDead)
                        {
                            target = targets[i].transform;
                            break;
                        }
                    }
                }
            }

            if (currentTarget == null || target != null && target != currentTarget.transform)
            {
                if (target != null)
                    SetCurrentTarget(target);
            }
        }

        public virtual void SetCurrentTarget(Transform target, bool overrideCanseTarget = true)
        {
            if (changeTargetTime < Time.time)
            {
                changeTargetTime = _changeTargetDelay + Time.time;
                currentTarget.InitTarget(target);
                if (overrideCanseTarget)
                {
                    currentTarget.isLost = false;
                    _targetInLineOfSight = true;
                    _targetIsLost = false;
                }
                updateFindTargetTime = 0f;
                updatePathTime = 0f;
                lastTargetPosition = target.position;
                LookToTarget(target, 2);
            }
        }

        public virtual void RemoveCurrentTarget()
        {
            currentTarget.ClearTarget();
        }

        public virtual void LookAround()
        {
            if (_headtrack) _headtrack.LookAround();
        }

        public virtual void LookTo(Vector3 point, float stayLookTime = 1)
        {
            if (_headtrack) _headtrack.LookAtPoint(point, stayLookTime);
        }

        public virtual void LookToTarget(Transform target, float stayLookTime = 1)
        {
            if (_headtrack) _headtrack.LookAtTarget(target, stayLookTime);
        }

        public virtual void SetSpeed(vAIMovementSpeed movementSpeed)
        {
            if (this.movementSpeed != movementSpeed)
            {
                if (movementSpeed == vAIMovementSpeed.Idle)
                {
                    Stop();
                }
                this.movementSpeed = movementSpeed;
            }
        }

        public virtual bool isInDestination
        {
            get
            {
                if (useNavMeshAgent && (remainingDistance <= stopingDistance || navMeshAgent.hasPath && remainingDistance > stopingDistance && navMeshAgent.desiredVelocity.magnitude < 0.1f)) return true;
                return remainingDistance <= stopingDistance;
            }
        }

        public virtual bool isMoving
        {
            get
            {
                return input.sqrMagnitude > 0.1f;
            }
        }

        public virtual float remainingDistance
        {
            get
            {
                return navMeshAgent && navMeshAgent.enabled && useNavMeshAgent && isOnNavMesh ? navMeshAgent.remainingDistance : remainingDistanceWithoutAgent;
            }
        }

        protected virtual float remainingDistanceWithoutAgent
        {
            get
            {
                return Vector3.Distance(transform.position, new Vector3(destination.x, transform.position.y, destination.z));
            }
        }

        public virtual Collider selfCollider
        {
            get { return _capsuleCollider; }
        }

        public virtual bool isOnJumpLink
        {
            get
            {
                if (!useNavMeshAgent) return false;
                if (navMeshAgent.isOnOffMeshLink && navMeshAgent.currentOffMeshLinkData.linkType == UnityEngine.AI.OffMeshLinkType.LinkTypeJumpAcross) return true;
                var linkData = navMeshAgent.currentOffMeshLinkData.offMeshLink;
                if (linkData != null)
                {
                    if (linkData.area == UnityEngine.AI.NavMesh.GetAreaFromName("Jump"))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public virtual bool isOnNavMesh
        {
            get
            {
                if (!useNavMeshAgent) return false;
                if (navMeshAgent.enabled) return navMeshAgent.isOnNavMesh;

                if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out navMeshHit, _capsuleCollider.radius, navMeshAgent.areaMask))
                {
                    return true;
                }
                return false;
            }
        }

        public virtual void MoveTo(Vector3 newDestination)
        {
            // if (useNavMeshAgent && navMeshAgent && navMeshAgent.isOnNavMesh && navMeshAgent.isStopped) navMeshAgent.isStopped = false;
            if (isStrafing) updatePathTime = 0;
            var dir = newDestination - transform.position;
            dir.y = 0;

            temporaryDirection = transform.forward;
            temporaryDirectionTime = 0;

            this.destination = newDestination;
            SetFreeLocomotion();
        }

        public virtual void StrafeMoveTo(Vector3 newDestination, Vector3 targetDirection)
        {
            if (useNavMeshAgent && navMeshAgent && navMeshAgent.isOnNavMesh && navMeshAgent.isStopped) navMeshAgent.isStopped = false;
            SetStrafeLocomotion();
            destination = newDestination;
            temporaryDirection = targetDirection;
            temporaryDirectionTime = 1f;
        }

        public virtual void RotateTo(Vector3 targetDirection)
        {
            targetDirection.y = 0;
            if (Vector3.Angle(transform.forward, targetDirection) > 20)
            {
                temporaryDirection = targetDirection;
                temporaryDirectionTime = 1f;
            }

        }

        public virtual Vector3 targetDestination
        {
            get { return _destination; }
        }

        public virtual float stopingDistance { get { return stopingDistanceRelativeToSpeed + _stopingDistance; } set { _stopingDistance = value; } }

        protected virtual float stopingDistanceRelativeToSpeed
        {
            get
            {
                return movementSpeed == vAIMovementSpeed.Idle ? 1 : movementSpeed == vAIMovementSpeed.Running ? _runningStopingDistance : movementSpeed == vAIMovementSpeed.Sprinting ? _sprintingStopingDistance : _walkingStopingDistance;
            }
        }

        public virtual Vector3 selfStartPosition { get; set; }

        public virtual vWaypointArea waypointArea
        {
            get
            {
                return _waypointArea;
            }
            set
            {
                if (value != null && value != _waypointArea)
                {
                    var waypoints = value.GetValidPoints();
                    if (_randomStartingPoint)
                        _currentWaypoint = Random.Range(0, waypoints.Count);
                }
                _waypointArea = value;
            }
        }

        public virtual vWaypoint targetWaypoint { get; protected set; }

        public virtual List<vWaypoint> visitedWaypoints { get; set; }

        public virtual bool selfStartingPoint { get { return _selfStartingPoint; } protected set { _selfStartingPoint = value; } }

        public virtual float changeWaypointDistance { get; protected set; }

        public bool customStartPoint
        {
            get
            {
                return !selfStartingPoint && _customStartingPoint != null;
            }
        }

        public Vector3 customStartPosition
        {
            get
            {
                return customStartPoint ? _customStartingPoint.position : transform.position;
            }
        }

        public virtual void NextWayPoint()
        {
            targetWaypoint = GetWaypoint();
        }

        public virtual void ClearDamage()
        {
            receivedDamage = null;
        }

        public override void TakeDamage(vDamage damage)
        {
            base.TakeDamage(damage);

            if (damage.damageValue > 0)
            {
                //Check condition to add a new target
                if (!currentTarget.transform || (currentTarget.transform && !currentTarget.isFixedTarget || (currentTarget.isFixedTarget && findOtherTarget)))
                {
                    //Check if new target is in detections settings
                    if (damage.sender && IsInLayerMask(damage.sender.gameObject.layer, _detectLayer) && _detectTags.Contains(damage.sender.gameObject.tag))
                    {
                        SetCurrentTarget(damage.sender, false);
                    }
                }
                receivedDamage = new vDamage(damage);
                //Clear damage with delay
                CancelInvoke("ClearDamage");
                Invoke("ClearDamage", 2);
                updatePathTime = 0f;
            }
        }

        public void ForceUpdatePath(float timeInUpdate = 1f)
        {
            forceUpdatePathTime = timeInUpdate;
        }

        public bool HasComponent<T>() where T : vIAIComponent
        {
            return aiComponents.ContainsKey(typeof(T));
        }

        public T GetAIComponent<T>() where T : vIAIComponent
        {
            return aiComponents.ContainsKey(typeof(T)) ? (T)aiComponents[typeof(T)] : default(T);
        }

        #endregion

        #endregion

    }
}