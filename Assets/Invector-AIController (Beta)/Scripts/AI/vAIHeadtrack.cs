using UnityEngine;
using System.Collections.Generic;

namespace Invector
{
    [vClassHeader("AI HEADTRACK", helpBoxText = "If the bone hips don't have the same orientation of the character,\n you can add a custom hips to override the original (Transforms)", useHelpBox = true)]
    public class vAIHeadtrack : vMonoBehaviour
    {
        #region Public Variables
        [vEditorToolbar("Settings")]
        public bool canLook = true;
        public float strafeHeadWeight = 0.8f;
        public float strafeBodyWeight = 0.8f;
        public float freeHeadWeight = 1f;
        public float freeBodyWeight = 0.4f;
        public float minAngleX = -90f, maxAngleX = 90, minAngleY = -90f, maxAngleY = 90f;
        public bool followCamera = false;
        public bool keepLookingOutAngle = true;
        public float offSetLookHeight = 1.5f;
        public float smooth = 12f;
        public List<string> animatorTags = new List<string>() { "Attack", "LockMovement", "CustomAction" };
        public Vector2 offsetSpine;
        public Transform mainLookTarget;
        public Transform eyes;
        public float timeToExitLookPoint = 1;
        public float timeToExitLookTarget = 1;
        public bool autoCancelUpdateIK = true;
        public Vector3 currentLookPoint;
        public Vector3 currentLookDirection;
        public bool useLookAround = false;
        [vHideInInspector("useLookAround")]
        [SerializeField] protected float lookAroundAngle = 60f;
        [vHideInInspector("useLookAround")]
        [SerializeField] protected AnimationCurve lookAroundCurve;
        [vHideInInspector("useLookAround")]
        [SerializeField] protected float lookAroundSpeed = 0.1f;

        [vEditorToolbar("Transforms")]
        [Tooltip("If the bone hips don't have the same orientation of the character, you can add a custom hips to override the original (Transforms)")]
        public Transform hips;
        [Header("Just for Debug")]
        public Transform head;
        public List<Transform> spine;      

        #endregion

        #region Private Variables
        private Animator animator;

        private List<int> tagsHash;
        private Transform temporaryLookTarget;

        private Vector3 temporaryLookPoint;
        private Vector3 targetLookPoint;
        private bool inLockPoint;
        private bool inLockTarget;
        private bool isInSmoothValues;
        private bool updateIK;
        private float currentOffsetLookHeight;
        private float exitLookPointTime;
        private float exitLookTargetTime;
        private float headHeight;
        private float yAngle, xAngle;
        private float _yAngle, _xAngle;
        private float yRotation, xRotation;
        private float _currentHeadWeight, _currentbodyWeight;
        private float lookAroundProgress;
        #endregion

        [vEditorToolbar("Events")]
        public UnityEngine.Events.UnityEvent onPreUpdateSpineIK, onPosUpdateSpineIK;

        #region PROTECTED VIRTUAL METHODS

        #region UNITY METHODS

        protected virtual void Start()
        {            
            animator = GetComponentInParent<Animator>();
            if(animator.isHuman)
            {
                head = animator.GetBoneTransform(HumanBodyBones.Head);
                var spine1 = animator.GetBoneTransform(HumanBodyBones.Spine);
                var spine2 = animator.GetBoneTransform(HumanBodyBones.Chest);
                spine = new List<Transform>();
                if (spine1)
                    spine.Add(spine1);
                if (spine2)
                    spine.Add(spine2);
                var neck = animator.GetBoneTransform(HumanBodyBones.Neck);
                if (!hips)
                    hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                if (neck && spine2 && neck.parent && neck.parent != spine2)
                    spine.Add(neck.parent);
            }            

            if (head)
                headHeight = Vector3.Distance(transform.position, head.position);

            tagsHash = new List<int>();

            for (int i = 0; i < animatorTags.Count; i++)
                tagsHash.Add(Animator.StringToHash(animatorTags[i]));
            ResetOffseLookHeight();
            GetLookPoint();
            lookAroundProgress = 0.5f;
        }

        protected virtual void FixedUpdate()
        {
            updateIK = true;
        }

        protected virtual void LateUpdate()
        {
            if (animator == null || animator.GetBool("isDead")) return;
            if ((!updateIK && animator.updateMode == AnimatorUpdateMode.AnimatePhysics)) return;
            updateIK = false;
            // call pre Update Event
            if (onPreUpdateSpineIK != null) onPreUpdateSpineIK.Invoke();
            // update SpineIK
            LookAtIK(GetLookPoint(), _currentHeadWeight, _currentbodyWeight);
            // call pos Update Event
            if (onPosUpdateSpineIK != null && !IgnoreHeadTrackFromAnimator()) onPosUpdateSpineIK.Invoke();
        }

        #endregion

        #region SPINE IK BEHAVIOUR

        protected virtual void LookAtIK(Vector3 point, float strafeHeadWeight, float spineWeight)
        {
            if (isLookingForSomething || !autoCancelUpdateIK)
            {
                var lookRotation = Quaternion.LookRotation(point);
                var euler = lookRotation.eulerAngles - transform.rotation.eulerAngles;

                var y = NormalizeAngle(euler.y);
                var x = NormalizeAngle(euler.x);

                xAngle = Mathf.Clamp(Mathf.Lerp(xAngle, (x), smooth * Time.deltaTime), minAngleX, maxAngleX);
                yAngle = Mathf.Clamp(Mathf.Lerp(yAngle, (y), smooth * Time.deltaTime), minAngleY, maxAngleY);

                foreach (Transform segment in spine)
                {
                    var _y = NormalizeAngle(yAngle + Quaternion.Euler(offsetSpine).eulerAngles.y);
                    var _x = NormalizeAngle(xAngle + Quaternion.Euler(offsetSpine).eulerAngles.x);

                    var rotX = Quaternion.AngleAxis((_x * spineWeight) / spine.Count, segment.InverseTransformDirection(transform.right));
                    var rotY = Quaternion.AngleAxis((_y * spineWeight) / spine.Count, segment.InverseTransformDirection(transform.up));
                    segment.rotation *= rotX * rotY;
                }
                xAngle = NormalizeAngle(xAngle);// + Quaternion.Euler(offsetSpine).eulerAngles.x;
                yAngle = NormalizeAngle(yAngle);// + Quaternion.Euler(offsetSpine).eulerAngles.y;
                _yAngle = Mathf.Lerp(_yAngle, (yAngle - (yAngle * spineWeight)) * strafeHeadWeight, smooth * Time.deltaTime);
                _xAngle = Mathf.Lerp(_xAngle, (xAngle - (xAngle * spineWeight)) * strafeHeadWeight, smooth * Time.deltaTime);
                var _rotX = Quaternion.AngleAxis(_xAngle, head.InverseTransformDirection(transform.right));
                var _rotY = Quaternion.AngleAxis(_yAngle, head.InverseTransformDirection(transform.up));
                head.rotation *= _rotX * _rotY;
            }
        }

        protected virtual void SmoothValues(float _headWeight = 0, float _bodyWeight = 0, float _x = 0, float _y = 0)
        {
            _currentHeadWeight = Mathf.Lerp(_currentHeadWeight, _headWeight, smooth * Time.deltaTime);
            _currentbodyWeight = Mathf.Lerp(_currentbodyWeight, _bodyWeight, smooth * Time.deltaTime);
            yRotation = Mathf.Lerp(yRotation, _y, smooth * Time.deltaTime);
            xRotation = Mathf.Lerp(xRotation, _x, smooth * Time.deltaTime);
            yRotation = Mathf.Clamp(yRotation, minAngleY, maxAngleY);
            xRotation = Mathf.Clamp(xRotation, minAngleX, maxAngleX);

            var completeY = Mathf.Abs(yRotation - Mathf.Clamp(_y, minAngleY, maxAngleY)) < 0.01f;
            var completeX = Mathf.Abs(yRotation - Mathf.Clamp(_x, minAngleY, maxAngleY)) < 0.01f;
            isInSmoothValues = !(completeY && completeX);
        }

        protected virtual Vector3 headPoint { get { return transform.position + (transform.up * headHeight); } }

        protected virtual Vector3 GetLookPoint()
        {
            if (!IgnoreHeadTrackFromAnimator() && canLook)
            {
                // default look Point
                var _defaultLookPoint = mainLookTarget ? mainLookTarget.position + Vector3.up * currentOffsetLookHeight : defaultLookPoint;
                targetLookPoint = _defaultLookPoint;
                // temporary look Target
                if (exitLookTargetTime > 0 || inLockTarget)
                {
                    if (temporaryLookTarget) targetLookPoint = temporaryLookTarget.position + Vector3.up * currentOffsetLookHeight;
                    else exitLookTargetTime = 0;
                    if (!inLockTarget) exitLookTargetTime -= Time.deltaTime;
                }
                // temporary look point
                if (exitLookPointTime > 0 || inLockPoint)
                {
                    targetLookPoint = temporaryLookPoint;
                    if (!inLockPoint) exitLookPointTime -= Time.deltaTime;
                }
                // calc look direction         
                var currentDir = defaultLookPoint - headPoint;
                var desiredDir = targetLookPoint - headPoint;

                currentDir = desiredDir;
                // apply limit angles
                var angle = GetTargetAngle(currentDir);

                //check if is out angle
                if (!keepLookingOutAngle)
                {
                    if (LookDirectionIsOnRange(currentDir))
                    {
                        if (animator.GetBool("IsStrafing")) SmoothValues(strafeHeadWeight, strafeBodyWeight, angle.x, angle.y);
                        else SmoothValues(freeHeadWeight, freeBodyWeight, angle.x, angle.y);
                    }
                    else SmoothValues();
                }
                else
                {
                    if (animator.GetBool("IsStrafing")) SmoothValues(strafeHeadWeight, strafeBodyWeight, angle.x, angle.y);

                    else SmoothValues(freeHeadWeight, freeBodyWeight, angle.x, angle.y);
                }
            }
            else SmoothValues();

            // finish look point calc
            var rotA = Quaternion.AngleAxis(yRotation, transform.up);
            var rotB = Quaternion.AngleAxis(xRotation, transform.right);
            var finalRotation = (rotA * rotB);
            var lookDirection = finalRotation * transform.forward;
            currentLookPoint = headPoint + (lookDirection);

            return lookDirection;
        }

        protected Vector3 defaultLookPoint
        {
            get { return followCamera ? (Camera.main.transform.position + (Camera.main.transform.forward * 100)) : headPoint + (transform.forward * 100); }
        }

        protected virtual Vector2 GetTargetAngle(Vector3 direction)
        {
            var lookRotation = Quaternion.LookRotation(direction, transform.up);        //rotation from head to camera point
            var angleResult = lookRotation.eulerAngles - transform.eulerAngles;         // diference between transform rotation and desiredRotation
            Quaternion desiredRotation = Quaternion.Euler(angleResult);                 // convert angleResult to Rotation
            var x = (float)System.Math.Round(NormalizeAngle(desiredRotation.eulerAngles.x), 2);
            var y = (float)System.Math.Round(NormalizeAngle(desiredRotation.eulerAngles.y), 2);
            return new Vector2(x, y);
        }

        protected virtual bool IgnoreHeadTrackFromAnimator()
        {
            for (int index = 0; index < animator.layerCount; index++)
            {
                var info = animator.GetCurrentAnimatorStateInfo(index);
                if (tagsHash.Contains(info.tagHash))
                {
                    return true;
                }
            }
            return false;
        }

        protected virtual float NormalizeAngle(float angle)
        {
            if (angle < -180)
                return angle + 360;
            else if (angle > 180)
                return angle - 360;
            else
                return angle;
        }

        protected virtual bool LookDirectionIsOnRange(Vector3 direction)
        {
            var angle = GetTargetAngle(direction);
            return (angle.x >= minAngleX && angle.x <= maxAngleX && angle.y >= minAngleY && angle.y <= maxAngleY);
        }

        #endregion

        #endregion

        #region PUBLIC VIRTUAL METHODS. LOOK POINT AND TARGET BEHAVIOUR

        /// <summary>
        /// Set the definitive look Target
        /// </summary>
        /// <param name="target"></param>
        public virtual void SetMainLookTarget(Transform target)
        {
            mainLookTarget = target;
        }

        /// <summary>
        /// Remove the definitive look Target
        /// </summary>
        /// <param name="target"></param>
        public virtual void RemoveMainLookTarget()
        {
            mainLookTarget = null;
        }

        /// <summary>
        /// Simulate a Look Around Animation using the Headtrack
        /// </summary>
        public virtual void LookAround()
        {
            if (!useLookAround) return;
            lookAroundProgress += Time.deltaTime * lookAroundSpeed;
            var pp = Mathf.PingPong(lookAroundProgress, 1f);
            var l = Quaternion.AngleAxis(Mathf.Lerp(-lookAroundAngle, lookAroundAngle, lookAroundCurve.Evaluate(pp)), transform.up) * transform.forward;
            var eyesPoint = eyes ? eyes.position : transform.position + Vector3.up * headHeight;
            var lookp = eyesPoint + l * 100f;
            LookAtPoint(lookp, 0.1f);
        }

        /// <summary>
        /// Look at point. Set a point to follow for default time (override lookTarget).<seealso cref="vAIHeadtrack.timeToExitLookPoint"/> 
        /// if you need to follow point, call this in update or use <seealso cref="vAIHeadtrack.LookAtTarget(Transform)"/>
        /// </summary>
        /// <param name="point">Point to look</param>
        public virtual void LookAtPoint(Vector3 point)
        {
            if (inLockPoint) return;
            temporaryLookPoint = point;
            exitLookPointTime = timeToExitLookPoint;
        }

        /// <summary>
        /// Look at point. Set a point to follow for a especific time (override lookTarget).
        /// if you need to follow point, call this in update or use <seealso cref="vAIHeadtrack.LookAtTarget(Transform, float)"/>
        /// </summary>
        /// <param name="point">Point to look</param>
        /// <param name="timeToExitLookPoint">Time that will to be looking</param>
        public virtual void LookAtPoint(Vector3 point, float timeToExitLookPoint)
        {
            if (inLockPoint) return;
            temporaryLookPoint = point;
            exitLookPointTime = timeToExitLookPoint;
        }

        /// <summary>
        /// Look at target.
        /// Set a target to follow for default time <seealso cref="vAIHeadtrack.timeToExitLookTarget"/> 
        /// if you need to follow target always, call <seealso cref="vAIHeadtrack.LookAtPoint(Vector3)"/> in update
        /// </summary>
        /// <param name="target"> Target to look</param>
        public virtual void LookAtTarget(Transform target)
        {
            if (inLockTarget) return;
            temporaryLookTarget = target;
            exitLookTargetTime = timeToExitLookPoint;
        }

        /// <summary>
        /// Look at target.
        /// Set a target to follow for a especific time
        /// if you need to follow target always, call <seealso cref="vAIHeadtrack.LookAtPoint(Vector3,float)"/> in update or Call <seealso cref="vAIHeadtrack.LockLookAt"/>
        /// </summary>
        /// <param name="target">Target to look</param>
        /// <param name="timeToExitLookTarget"> Time that will to be looking</param>
        public virtual void LookAtTarget(Transform target, float timeToExitLookTarget)
        {
            if (inLockTarget) return;
            temporaryLookTarget = target;
            exitLookTargetTime = timeToExitLookTarget;
        }

        /// <summary>
        /// Lock the current temporary look point
        /// </summary>
        public virtual void LockLookAtPoint()
        {
            inLockPoint = true;
            inLockTarget = false;
        }

        /// <summary>
        /// Unlock the current temporary look point
        /// </summary>
        public virtual void UnlockLookAtPoint()
        {
            inLockPoint = false;
        }

        /// <summary>
        /// Lock the current temporary look target
        /// </summary>
        public virtual void LockLookAtTarget()
        {
            inLockTarget = true;
            inLockPoint = false;
        }

        /// <summary>
        /// Unlock the current temporary look target
        /// </summary>
        public virtual void UnlockLookAtTarget()
        {
            inLockTarget = false;
        }

        /// <summary>
        /// Remove the temporary look point (Ignore timeToExit) <seealso cref="vAIHeadtrack.timeToExitLookPoint"/>
        /// </summary>
        public virtual void ResetLookPoint()
        {
            exitLookPointTime = 0;
            inLockPoint = false;
        }

        /// <summary>
        /// Remove the temporary look target (Ignore timeToExit) <seealso cref="vAIHeadtrack.timeToExitLookTarget"/>
        /// </summary>
        public virtual void ResetLookTarget()
        {
            exitLookTargetTime = 0;
            temporaryLookTarget = null;
            inLockTarget = false;
        }

        /// <summary>
        /// Remove all temporary look (Ignore timeToExit)<seealso cref="vAIHeadtrack.timeToExitLookPoint"/>, <seealso cref="vAIHeadtrack.timeToExitLookTarget"/>
        /// </summary>
        public virtual void ResetLook()
        {
            ResetLookPoint();
            ResetLookTarget();
        }

        /// <summary>
        /// Check if has a look point or look target
        /// </summary>
        public virtual bool isLookingForSomething
        {
            get { return !((defaultLookPoint == targetLookPoint || !canLook) && !isInSmoothValues); }
        }

        /// <summary>
        /// Set off set look height
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetOffsetLookHeight(float value)
        {
            currentOffsetLookHeight = value;
        }

        /// <summary>
        /// Set offset look Height to default <see cref="vAIHeadtrack.offSetLookHeight"/>
        /// </summary>
        public virtual void ResetOffseLookHeight()
        {
            currentOffsetLookHeight = offSetLookHeight;
        }
        #endregion
    }
}