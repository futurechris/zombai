using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombifyBehavior : AgentBehavior {

	public override bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		// If you're within convertRange of a brain, try to grab't!
		// Can only grab one though, so grab the closest one.
		Action newAction;
		
		Agent tempAgent;
		bool found = findNearestAgent(percepts, AgentPercept.LivingState.ALIVE, out tempAgent);
		
		if(found)
		{
			float preyDistance = Vector2.Distance(_myself.Location, tempAgent.Location);
			if(preyDistance < _myself.ConvertRange)
			{
				newAction = new Action(Action.ActionType.CONVERT);
				newAction.TargetAgent = (tempAgent);
				
				this.currentPlans.Clear();
				this.currentPlans.Add(newAction);
				
				return true;
			}
		}
		
		return false;
	}
}
