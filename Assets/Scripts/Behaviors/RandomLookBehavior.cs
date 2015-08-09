using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomLookBehavior : AgentBehavior
{
	float updateDelay = 1.0f;
	float nextUpdate = 0;
	float angle = 0.0f;

	public override bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		if(myself.getLookInUse())
		{
			return false;
		}

		// only change the "plan" every so often.
		if(nextUpdate > Time.realtimeSinceStartup)
		{
			return true;
		}


		Action newAction = new Action(Action.ActionType.TURN_TO_DEGREES);
		newAction.setDirection(Random.Range(-180.0f, 180.0f));
		
		currentPlans.Clear();
		currentPlans.Add(newAction);
		
		nextUpdate += 0.5f + (Random.value * updateDelay);


		return true;
	}
}
