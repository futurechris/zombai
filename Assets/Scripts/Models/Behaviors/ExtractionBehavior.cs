using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExtractionBehavior : AgentBehavior {
	
	public override bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		Action newAction;
		
		AgentPercept tempPercept;
		bool found = findNearestPercept(percepts, AgentPercept.LivingState.INANIMATE, AgentPercept.PerceptType.EXTRACT, out tempPercept);
		
		if(found)
		{
			float extractDistance = Vector2.Distance(_myself.Location, tempPercept.locOne);
			if(extractDistance < ActionArbiter.Instance.ExtractionDistance)
			{
				newAction = new Action(Action.ActionType.EXTRACT);
				newAction.TargetPoint = (tempPercept.locOne);
				
				this.currentPlans.Clear();
				this.currentPlans.Add(newAction);
				
				return true;
			}
			else if(!_myself.MoveInUse)
			{
				newAction = new Action(Action.ActionType.MOVE_TOWARDS);
				newAction.TargetPoint = (tempPercept.locOne);

				this.currentPlans.Clear();
				this.currentPlans.Add(newAction);

				return true;
			}
		}
		
		return false;
	}
}
