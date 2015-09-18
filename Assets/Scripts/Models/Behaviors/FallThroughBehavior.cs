using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FallThroughBehavior : AgentBehavior
{
	private List<AgentBehavior> behaviorList = new List<AgentBehavior>();

	public void addBehavior(AgentBehavior newBehavior)
	{
		behaviorList.Add(newBehavior);
		newBehavior.setAgent(myself);
	}

	public AgentBehavior getBehaviorAt(int index)
	{
		return behaviorList[index];
	}

	public void removeBehaviorAt(int index)
	{
		behaviorList.RemoveAt(index);
	}

	public override void setAgent(Agent newAgent)
	{
		myself = newAgent;

		for(int i=0; i<this.behaviorList.Count; i++)
		{
			behaviorList[i].setAgent(myself);
		}
	}

	public override bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		bool planCleared = false;
		for(int i=0; i<this.behaviorList.Count; i++)
		{
			if(behaviorList[i].updatePlan(percepts, allottedWorkUnits))
			{
				if(!planCleared)
				{
					currentPlans.Clear();
					planCleared = true;
				}

				List<Action> tempPlans = behaviorList[i].getCurrentPlans();
				for(int planIdx=0; planIdx<tempPlans.Count; planIdx++)
				{
					currentPlans.Add(tempPlans[planIdx]);
					myself.setLookInUse(myself.getLookInUse() || tempPlans[planIdx].getUsingLook());
					myself.setMoveInUse(myself.getMoveInUse() || tempPlans[planIdx].getUsingMove());
				}
			}
		}
		return false;
	}
}
