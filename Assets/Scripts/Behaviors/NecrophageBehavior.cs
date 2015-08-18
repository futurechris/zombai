using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NecrophageBehavior : AgentBehavior
{
	// "pursue" nearest corpse
	public override bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		Action newMoveAction;
		Action newLookAction;

		Agent tempAgent;
		bool found = findNearestAgent(percepts, AgentPercept.LivingState.DEAD, out tempAgent);
		
		if(found && !myself.getMoveInUse())
		{
			newMoveAction = new Action(Action.ActionType.MOVE_TOWARDS);
			newMoveAction.setTargetPoint(tempAgent.getLocation());
			newLookAction = new Action(Action.ActionType.TURN_TOWARDS);
			newLookAction.setTargetPoint(tempAgent.getLocation());

			currentPlans.Clear();
			this.currentPlans.Add(newMoveAction);
			this.currentPlans.Add(newLookAction);

			return true;
		}

		return false;
	}
}
