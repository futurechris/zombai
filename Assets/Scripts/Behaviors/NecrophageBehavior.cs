using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NecrophageBehavior : AgentBehavior
{
	// "pursue" nearest corpse
	public override bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		Action newAction;
		
		float preyDistance = float.MaxValue;
		Vector2 preyPosition = Vector2.zero;
		float tempDistance = 0.0f;
		for(int i=0; i<percepts.Count; i++)
		{
			if(percepts[i].living == AgentPercept.LivingState.DEAD )
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

			currentPlans.Clear();
			this.currentPlans.Add(newAction);
			
			myself.setMoveInUse(true);
			return true;
		}
		
		return false;
	}
}
