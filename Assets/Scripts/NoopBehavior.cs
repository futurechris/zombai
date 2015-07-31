using UnityEngine;
using System.Collections;

public class NoopBehavior : AgentBehavior
{
	public NoopBehavior()
	{
		currentPlan = new Action(Action.ActionType.STAY);
	}

	public virtual void updatePlan(int allottedWorkUnits) {}
}
