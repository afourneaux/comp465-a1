using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class TagPlayer : AIMovement
    {
        public float safeRadius;
        public float tagRadius;
        public bool isIt;
        public bool isTarget = false;
        private bool isFrozen = false;
        private TagPlayer target = null;
        public TagPlayer It;
        private Vector3 wanderTo;
        private bool isWandering = false;

        private void DrawDebug(AIAgent agent)
        {
            if (debug)
            {
                if (isIt) {
                    DebugUtil.DrawCircle(transform.position, transform.up, Color.yellow, safeRadius);
                    if (target != null) {
                        Debug.DrawLine(transform.position, target.transform.position, Color.red);
                    }
                }
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
            
            if (isIt) {
                output = Pursue(agent, output);
            } else if (isFrozen) {
                output = Freeze(agent, output);
            } else if (isTarget) {
                output = Flee(agent, output);
            } else {
                output = Idle(agent, output);
            }
            

            return output;
        }

        public void SetFreeze(bool state)
        {
            isFrozen = state;
            transform.Find("Freeze").gameObject.SetActive(state);
            if (state == true) {
                TagController.instance.LastFrozen = this;
                ClearTarget();
            }
        }

        public bool IsFrozen() {
            return isFrozen;
        }

        private SteeringOutput Pursue(AIAgent agent, SteeringOutput output) {
            if (target == null) {
                target = TagController.instance.GetClosestPlayer(this);
                if (target == null) {
                    // Game over - do a victory twirl
                    output.linear =  GetLoopedVectorToTarget(agent.transform.position, Vector3.zero);
                    output.angular = Quaternion.AngleAxis(GameConstants.TWIRL_SPEED, Vector3.up);
                    TagController.instance.ResetGame();
                    return output;
                } else {
                    target.isTarget = true;
                }
            }

            Vector3 desiredVelocity = GetLoopedVectorToTarget(agent.transform.position, target.transform.position);

            if (desiredVelocity.magnitude < tagRadius) {
                // Tag the player
                target.SetFreeze(true);
                ClearTarget();
                output.linear = -agent.Velocity;
                output.angular = Quaternion.identity;
                return output;
            }

            // Set the destintion to the target's expected location
            output.linear = GetPredictedLocation(agent, target);

            // Look where you're going
            Vector3 from = Vector3.ProjectOnPlane(agent.transform.forward, Vector3.up);
            Vector3 to = Quaternion.LookRotation(output.linear) * Vector3.forward;
            float angleY = Vector3.SignedAngle(from, to, Vector3.up);
            output.angular = Quaternion.AngleAxis(angleY, Vector3.up);

            return output;
        }

        private SteeringOutput Flee(AIAgent agent, SteeringOutput output) {
            output.linear = GetLoopedVectorToTarget(It.transform.position, agent.transform.position);
            
            // Look where you're going
            Vector3 from = Vector3.ProjectOnPlane(agent.transform.forward, Vector3.up);
            Vector3 to = Quaternion.LookRotation(output.linear) * Vector3.forward;
            float angleY = Vector3.SignedAngle(from, to, Vector3.up);
            output.angular = Quaternion.AngleAxis(angleY, Vector3.up);

            return output;
        }

        private SteeringOutput Idle(AIAgent agent, SteeringOutput output) {
            if (isWandering) {
                Vector3 wanderToVelocity = GetLoopedVectorToTarget(agent.transform.position, wanderTo);
                if (wanderToVelocity.magnitude < tagRadius) {
                    isWandering = false;
                    output.linear = -agent.Velocity;
                    output.angular = Quaternion.identity;
                    return output;
                } else {
                    output.linear = wanderToVelocity;

                    // Look where you're going
                    Vector3 wanderFrom = Vector3.ProjectOnPlane(agent.transform.forward, Vector3.up);
                    Vector3 wanderTo = Quaternion.LookRotation(output.linear) * Vector3.forward;
                    float wanderAngleY = Vector3.SignedAngle(wanderFrom, wanderTo, Vector3.up);
                    output.angular = Quaternion.AngleAxis(wanderAngleY, Vector3.up);

                    return output;
                }

            }
            if (target == null) {
                target = TagController.instance.GetClosestFrozenPlayer(this);
                if (target == null) {
                    // Nobody to unfreeze. Fart around!
                    isWandering = true;
                    wanderTo = new Vector3(
                        Random.Range(-GameConstants.STAGE_WIDTH / 2, GameConstants.STAGE_WIDTH / 2), 
                        0, 
                        Random.Range(GameConstants.STAGE_BOTTOM, GameConstants.STAGE_TOP)
                    );

                    output.linear =  -agent.Velocity;
                    output.angular = Quaternion.identity;
                    return output;
                } else {
                    target.isTarget = true;
                }
            }

            Vector3 desiredVelocity = GetLoopedVectorToTarget(agent.transform.position, target.transform.position);

            if (desiredVelocity.magnitude < tagRadius) {
                // Unfreeze the player
                target.SetFreeze(false);
                ClearTarget();
                output.linear = -agent.Velocity;
                output.angular = Quaternion.identity;
                return output;
            }

            output.linear = desiredVelocity;
            
            // Look where you're going
            Vector3 from = Vector3.ProjectOnPlane(agent.transform.forward, Vector3.up);
            Vector3 to = Quaternion.LookRotation(output.linear) * Vector3.forward;
            float angleY = Vector3.SignedAngle(from, to, Vector3.up);
            output.angular = Quaternion.AngleAxis(angleY, Vector3.up);

            return output;
        }

        private void ClearTarget() {
            if (target == null) {
                return;
            }
            target.isTarget = false;
            target = null;
        }

        private SteeringOutput Freeze(AIAgent agent, SteeringOutput output) {
            output.linear =  -agent.Velocity;
            output.angular = Quaternion.identity;
            return output;
        }

        // Get velocity to target, factoring for looping terrain
        private Vector3 GetLoopedVectorToTarget(Vector3 start, Vector3 end) {
            Vector3 loopedPosition;
            if (end.x < 0) {
                loopedPosition = new Vector3(end.x + GameConstants.STAGE_WIDTH, end.y, end.z);
            } else {
                loopedPosition = new Vector3(end.x - GameConstants.STAGE_WIDTH, end.y, end.z);
            }

            Vector3 directVelocity = end - start;
            Vector3 loopedVelocity = loopedPosition - start;

            if (directVelocity.magnitude < loopedVelocity.magnitude) {
                return directVelocity;
            } else {
                return loopedVelocity;
            }
        }

        // Return the expected location of a target in the time it will take to catch up to that target
        private Vector3 GetPredictedLocation(AIAgent self, TagPlayer target) {
            // No need to calculate distance / time since both agents have the same max speed
            Vector3 distance = GetLoopedVectorToTarget(self.transform.position, target.transform.position);
            return ClampPositionToStage(target.transform.forward * distance.magnitude);
        }

        // Given a position, loop its x value to ensure it is in the stage
        private Vector3 ClampPositionToStage(Vector3 position) {
            if (position.x < -GameConstants.STAGE_WIDTH / 2) {
                return new Vector3(position.x + GameConstants.STAGE_WIDTH, position.y, position.z);
            } else if (position.x > GameConstants.STAGE_WIDTH / 2) {
                return new Vector3(position.x - GameConstants.STAGE_WIDTH, position.y, position.z);
            }
            return position;
        }
    }
}
