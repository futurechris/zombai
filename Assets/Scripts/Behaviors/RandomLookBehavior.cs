using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomLookBehavior : AgentBehavior {
	public override bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		if(myself.getLookInUse())
		{
			return false;
		}
		currentPlans.Clear();

		Action newAction = new Action(Action.ActionType.TURN_TO_DEGREES);
		newAction.setDirection(Random.Range(-180.0f, 180.0f));
		currentPlans.Add(newAction);
		myself.setLookInUse(true);

		return true;
	}
}
