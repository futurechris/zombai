using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomLookBehavior : AgentBehavior
{
	float updateDelay = 1.0f;
	float nextUpdate = float.MinValue;
	float angle = 0.0f;

	public override bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		if(_myself.LookInUse)
		{
			return false;
		}

		// only change the "plan" every so often.
		if(nextUpdate > Time.time)
		{
			return true;
		}


		Action newAction = new Action(Action.ActionType.TURN_TO_DEGREES);
		newAction.Direction = (Random.Range(-180.0f, 180.0f));
		
		currentPlans.Clear();
		currentPlans.Add(newAction);
		
		nextUpdate = Time.time + 0.5f + (Random.value * updateDelay);


		return true;
	}
}
