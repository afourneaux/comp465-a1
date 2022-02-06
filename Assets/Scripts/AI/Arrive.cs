using UnityEngine;

namespace AI
{
    public class Arrive : AIMovement
    {
        public float slowRadius;
        public float stopRadius;

        private void DrawDebug(AIAgent agent)
        {
            if (debug)
            {
                DebugUtil.DrawCircle(agent.TargetPosition, transform.up, Color.yellow, stopRadius);
                DebugUtil.DrawCircle(agent.TargetPosition, transform.up, Color.magenta, slowRadius);
            }
        }

        public override SteeringOutput GetKinematic(AIAgent agent)
        {
            DrawDebug(agent);

            var output = base.GetKinematic(agent);

            // TODO: calculate linear component
			
			
            return output;
        }

        public override SteeringOutput GetSteering(AIAgent agent)
        {
            DrawDebug(agent);

            var output = base.GetSteering(agent);

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

            if (distance <= stopRadius) {
                desiredVelocity = Vector3.zero;
            } else if (distance < slowRadius) {
                desiredVelocity *= (distance / slowRadius);
            }
            
            output.linear = desiredVelocity - agent.Velocity;

            return output;
        }
    }
}
