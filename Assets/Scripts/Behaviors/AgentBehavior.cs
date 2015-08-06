using System.Collections;
using System.Collections.Generic;

public class AgentBehavior
{
	protected Action currentPlan = new Action(); // new Action() defaults to a NOOP ('STAY')

	protected Agent myself;

	public Action getCurrentPlan()
	{
		if(currentPlan == null)
		{
			currentPlan = new Action();
		}
		return currentPlan;
	}

	public virtual void setAgent(Agent newAgent)
	{
		myself = newAgent;
	}

	public virtual bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits) {return true;}


}
