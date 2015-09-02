using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NecrophageBehavior : AgentBehavior
{
	// this value * arbiter's convert distance = radius inside which
	// the necrophage won't continue to move towards the corpse
	private static float necrophageRadiusMultiplier = 0.5f;

	// "pursue" nearest corpse
	public override bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		Action newMoveAction;
		Action newLookAction;

		Agent tempAgent;
		bool found = findNearestAgent(percepts, AgentPercept.LivingState.DEAD, out tempAgent);
		
		if(found && !myself.getMoveInUse() && !myself.getLookInUse())
		{
			float distance = Vector2.Distance(myself.getLocation(),tempAgent.getLocation());

			if(distance < ActionArbiter.Instance.getConvertDistance()*necrophageRadiusMultiplier)
			{
				newMoveAction = new Action(Action.ActionType.STAY);
			}
			else
			{
				newMoveAction = new Action(Action.ActionType.MOVE_TOWARDS);
				newMoveAction.setTargetPoint(tempAgent.getLocation());
			}

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
