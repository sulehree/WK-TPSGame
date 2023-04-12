
using UnityEngine;
using System.Collections;

namespace Invector.vCharacterController.AI
{
    [vClassHeader(" AI SHOOTER CONTROLLER", iconName = "AI-icon")]
    public class vControlAIShooter : vControlAICombat, vIControlAIShooter
    {
        [vEditorToolbar("Shooter Settings", order = 10)]
        [Header("Shooter Settings")]
        public float minTimeShooting = 2f;
        public float maxTimeShooting = 5f;
        public float minShotWaiting = 3f;
        public float maxShotWaiting = 6f;
        public float aimTargetHeight = .35f;
        public bool doReloadWhileWaiting = true;
        public float aimSmoothDamp = 10f;
        [SerializeField]
        private bool lockAimDebug;
        [SerializeField]
        [vHideInInspector("lockAimDebug")]
        private Transform aimDebugTarget;
        [SerializeField]
        [vHideInInspector("lockAimDebug")]
        private bool debugShoots;
        public bool IsReloading { get; protected set; }
        public bool IsEquipping { get; protected set; }
        public bool IsAiming { get; protected set; }
        public bool IsValidAim { get; protected set; }
        public vAIShooterManager shooterManager { get; set; }
        protected float _timeShotting;
        protected int shots;
        protected float _waitingToShot;
        protected float _upperBodyID;
        protected Vector3 aimPosition;
        protected bool inShotDelay;
        protected bool waitingReload;
        protected IK.vIKSolver leftIK, rightIK;

        private Transform leftUpperArm, rightUpperArm, leftHand, rightHand;
        private GameObject aimAngleReference;
        private Quaternion upperArmRotation, handRotation;
        private float rightRotationWeight;
        private float _onlyArmsLayerWeight;
        private float handIKWeight;
        private float aimTime;
        private float deleyEnableAimAfterRagdolled;
        private int onlyArmsLayer;
        private int _moveSetID;
        private int _attackID;
        private bool aimEnable;

        private Vector3 aimVelocity;
        private Vector3 aimTarget;
        public override void CreateSecundaryComponents()
        {
            base.CreateSecundaryComponents();
            if (GetComponent<vAIShooterManager>() == null) gameObject.AddComponent<vAIShooterManager>();
            if (GetComponent<vAIHeadtrack>() == null) gameObject.AddComponent<vAIHeadtrack>();
        }

        protected int MoveSetID
        {
            get
            {
                return _moveSetID;
            }
            set
            {
                if (value != _moveSetID || animator.GetFloat("MoveSet_ID") != value)
                {
                    _moveSetID = value;
                    animator.SetFloat("MoveSet_ID", (float)_moveSetID, 0.25f, Time.deltaTime);
                }
            }
        }

        protected int AttackID
        {
            get
            {
                return _attackID;
            }
            set
            {
                if (value != _attackID)
                {
                    _attackID = value;
                    animator.SetInteger("AttackID", _attackID);
                }
            }
        }

        public bool CanShot
        {
            get
            {
                if (_waitingToShot < Time.time && !inTurn && (isStrafing || debugShoots))
                {
                    _timeShotting = Random.Range(minTimeShooting, maxTimeShooting) + Time.time;
                    _waitingToShot = _timeShotting + Random.Range(minShotWaiting, maxShotWaiting);
                }

                return _timeShotting > Time.time;
            }
        }

        protected override void Start()
        {
            base.Start();
            waitingReload = false;
            inShotDelay = true;
            InitShooter();
        }

        public Vector3 _debugAimPosition;

        protected float UpperBodyID
        {
            get { return _upperBodyID; }
            set
            {
                if (_upperBodyID != value || animator.GetFloat("UpperBody_ID") != value)
                {
                    _upperBodyID = value;
                    animator.SetFloat("UpperBody_ID", _upperBodyID);
                }
            }
        }

        Vector3 DebugAimPosition
        {
            get
            {
                return !aimDebugTarget ? (transform.position + transform.forward * (2f + _debugAimPosition.z) + transform.right * _debugAimPosition.x + transform.up * (1.5f + _debugAimPosition.y)) : aimDebugTarget.position;
            }
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            if (lockAimDebug)
                Gizmos.DrawSphere(DebugAimPosition, 0.1f);
        }

        public virtual void SetShooterHitLayer(LayerMask mask)
        {
            if (shooterManager)
            {
                shooterManager.SetDamageLayer(mask);
            }
        }

        public override void Attack(bool strongAttack = false, int attackID = -1)
        {
            if (ragdolled) return;
            if (shooterManager && attackID != -1)
                AttackID = attackID;
            else
                AttackID = shooterManager.GetAttackID();

            if (currentTarget.transform)
            {
                if (CanShot) shots++;
            }
        }

        public override void ResetAttackTime()
        {
            base.ResetAttackTime();
            _waitingToShot = _timeShotting + Random.Range(minShotWaiting, maxShotWaiting);
        }

        protected virtual void InitShooter()
        {
            if (_headtrack)
            {
                _headtrack.onPreUpdateSpineIK.AddListener(HandleAim);
                _headtrack.onPosUpdateSpineIK.AddListener(IKBehaviour);
            }
            shooterManager = GetComponent<vAIShooterManager>();
            leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            onlyArmsLayer = animator.GetLayerIndex("OnlyArms");
            aimAngleReference = new GameObject("aimAngleReference");
            aimAngleReference.transform.rotation = transform.rotation;
            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            aimAngleReference.transform.SetParent(head);
            aimAngleReference.transform.localPosition = Vector3.zero;
        }

        protected virtual void HandleAim()
        {
            if (ragdolled)
            {
                aimTime = 0;
                IsAiming = false;
                deleyEnableAimAfterRagdolled = 2f;
                return;
            }
            else if (deleyEnableAimAfterRagdolled <= 0)
            {
                ControlAimTime();
                if (IsAiming) _headtrack.LookAtPoint(aimPosition, 1f);
            }
            else
            {
                aimTime = 0;
                IsAiming = false;
                deleyEnableAimAfterRagdolled -= Time.deltaTime;
            }
        }

        protected virtual void IKBehaviour()
        {
            UpdateAimBehaviour();
            if (lockAimDebug)
            {
                if (!IsStrafingAnim)
                {
                    isStrafing = true;
                    IsStrafingAnim = true;
                }

                AimTo(DebugAimPosition, .5f);
                if (debugShoots) Shot();
            }
        }

        protected override void UpdateAnimator()
        {
            base.UpdateAnimator();
            UpdateCombatAnimator();
        }

        protected override void UpdateCombatAnimator()
        {
            base.UpdateCombatAnimator();
            UpdateShooterAnimator();
        }

        protected virtual void UpdateShooterAnimator()
        {
            if (shooterManager.currentWeapon)
            {
                IsReloading = IsAnimatorTag("Reload");
                if (waitingReload && IsReloading) StartCoroutine(ResetShotDelay());
                // find states with the IsEquipping tag
                IsEquipping = IsAnimatorTag("IsEquipping");
                var _isAiming = IsAiming && !IsReloading;
                if (_isAiming && !aimEnable)
                {
                    shooterManager.currentWeapon.onEnableAim.Invoke();
                    aimEnable = true;
                }
                else if (!_isAiming && aimEnable)
                {
                    shooterManager.currentWeapon.onDisableAim.Invoke();
                    aimEnable = false;
                }
                animator.SetBool("CanAim", _isAiming && CanAim);

                UpperBodyID = shooterManager.GetUpperBodyID();
                MoveSetID = shooterManager.GetMoveSetID();
                animator.SetBool("IsAiming", _isAiming);
            }
            else
            {
                IsReloading = false;
                animator.SetBool("IsAiming", false);
                animator.SetBool("CanAim", false);
                if (aimEnable)
                {
                    shooterManager.currentWeapon.onDisableAim.Invoke();
                    aimEnable = false;
                }
            }
            _onlyArmsLayerWeight = Mathf.Lerp(_onlyArmsLayerWeight, IsAiming || isRolling ? 0f : shooterManager && shooterManager.currentWeapon ? 1f : 0f, 6f * Time.deltaTime);
            animator.SetLayerWeight(onlyArmsLayer, _onlyArmsLayerWeight);
        }

        protected virtual void UpdateAimBehaviour()
        {
            if (shooterManager && shooterManager.rWeapon && shooterManager.rWeapon.gameObject.activeSelf)
            {
                RotateArm();
                RotateHand();
                if (!shooterManager.lWeapon || !shooterManager.lWeapon.gameObject.activeSelf)
                    UpdateSupportHandIK();
            }
            if (shooterManager && shooterManager.lWeapon && shooterManager.lWeapon.gameObject.activeSelf)
            {
                RotateArm(true);
                RotateHand(true);
                if (!shooterManager.rWeapon || !shooterManager.rWeapon.gameObject.activeSelf)
                    UpdateSupportHandIK(true);
            }
            UpdateShotTime();
            if (shots > 0)
            {
                Shot();
            }
            ValidateAim();
            if (IsAiming && IsValidAim)
                aimPosition = Vector3.SmoothDamp(aimPosition, aimTarget, ref aimVelocity, aimSmoothDamp * Time.deltaTime);
            else
                aimPosition = aimTarget;
        }

        protected virtual void ValidateAim()
        {
            if (shooterManager && IsAiming && CanAim)
            {
                var weapon = shooterManager.rWeapon ? shooterManager.rWeapon : shooterManager.lWeapon;
                if (weapon)
                {
                    var angle = Vector3.Angle(weapon.muzzle.forward, aimPosition - aimAngleReference.transform.position);
                    IsValidAim = angle < 10;
                    return;
                }
            }
            IsValidAim = false;
        }

        protected virtual void UpdateShotTime()
        {
            shooterManager.UpdateShotTime();
        }

        protected virtual void ControlAimTime()
        {
            if (aimTime > 0)
            {

                aimTime -= Time.deltaTime;
            }
            else if (IsAiming) IsAiming = false;
        }

        protected virtual void UpdateSupportHandIK(bool isUsingLeftHand = false)
        {
            if (ragdolled) return;
            var weapon = shooterManager.rWeapon ? shooterManager.rWeapon : shooterManager.lWeapon;
            if (!shooterManager || !weapon || !weapon.gameObject.activeInHierarchy || !shooterManager.useLeftIK) return;
            if (animator.GetCurrentAnimatorStateInfo(6).IsName("Shot Fire") && weapon.disableIkOnShot) { handIKWeight = 0; return; }

            bool useIkConditions = false;
            var animatorInput = isStrafing ? new Vector2(animator.GetFloat("InputVertical"), animator.GetFloat("InputHorizontal")).magnitude : animator.GetFloat("InputVertical");
            if (!IsAiming)
            {
                if (animatorInput < 0.1f)
                    useIkConditions = weapon.useIkOnIdle;
                else if (isStrafing)
                    useIkConditions = weapon.useIkOnStrafe;
                else
                    useIkConditions = weapon.useIkOnFree;
            }
            else if (IsAiming)
                useIkConditions = weapon.useIKOnAiming;

            // create left arm ik solver if equal null
            if (leftIK == null) leftIK = new IK.vIKSolver(animator, AvatarIKGoal.LeftHand);
            if (rightIK == null) rightIK = new IK.vIKSolver(animator, AvatarIKGoal.RightHand);

            IK.vIKSolver targetIK = null;
            if (isUsingLeftHand)
            {
                targetIK = rightIK;
            }
            else targetIK = leftIK;

            if (targetIK != null)
            {
                // control weight of ik
                if (weapon && weapon.handIKTarget && Time.timeScale > 0 && !IsReloading && !actions && !customAction && !IsEquipping && (isGrounded || IsAiming) && !lockMovement && useIkConditions)
                    handIKWeight = Mathf.Lerp(handIKWeight, 1, 10f * Time.deltaTime);
                else
                    handIKWeight = Mathf.Lerp(handIKWeight, 0, 10f * Time.deltaTime);

                if (handIKWeight <= 0) return;
                // update IK
                targetIK.SetIKWeight(handIKWeight);
                if (shooterManager && weapon && weapon.handIKTarget)
                {
                    var _offset = (weapon.handIKTarget.forward * shooterManager.ikPositionOffset.z) + (weapon.handIKTarget.right * shooterManager.ikPositionOffset.x) + (weapon.handIKTarget.up * shooterManager.ikPositionOffset.y);
                    targetIK.SetIKPosition(weapon.handIKTarget.position + _offset);
                    var _rotation = Quaternion.Euler(shooterManager.ikRotationOffset);
                    targetIK.SetIKRotation(weapon.handIKTarget.rotation * _rotation);
                }
            }
        }

        protected virtual void RotateArm(bool isUsingLeftHand = false)
        {
            if (!shooterManager || IsReloading || ragdolled) return;

            var weapon = !isUsingLeftHand ? shooterManager.rWeapon : shooterManager.lWeapon;

            if (weapon && weapon.gameObject.activeInHierarchy && (IsAiming) && weapon.alignRightUpperArmToAim && CanAim)
            {
                var aimPoint = aimPosition;
                Vector3 v = aimPoint - weapon.aimReference.position;
                Vector3 v2 = Quaternion.AngleAxis(-weapon.recoilUp, weapon.aimReference.right) * v;
                var orientation = weapon.aimReference.forward;
                rightRotationWeight = Mathf.Lerp(rightRotationWeight, !shooterManager.isShooting || weapon.ammoCount <= 0 ? 1f : 0f, .2f * Time.deltaTime);
                var upperArm = isUsingLeftHand ? leftUpperArm : rightUpperArm;
                var r = Quaternion.FromToRotation(orientation, v) * upperArm.rotation;
                var r2 = Quaternion.FromToRotation(orientation, v2) * upperArm.rotation;
                Quaternion rot = Quaternion.Lerp(r2, r, rightRotationWeight);
                var angle = Vector3.Angle(aimPosition - aimAngleReference.transform.position, aimAngleReference.transform.forward);

                if (!(angle > shooterManager.maxHandAngle || angle < -shooterManager.maxHandAngle))
                    upperArmRotation = Quaternion.Lerp(upperArmRotation, rot, shooterManager.smoothHandRotation * Time.deltaTime);
                // else upperArmRotation = upperArm.rotation;// Quaternion.Lerp(upperArm.rotation, rot, shooterManager.smoothHandRotation * Time.deltaTime);
                if (!float.IsNaN(upperArmRotation.x) && !float.IsNaN(upperArmRotation.y) && !float.IsNaN(upperArmRotation.z))
                    upperArm.rotation = upperArmRotation;
            }
        }

        protected virtual void RotateHand(bool isUsingLeftHand = false)
        {
            if (!shooterManager || IsReloading || ragdolled) return;
            var weapon = !isUsingLeftHand ? shooterManager.rWeapon : shooterManager.lWeapon;

            if (weapon && weapon.gameObject.activeInHierarchy && weapon.alignRightHandToAim && IsAiming && CanAim)
            {
                var aimPoint = aimPosition;
                Vector3 v = aimPoint - weapon.aimReference.position;
                Vector3 v2 = Quaternion.AngleAxis(-weapon.recoilUp, weapon.aimReference.right) * v;
                var orientation = weapon.aimReference.forward;

                if (!weapon.alignRightUpperArmToAim)
                    rightRotationWeight = Mathf.Lerp(rightRotationWeight, !shooterManager.isShooting || weapon.ammoCount <= 0 ? 1f : 0f, .2f * Time.deltaTime);

                var hand = isUsingLeftHand ? leftHand : rightHand;
                var r = Quaternion.FromToRotation(orientation, v) * hand.rotation;
                var r2 = Quaternion.FromToRotation(orientation, v2) * hand.rotation;
                Quaternion rot = Quaternion.Lerp(r2, r, rightRotationWeight);
                var angle = Vector3.Angle(aimPosition - aimAngleReference.transform.position, aimAngleReference.transform.forward);

                if (!(angle > shooterManager.maxHandAngle || angle < -shooterManager.maxHandAngle))
                    handRotation = Quaternion.Lerp(handRotation, rot, shooterManager.smoothHandRotation * Time.deltaTime);
                // else handRotation = Quaternion.Lerp(hand.rotation, rot, shooterManager.smoothHandRotation * Time.deltaTime);

                if (!float.IsNaN(handRotation.x) && !float.IsNaN(handRotation.y) && !float.IsNaN(handRotation.z))
                    hand.rotation = handRotation;
                weapon.SetScopeLookTarget(aimPoint);
            }
        }

        protected virtual bool CanAim
        {
            get
            {
                if (ragdolled || !isStrafing && !lockAimDebug) return false;
                var p1 = aimTarget;
                p1.y = transform.position.y;
                var angle = Vector3.Angle(transform.forward, p1 - transform.position);
                var can = angle < 30f;
                //var aimLocalPoint = transform.InverseTransformPoint(aimTarget);
                //var can = aimLocalPoint.z > _capsuleCollider.radius && Mathf.Abs(aimLocalPoint.x) > _capsuleCollider.radius;
                if (!can) RotateTo(aimTarget - transform.position);
                return can;
            }
        }

        public virtual void Shot()
        {
            if (isDead || !shooterManager || !shooterManager.currentWeapon) return;

            if (CanShot && !IsReloading && !waitingReload && inShotDelay && CanAim && IsValidAim && IsAiming)
            {
                if (shooterManager.weaponHasAmmo)
                    shooterManager.Shoot(aimPosition);
                else if (!IsReloading && !waitingReload)
                    StartCoroutine(ReloadDelay());
            }

            if (!CanShot && !IsReloading && !waitingReload && doReloadWhileWaiting && shooterManager.currentWeapon.ammoCount < shooterManager.currentWeapon.clipSize)
            {
                shooterManager.ReloadWeapon();
            }
        }

        protected virtual IEnumerator ReloadDelay()
        {
            waitingReload = true;
            inShotDelay = false;
            yield return new WaitForSeconds(.5f);
            shooterManager.ReloadWeapon();
        }

        protected virtual IEnumerator ResetShotDelay()
        {
            waitingReload = false;
            while (IsReloading)
            {
                yield return null;
            }
            yield return new WaitForSeconds(.5f);
            inShotDelay = true;
        }

        protected override void TryBlockAttack(vDamage damage)
        {
            if (shooterManager.currentWeapon != null) { isBlocking = false; }
            else base.TryBlockAttack(damage);
        }

        public override void Blocking()
        {
            if (shooterManager.currentWeapon != null) { isBlocking = false; return; }
            base.Blocking();
        }
               
        public override void AimTo(Vector3 point, float timeToCancelAim = 0.1f, object sender = null)
        {
            aimTime = timeToCancelAim;
            IsAiming = true;
            aimTarget = point;
        }

        public override void AimToTarget(float stayLookTime = 1, object sender = null)
        {
            aimTime = stayLookTime;
            IsAiming = true;
            if (currentTarget.transform && currentTarget.collider)
                aimTarget = _lastTargetPosition + Vector3.up * ((currentTarget.collider.bounds.size.y * 0.5f) + aimTargetHeight);
            else
                aimTarget = _lastTargetPosition + Vector3.up * aimTargetHeight;
        }
    }
}
