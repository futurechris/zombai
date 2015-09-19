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
		
		if(found && !_myself.MoveInUse && !_myself.LookInUse)
		{
			float distance = Vector2.Distance(_myself.Location,tempAgent.Location);

			if(distance < _myself.ConvertRange*necrophageRadiusMultiplier)
			{
				newMoveAction = new Action(Action.ActionType.STAY);
			}
			else
			{
				newMoveAction = new Action(Action.ActionType.MOVE_TOWARDS);
				newMoveAction.TargetPoint = (tempAgent.Location);
			}

			newLookAction = new Action(Action.ActionType.TURN_TOWARDS);
			newLookAction.TargetPoint = (tempAgent.Location);

			currentPlans.Clear();
			this.currentPlans.Add(newMoveAction);
			this.currentPlans.Add(newLookAction);

			return true;
		}

		return false;
	}
}
