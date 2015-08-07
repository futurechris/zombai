using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PursueBehavior : AgentBehavior
{
	float updateDelay = 0.5f;
	float nextUpdate = 0;

	// pursue nearest living agent
	// otherwise wander aimlessly
	public override bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		currentPlans.Clear();
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

		if(preyDistance < float.MaxValue && !myself.getMoveInUse())
		{
			newAction = new Action(Action.ActionType.MOVE_TOWARDS);
			newAction.setTargetPoint(preyPosition);
			this.currentPlans.Add(newAction);

			nextUpdate = Time.realtimeSinceStartup + updateDelay;
			myself.setMoveInUse(true);
			return true;
		}

		return false;
	}
}
