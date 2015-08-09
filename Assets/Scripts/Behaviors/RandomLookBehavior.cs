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
		if(nextUpdate > Time.realtimeSinceStartup || myself.getLookInUse())
		{
			return false;
		}

		currentPlans.Clear();

		Action newAction = new Action(Action.ActionType.TURN_TO_DEGREES);
		newAction.setDirection(Random.Range(-180.0f, 180.0f));

		currentPlans.Add(newAction);
		myself.setLookInUse(true);

		nextUpdate += 0.5f + (Random.value * updateDelay);

		return true;
	}
}
