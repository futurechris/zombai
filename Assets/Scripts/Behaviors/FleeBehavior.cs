using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FleeBehavior : AgentBehavior
{
	float updateDelay = 0.5f;
	float nextUpdate = 0;

	// flee from nearest undead
	// otherwise, wander aimlessly
	public override void updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		if(nextUpdate > Time.realtimeSinceStartup)
		{
			return;
		}

		Action newAction;

		float enemyDistance = float.MaxValue;
		Vector2 enemyPosition = Vector2.zero;
		float tempDistance = 0.0f;
		for(int i=0; i<percepts.Count; i++)
		{
			if(percepts[i].living == AgentPercept.LivingState.UNDEAD )
			{
				tempDistance = Vector2.Distance(percepts[i].locOne, myself.getLocation());
				if( tempDistance < enemyDistance)
				{
					enemyDistance = tempDistance;
					enemyPosition = percepts[i].locOne;
				}
			}
		}
		
		if(enemyDistance < float.MaxValue)
		{
			Vector2 mirror = myself.getLocation() + (myself.getLocation() - enemyPosition);
			newAction = new Action(Action.ActionType.MOVE_TOWARDS);
			newAction.setTargetPoint(mirror);
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
