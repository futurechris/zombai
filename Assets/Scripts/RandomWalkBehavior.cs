using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomWalkBehavior : AgentBehavior {

	float updateDelay = 0.25f;
	float nextUpdate = 0;

	public override void updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		if(nextUpdate > Time.realtimeSinceStartup)
		{
			return;
		}

		// pick a random angle in radians and walk in that direction
		float a = Random.Range(0, 2.0f*Mathf.PI);

		float distance = 20; // unimportant, won't get that far in the small timesteps allowed.
		float targetX = distance * Mathf.Cos(a);
		float targetY = distance * Mathf.Sin(a);

		Action newAction = new Action(Action.ActionType.MOVE_TOWARDS);
		newAction.setTargetPoint(new Vector2(targetX,targetY));

		this.currentPlan = newAction;

		nextUpdate += updateDelay;
	}
}
