using UnityEngine;

namespace AI
{
    public class FaceAway : AIMovement
    {
        public override SteeringOutput GetKinematic(AIAgent agent)
        {
            var output = base.GetKinematic(agent);

            // TODO: calculate angular component
			

            return output;
        }

        public override SteeringOutput GetSteering(AIAgent agent)
        {
            var output = base.GetSteering(agent);

            // TODO: calculate angular component
			

            return output;
        }
    }
}
