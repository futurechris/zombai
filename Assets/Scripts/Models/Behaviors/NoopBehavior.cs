using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NoopBehavior : AgentBehavior
{
	public NoopBehavior()
	{
		currentPlans.Clear();
	}

	public override bool executePlanUpdate()
	{
		currentPlans.Clear();
		return true;
	}
}
