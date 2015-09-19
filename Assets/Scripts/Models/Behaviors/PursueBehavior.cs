using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PursueBehavior : AgentBehavior
{
	// pursue nearest living agent
	public override bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		if(_myself.MoveInUse)
		{
			return false;
		}

		Action newMoveAction;
		Action newLookAction;

		Agent tempAgent;
		bool found = findNearestAgent(percepts, AgentPercept.LivingState.ALIVE, out tempAgent);

		if(found && !_myself.MoveInUse)
		{
			newMoveAction = new Action(Action.ActionType.MOVE_TOWARDS);
			newMoveAction.TargetPoint = (tempAgent.Location);

			newLookAction = new Action(Action.ActionType.TURN_TOWARDS);
			newLookAction.TargetPoint = (tempAgent.Location);

			this.currentPlans.Clear();
			this.currentPlans.Add(newMoveAction);
			this.currentPlans.Add(newLookAction);
			return true;
		}

		return false;
	}
}
