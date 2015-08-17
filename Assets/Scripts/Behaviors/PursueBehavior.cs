using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PursueBehavior : AgentBehavior
{
	// pursue nearest living agent
	public override bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		if(myself.getMoveInUse())
		{
			return false;
		}

		Action newMoveAction;
		Action newLookAction;

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
			newMoveAction = new Action(Action.ActionType.MOVE_TOWARDS);
			newMoveAction.setTargetPoint(preyPosition);

			newLookAction = new Action(Action.ActionType.TURN_TOWARDS);
			newLookAction.setTargetPoint(preyPosition);

			this.currentPlans.Clear();
			this.currentPlans.Add(newMoveAction);
			this.currentPlans.Add(newLookAction);
			return true;
		}

		return false;
	}
}
