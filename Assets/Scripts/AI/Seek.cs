using UnityEngine;

namespace AI
{
    public class Seek : AIMovement
    {
        public override SteeringOutput GetKinematic(AIAgent agent)
        {
            var output = base.GetKinematic(agent);

            // TODO: calculate linear component
            

            if (debug) Debug.DrawRay(transform.position, output.linear, Color.cyan);

            return output;
        }

        public override SteeringOutput GetSteering(AIAgent agent)
        {
            var output = base.GetSteering(agent);

            Vector3 desiredVelocity = agent.TargetPosition - transform.position;
            desiredVelocity = desiredVelocity.normalized * agent.maxSpeed;
            output.linear = desiredVelocity - agent.Velocity;

            if (debug) Debug.DrawRay(transform.position + agent.Velocity, output.linear, Color.green);

            return output;
        }
    }
}
