using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombifyBehavior : AgentBehavior {

	// TODO: Decide what faculties this behavior uses.
	public override bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		// If you're within ActionArbiter.convertDistance of a brain, try to grab't!
		// Can only grab one though, so grab the closest one.
		Action newAction;
		
		float preyDistance = float.MaxValue;
		Vector2 preyPosition = Vector2.zero;
		Agent preyAgent = null;
		float tempDistance = 0.0f;
		for(int i=0; i<percepts.Count; i++)
		{
			if(percepts[i].living == AgentPercept.LivingState.ALIVE )
			{
				tempDistance = Vector2.Distance(percepts[i].locOne, myself.getLocation());

				if( tempDistance < preyDistance && (percepts[i].perceivedAgent!=null))
				{
					preyDistance = tempDistance;
					preyPosition = percepts[i].locOne;
					preyAgent = percepts[i].perceivedAgent;
				}
			}
		}
		
		if(preyDistance < float.MaxValue && preyAgent != null)
		{
			newAction = new Action(Action.ActionType.CONVERT);
			newAction.setTargetAgent(preyAgent);
			
			this.currentPlans.Clear();
			this.currentPlans.Add(newAction);
			
			return true;
		}
		
		return false;
	}
}
