using System.Collections;

public class AgentBehavior
{
	protected Action currentPlan = new Action(); // new Action() defaults to a NOOP ('STAY')

	public Action getCurrentPlan()
	{
		if(currentPlan == null)
		{
			currentPlan = new Action();
		}
		return currentPlan;
	}

	public virtual void updatePlan(int allottedWorkUnits) {}
}
