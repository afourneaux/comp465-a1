using UnityEngine;

namespace AI
{
    public class HumanMove : AIMovement
    {
        public float stepRadius;
        public float stopRadius;
        public float arcScalar;
        public float speedThreshold;


        private void DrawDebug(AIAgent agent)
        {
            if (debug)
            {
                DebugUtil.DrawCircle(agent.TargetPosition, transform.up, Color.yellow, stopRadius);
                DebugUtil.DrawCircle(agent.TargetPosition, transform.up, Color.magenta, stepRadius);

                // Display speed-based view cone
                float angle = arcScalar / Mathf.Max(agent.Velocity.magnitude, 0.1f);
                Debug.DrawLine(transform.position, transform.position + Quaternion.AngleAxis(-angle/2, Vector3.up) * transform.forward, Color.yellow);
                Debug.DrawLine(transform.position, transform.position + Quaternion.AngleAxis(angle/2, Vector3.up) * transform.forward, Color.yellow);
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
                desiredVelocity = directVelocity;
            } else {
                desiredVelocity = loopedVelocity;
            }

            float distance = desiredVelocity.magnitude;
            desiredVelocity = desiredVelocity.normalized * agent.maxSpeed;

            // Determine angle difference between player facing and destination
            Vector3 from = Vector3.ProjectOnPlane(agent.transform.forward, Vector3.up);
            Vector3 to = Quaternion.LookRotation(desiredVelocity) * Vector3.forward;
            float angleY = Vector3.SignedAngle(from, to, Vector3.up);
            Quaternion desiredRotation = Quaternion.identity;

            if (distance <= stopRadius) {
                // ALREADY AT LOCATION
                desiredVelocity = -agent.Velocity;
            } else if (agent.Velocity.magnitude < speedThreshold && distance < stepRadius) {
                // CONSIDERED SLOW SPEED AND NEAR ENOUGH TO THE GOAL
                desiredVelocity *= Mathf.Clamp((distance / stepRadius), 0.0f, speedThreshold);
            } else if (distance < stepRadius) {
                // ENTERING STEP RADIUS AT HIGH SPEED, SLOW DOWN
                desiredVelocity *= -1;
            } else {
                desiredRotation = Quaternion.AngleAxis(angleY, Vector3.up);

                if (Mathf.Abs(angleY) < arcScalar / Mathf.Max(agent.Velocity.magnitude, 0.1f)) {
                    // Angle is within player sight
                } else {
                    // Angle is outside of player sight
                    desiredVelocity = -agent.Velocity;
                }
            }
            
            output.linear = desiredVelocity - agent.Velocity;
            output.angular = desiredRotation;

            return output;
        }
    }
}
