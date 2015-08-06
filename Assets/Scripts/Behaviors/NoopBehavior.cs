using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NoopBehavior : AgentBehavior
{
	public NoopBehavior()
	{
		currentPlan = new Action(Action.ActionType.STAY);
	}

	public override bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits) {return true;}
}
