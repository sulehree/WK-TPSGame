using UnityEngine;

namespace Invector.vCharacterController.AI
{
    using vEventSystems;
    using System.Collections.Generic;

    public partial interface vIControlAI : vIHealthController
    {
        /// <summary>
        /// Used just to Create AI Editor
        /// </summary>
        void CreatePrimaryComponents();
        /// <summary>
        /// Used just to Create AI Editor
        /// </summary>
        void CreateSecundaryComponents();

        /// <summary>
        /// Check if <seealso cref="vIControlAI"/> has a <seealso cref=" vIAIComponent"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool HasComponent<T>() where T : vIAIComponent;
        
        /// <summary>
        /// Get Specific <seealso cref="vIAIComponent"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetAIComponent<T>() where T : vIAIComponent;       

        Vector3 selfStartPosition { get; set; }
        Vector3 targetDestination { get; }
        Collider selfCollider { get; }
        Animator animator { get; }
        vAnimatorStateInfos animatorStateInfos { get; }
        vWaypointArea waypointArea { get; set; }
        vDamage receivedDamage { get; }
        vWaypoint targetWaypoint { get; }
        List<vWaypoint> visitedWaypoints { get; set; }
        vAITarget currentTarget { get; }
        Vector3 lastTargetPosition { get; }        

        bool isInDestination { get; }
        bool isMoving { get; }
        bool targetInLineOfSight { get; }

        float targetDistance { get; }
        float changeWaypointDistance { get; }
        float remainingDistance { get; }
        float stopingDistance { get; set; }
        bool selfStartingPoint { get; }
        bool customStartPoint { get; }
        
        Vector3 customStartPosition { get; }
        void SetDetectionLayer(LayerMask mask);
        void SetDetectionTags(List<string>tags);
        void SetObstaclesLayer(LayerMask mask);
        void SetLineOfSight(float fov = -1, float minDistToDetect = -1, float maxDistToDetect = -1, float lostTargetDistance = -1);       
        void NextWayPoint();
        void SetSpeed(vAIMovementSpeed speed);
        void MoveTo(Vector3 destination);
        void StrafeMoveTo(Vector3 destination, Vector3 forwardDiretion);
        void RotateTo(Vector3 direction);
        void SetCurrentTarget(Transform target,bool overrideCanseeTarget = true);
        void RemoveCurrentTarget();
        void FindTarget();
        void LookAround();
        void LookTo(Vector3 point, float stayLookTime = 1f);
        void LookToTarget(Transform target, float stayLookTime = 1f);
        void Stop();
        void ClearDamage();
        void ForceUpdatePath(float timeInUpdate = 1f);
    }

    public partial interface vIControlAICombat : vIControlAI
    {
        int strafeCombatSide { get; }     
        float minDistanceOfTheTarget { get; }        
        float combatRange { get; }
        bool isInCombat { get; set; }
        bool strafeCombatMovement { get; }     
      
        int attackCount { get; set; }
        float attackDistance { get; }
        bool isAttacking { get; }
        bool canAttack { get; }
        void InitAttackTime();
        void ResetAttackTime();
        void Attack(bool strongAttack = false, int attackID = -1);

        bool isBlocking { get; }
        bool canBlockInCombat { get; }     
        void ResetBlockTime();
        void Blocking();
        
        void AimTo(Vector3 point, float stayLookTime = 1f, object sender = null);
        void AimToTarget(float stayLookTime = 1f, object sender = null);
      
    }

    public partial interface vIControlAIMelee : vIControlAICombat
    {
        vMelee.vMeleeManager MeleeManager { get; set; }
        void SetMeleeHitTags(List<string> tags);
    }

    public partial interface vIControlAIShooter : vIControlAICombat
    {
        vAIShooterManager shooterManager { get; set; }
        void SetShooterHitLayer(LayerMask mask);
    }

    [System.Serializable]
    public class vAISimpleTarget
    {
        [SerializeField, HideInInspector] protected Transform _transform;
        [SerializeField, HideInInspector] protected Collider _collider;
        public Transform transform { get { return _transform; } protected set { _transform = value; } }
        public Collider collider { get { return _collider; } protected set { _collider = value; } }

        public virtual void InitTarget(Transform target)
        {
            if (target)
            {
                transform = target;
                collider = transform.GetComponent<Collider>();
            }
        }
        public virtual void ClearTarget()
        {
            transform = null;
            collider = null;
        }
        public static implicit operator Transform(vAISimpleTarget m)
        {
            try
            {
                return m.transform;
            }
            catch { return null; }
        }
    }

    [System.Serializable]
    public class vAITarget : vAISimpleTarget
    {
        public vHealthController healthController;
        public vIMeleeFighter meleeFighter;
        public bool isLost;
        public bool isFixedTarget = true;
        public bool _hadHealthController;

        public bool hasCollider
        {
            get
            {
                return collider != null;
            }
        }

        public bool hasHealthController
        {
            get
            {
                if (_hadHealthController && healthController == null)
                    transform = null;
                return healthController != null;
            }
        }

        public bool isDead
        {
            get
            {
                var value = true;                
                if (hasHealthController) value = healthController.isDead;                
                else if (_hadHealthController) return true;                
                else if (transform) value = !transform.gameObject.activeSelf;
                return value;
            }
        }

        public bool isArmed
        {
            get
            {
                if (!isFighter) return false;
                return meleeFighter.isArmed;
            }
        }

        public bool isBlocking
        {
            get
            {
                if (!isFighter) return false;
                return meleeFighter.isBlocking;
            }
        }

        public bool isAttacking
        {
            get
            {
                if (!isFighter) return false;
                return meleeFighter.isAttacking;
            }
        }

        public bool isFighter
        {
            get
            {
                return meleeFighter != null;
            }
        }

        public float currentHealth
        {
            get
            {
                if (hasHealthController) return healthController.currentHealth;
                return 0;
            }
        }

        public override void InitTarget(Transform target)
        {
            base.InitTarget(target);
            if (target)
            {
                healthController = target.GetComponent<vHealthController>();
                _hadHealthController = this.healthController != null;
                meleeFighter = target.GetComponent<vIMeleeFighter>();
            }
        }

        public override void ClearTarget()
        {
            base.ClearTarget();
            healthController = null;
            meleeFighter = null;
        }
    }

    public interface vIStateAttackListener
    {
        void OnReceiveAttack(vIControlAICombat combatController, ref vDamage damage, vIMeleeFighter attacker, ref bool canBlock);
    }
}