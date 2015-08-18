using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FleeBehavior : AgentBehavior
{
	// flee from nearest undead
	public override bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		if(myself.getMoveInUse())
		{
			return false;
		}

		Action newAction;

		Agent nearestEnemy;
		bool found = findNearestAgent(percepts, AgentPercept.LivingState.UNDEAD, out nearestEnemy);
		
		if(found && !myself.getMoveInUse())
		{
			Vector2 mirror = myself.getLocation() + (myself.getLocation() - nearestEnemy.getLocation());
			newAction = new Action(Action.ActionType.MOVE_TOWARDS);
			newAction.setTargetPoint(mirror);

			this.currentPlans.Clear();
			this.currentPlans.Add(newAction);

			return true;
		}

		return false;
	}

}
