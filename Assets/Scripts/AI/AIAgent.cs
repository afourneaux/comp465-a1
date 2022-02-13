using UnityEngine;

namespace AI
{
    public class AIAgent : MonoBehaviour
    {
        public float maxSpeed;
        public float maxDegreesDelta;
        public float initialSpeed;
        public float acceleration;
        public float speedBoostValue;
        public bool lockY = true;
        public bool debug;
        public bool isFrozen = false;
        public bool hasSpeedBoost = false;

        public enum EBehaviorType { Kinematic, Steering }
        public EBehaviorType behaviorType;

        private Animator animator;

        [SerializeField] private Transform trackedTarget;
        [SerializeField] private Vector3 targetPosition;
        public Vector3 TargetPosition
        {
            get => trackedTarget != null ? trackedTarget.position : targetPosition;
        }
        public Vector3 TargetForward
        {
            get => trackedTarget != null ? trackedTarget.forward : Vector3.forward;
        }
        public Vector3 TargetVelocity
        {
            get
            {
                Vector3 v = Vector3.zero;
                if (trackedTarget != null)
                {
                    AIAgent targetAgent = trackedTarget.GetComponent<AIAgent>();
                    if (targetAgent != null)
                        v = targetAgent.Velocity;
                }

                return v;
            }
        }

        public Vector3 Velocity { get; set; }

        public void TrackTarget(Transform targetTransform)
        {
            trackedTarget = targetTransform;
        }

        public void UnTrackTarget()
        {
            trackedTarget = null;
        }

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void Start() {
            Velocity = transform.forward * initialSpeed;
        }

        private void Update()
        {
            if (debug)
                Debug.DrawRay(transform.position, Velocity, Color.red);

            Vector3 steeringSum;
            Quaternion rotationSum;

            if (behaviorType == EBehaviorType.Kinematic)
            {
                // TODO: average all kinematic behaviors attached to this object to obtain the final kinematic output and then apply it
                rotationSum = Quaternion.identity;
            }
            else
            {
                GetSteeringSum(out steeringSum, out rotationSum);
                if (debug) {
                    Debug.DrawRay(transform.position + Velocity, steeringSum * acceleration, Color.green);
                }
                Vector3 accelerationValue = steeringSum * acceleration * Time.deltaTime;
                if (hasSpeedBoost) {
                    accelerationValue *= speedBoostValue;
                }
                Velocity += accelerationValue;
                Velocity = Vector3.ClampMagnitude(Velocity, hasSpeedBoost ? maxSpeed * speedBoostValue : maxSpeed);
            }

            transform.position += Velocity * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, transform.rotation * rotationSum, maxDegreesDelta * Time.deltaTime);
            
            // If the player hits the top or bottom of the stage, bounce them back
            if (transform.position.z > GameConstants.STAGE_TOP) {
                transform.position = new Vector3(transform.position.x, transform.position.y, GameConstants.STAGE_TOP);
                Velocity = new Vector3(Velocity.x, Velocity.y, -Velocity.z);
            }
            if (transform.position.z < GameConstants.STAGE_BOTTOM) {
                transform.position = new Vector3(transform.position.x, transform.position.y, GameConstants.STAGE_BOTTOM);
                Velocity = new Vector3(Velocity.x, Velocity.y, -Velocity.z);
            }
            
            if (Mathf.Abs(transform.position.x) > GameConstants.STAGE_WIDTH / 2) {
                float x = transform.position.x;
                if (x < 0) {
                    x += GameConstants.STAGE_WIDTH;
                } else {
                    x -= GameConstants.STAGE_WIDTH;
                }
                transform.position = new Vector3(x, 0, transform.position.z);
            }
            
            //animator.SetBool("walking", Velocity.magnitude > 0);
            //animator.SetBool("running", Velocity.magnitude > maxSpeed/2);
        }

        private void GetKinematicAvg(out Vector3 kinematicAvg, out Quaternion rotation)
        {
            kinematicAvg = Vector3.zero;
            Vector3 eulerAvg = Vector3.zero;
            AIMovement[] movements = GetComponents<AIMovement>();
            int count = 0;
            foreach (AIMovement movement in movements)
            {
                kinematicAvg += movement.GetKinematic(this).linear;
                eulerAvg += movement.GetKinematic(this).angular.eulerAngles;

                ++count;
            }

            if (count > 0)
            {
                kinematicAvg /= count;
                eulerAvg /= count;
                rotation = Quaternion.Euler(eulerAvg);
            }
            else
            {
                kinematicAvg = Velocity;
                rotation = transform.rotation;
            }
        }

        private void GetSteeringSum(out Vector3 steeringForceSum, out Quaternion rotation)
        {
            steeringForceSum = Vector3.zero;
            rotation = Quaternion.identity;
            AIMovement[] movements = GetComponents<AIMovement>();
            foreach (AIMovement movement in movements)
            {
                SteeringOutput output = movement.GetSteering(this);
                steeringForceSum += output.linear;
                rotation *= output.angular;
            }
        }
    }
}