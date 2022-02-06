using UnityEngine;

namespace AI
{
    public class HumanFlee : AIMovement
    {
        public float stepRadius;
        public float speedThreshold;
        public float arcThreshold;

        private void DrawDebug(AIAgent agent)
        {
            if (debug)
            {
                DebugUtil.DrawCircle(agent.TargetPosition, transform.up, Color.red, stepRadius);
            }
        }

        public override SteeringOutput GetKinematic(AIAgent agent)
        {
            // NOT NEEDED
            return base.GetKinematic(agent);
        }

        public override SteeringOutput GetSteering(AIAgent agent)
        {
            DrawDebug(agent);

            var output = base.GetSteering(agent);

            // Get velocity to target, factoring for looping terrain
            Vector3 loopedPosition;
            if (agent.TargetPosition.x < 0) {
                loopedPosition = new Vector3(agent.TargetPosition.x + GameConstants.STAGE_WIDTH, agent.TargetPosition.y, agent.TargetPosition.z);
            } else {
                loopedPosition = new Vector3(agent.TargetPosition.x - GameConstants.STAGE_WIDTH, agent.TargetPosition.y, agent.TargetPosition.z);
            }

            Vector3 directVelocity = agent.TargetPosition - transform.position;
            Vector3 loopedVelocity = loopedPosition - transform.position;
            Vector3 desiredVelocity;

            if (directVelocity.magnitude < loopedVelocity.magnitude) {
                desiredVelocity = -directVelocity;
            } else {
                desiredVelocity = -loopedVelocity;
            }

            float distance = desiredVelocity.magnitude;
            desiredVelocity = desiredVelocity.normalized * agent.maxSpeed;

            // Determine angle difference between player facing and destination
            Vector3 from = Vector3.ProjectOnPlane(agent.transform.forward, Vector3.up);
            Vector3 to = Quaternion.LookRotation(desiredVelocity) * Vector3.forward;
            float angleY = Vector3.SignedAngle(from, to, Vector3.up);
            Quaternion desiredRotation = Quaternion.identity;

            if (distance < stepRadius) {
                // CONSIDERED SLOW SPEED AND NEAR ENOUGH TO THE GOAL
            } else {
                if (Mathf.Abs(angleY) <= arcThreshold) {
                    // Player is facing away from the goal
                    desiredVelocity = transform.forward * agent.maxSpeed;
                } else {
                    // Player is still turning
                    desiredVelocity = Vector3.zero;
                }
                if (agent.Velocity.magnitude <= speedThreshold) {
                    // Player is stopped
                    desiredRotation = Quaternion.AngleAxis(angleY, Vector3.up);
                }
            }
            
            output.linear = desiredVelocity - agent.Velocity;
            output.angular = desiredRotation;

            return output;
        }
    }
}
