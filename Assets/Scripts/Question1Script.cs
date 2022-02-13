using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Question1Script : MonoBehaviour
{
    void Start()
    {
        // KINEMATIC
        Vector2 pc = new Vector2(3,6);
        Vector2 vc = new Vector2(2,3);
        Vector2 pt = new Vector2(5,4);
        float t = 0.25f;
        float mv = 3.6f;
        float am = 12.25f;

        Vector2 newLoc = pc;
        Vector2 targetDirection;
        float velocity = Mathf.Min(mv, vc.magnitude);

        for (int i = 0; i < 5; i++) {
            targetDirection = pt - newLoc;
            if (velocity * t > targetDirection.magnitude) {
                newLoc = pt;                                                    // Arrive at the target
            } else {
                newLoc = newLoc + (targetDirection.normalized * velocity * t);  // Move towards the target
            }
            velocity = Mathf.Min(mv, velocity + (am * t));      // Increase velocity by acceleration, capped by max velocity
            
            Debug.Log("Step " + (i + 1) + " - x: " + newLoc.x + " y: " + newLoc.y);
        }
        
        Debug.Log("///////////////////////////////////////////");


        // STEERING
        newLoc = pc;
        Vector2 actualDirection = vc;

        for (int i = 0; i < 5; i++) {
            targetDirection = pt - newLoc;
            if (targetDirection.magnitude > 1) {
                actualDirection += targetDirection.normalized * am * t;
                actualDirection = Vector2.ClampMagnitude(actualDirection, mv);
                newLoc += actualDirection * t;
            } else {
                newLoc = pt;
            }
            Debug.Log("Step " + (i + 1) + " - x: " + newLoc.x + " y: " + newLoc.y);
        }

        // KINEMATIC ARRIVE
        newLoc = pc;

        float t2t = 0.55f;
        float r = 0.5f;

        for (int i = 0; i < 5; i++) {
            targetDirection = pt - newLoc;
            if (targetDirection.magnitude < r) {
                newLoc = newLoc + (targetDirection.normalized * Mathf.Min(velocity, targetDirection.magnitude / t2t) * t);   // Arrive at the target
            } else {
                newLoc = newLoc + (targetDirection.normalized * velocity * t);  // Move towards the target
            }
            velocity = Mathf.Min(mv, velocity + (am * t));      // Increase velocity by acceleration, capped by max velocity
            
            Debug.Log("Step " + (i + 1) + " - x: " + newLoc.x + " y: " + newLoc.y);
        }

        // STEERING ARRIVE
        newLoc = pc;
        actualDirection = vc;

        t2t = 0.5f;
        float ra = 0.2f;
        float rs = 1.5f;

        for (int i = 0; i < 5; i++) {
            targetDirection = pt - newLoc;
            if (targetDirection.magnitude > rs) {
                actualDirection += targetDirection * am * t;
                actualDirection = Vector2.ClampMagnitude(actualDirection, mv);
                newLoc += actualDirection * t;
            } else if (targetDirection.magnitude > ra) {
                actualDirection += Vector2.ClampMagnitude((targetDirection - actualDirection) / t2t, am) * t;
                newLoc += actualDirection * t;
            }
            
            Debug.Log("Step " + (i + 1) + " - x: " + newLoc.x + " y: " + newLoc.y);
        }
    }
}
