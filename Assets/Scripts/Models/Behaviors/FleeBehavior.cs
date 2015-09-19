using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FleeBehavior : AgentBehavior
{
	// flee from nearest undead
	public override bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		if(_myself.MoveInUse)
		{
			return false;
		}

		Action newAction;

		Agent nearestEnemy;
		bool found = findNearestAgent(percepts, AgentPercept.LivingState.UNDEAD, out nearestEnemy);
		
		if(found && !_myself.MoveInUse)
		{
			Vector2 mirror = _myself.Location + (_myself.Location - nearestEnemy.Location);
			newAction = new Action(Action.ActionType.MOVE_TOWARDS);
			newAction.TargetPoint = (mirror);

			this.currentPlans.Clear();
			this.currentPlans.Add(newAction);

			return true;
		}

		return false;
	}

}
