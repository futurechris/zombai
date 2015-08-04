using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PursueBehavior : AgentBehavior
{
	float updateDelay = 0.5f;
	float nextUpdate = 0;

	// pursue nearest living agent
	// otherwise wander aimlessly
	public override void updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		if(nextUpdate > Time.realtimeSinceStartup)
		{
			return;
		}

		Action newAction;

		float preyDistance = float.MaxValue;
		Vector2 preyPosition = Vector2.zero;
		float tempDistance = 0.0f;
		for(int i=0; i<percepts.Count; i++)
		{
			if(percepts[i].living == AgentPercept.LivingState.ALIVE )
			{
				tempDistance = Vector2.Distance(percepts[i].locOne, myself.getLocation());
				if( tempDistance < preyDistance)
				{
					preyDistance = tempDistance;
					preyPosition = percepts[i].locOne;
				}
			}
		}

		if(preyDistance <  float.MaxValue)
		{
			newAction = new Action(Action.ActionType.MOVE_TOWARDS);
			newAction.setTargetPoint(preyPosition);
			this.currentPlan = newAction;

			nextUpdate = Time.realtimeSinceStartup + updateDelay;
			return;
		}

		// For now "wander aimlessly" means randomWalkBehavior
		
		// pick a random angle in radians and walk in that direction
		float a = Random.Range(0, 2.0f*Mathf.PI);
		
		float distance = 20; // unimportant, won't get that far in the small timesteps allowed.
		float targetX = distance * Mathf.Cos(a);
		float targetY = distance * Mathf.Sin(a);
		
		newAction = new Action(Action.ActionType.MOVE_TOWARDS);
		newAction.setTargetPoint(myself.getLocation() + new Vector2(targetX,targetY));
		
		this.currentPlan = newAction;
		
		nextUpdate = Time.realtimeSinceStartup + updateDelay;
	}
}
