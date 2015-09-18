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
			float extractDistance = Vector2.Distance(myself.getLocation(), tempPercept.locOne);
			if(extractDistance < ActionArbiter.Instance.ExtractionDistance)
			{
				newAction = new Action(Action.ActionType.EXTRACT);
				newAction.setTargetPoint(tempPercept.locOne);
				
				this.currentPlans.Clear();
				this.currentPlans.Add(newAction);
				
				return true;
			}
			else if(!myself.getMoveInUse())
			{
				newAction = new Action(Action.ActionType.MOVE_TOWARDS);
				newAction.setTargetPoint(tempPercept.locOne);

				this.currentPlans.Clear();
				this.currentPlans.Add(newAction);

				return true;
			}
		}
		
		return false;
	}
}
