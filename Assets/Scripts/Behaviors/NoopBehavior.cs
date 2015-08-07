using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NoopBehavior : AgentBehavior
{
	public NoopBehavior()
	{
		currentPlans.Clear();
	}

	public override bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		currentPlans.Clear();
		return true;
	}
}
