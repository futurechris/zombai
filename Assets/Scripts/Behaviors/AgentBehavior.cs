using System.Collections;
using System.Collections.Generic;

public class AgentBehavior
{
	protected List<Action> currentPlans = new List<Action>();

	protected Agent myself;

	public List<Action> getCurrentPlans()
	{
		return currentPlans;
	}

	public virtual void setAgent(Agent newAgent)
	{
		myself = newAgent;
	}

	public virtual bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		currentPlans.Clear();
		return true;
	}


}
