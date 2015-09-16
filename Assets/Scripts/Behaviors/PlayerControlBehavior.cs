using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerControlBehavior : AgentBehavior
{
	// Get keyboard/touch input and convert it into a "plan"
	public override bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		if(myself.getMoveInUse())
		{
			return false;
		}
		
		Action newMoveAction;
		Action newLookAction;

		if(!myself.getMoveInUse())
		{
			float hAxis = Input.GetAxis("Horizontal");
			float vAxis = Input.GetAxis("Vertical");

			if(hAxis != 0 || vAxis != 0)
			{
				Vector2 targetPoint = myself.getLocation()+new Vector2(hAxis/2.0f, vAxis/2.0f);
				Vector2 lookTowards = Vector2.zero;
				
				newMoveAction = new Action(Action.ActionType.MOVE_TOWARDS);
				newMoveAction.setTargetPoint(targetPoint);
				
				newLookAction = new Action(Action.ActionType.TURN_TOWARDS);
				newLookAction.setTargetPoint(targetPoint);
				
				this.currentPlans.Clear();
				this.currentPlans.Add(newMoveAction);
				this.currentPlans.Add(newLookAction);
				return true;
			}

		}
		
		return false;
	}
}
