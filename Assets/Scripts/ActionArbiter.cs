using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionArbiter {

	//////////////////////////////////////////////////////////////////
	#region Arbitration parameters

	private float convertDistance = 2.0f;

	#endregion Arbitration parameters
	//////////////////////////////////////////////////////////////////
	public enum ActionType { CONVERT };

	private List<ActionParameters> queuedActions = new List<ActionParameters>();

	//////////////////////////////////////////////////////////////////
	#region Interface

	public void resolveActions()
	{
		foreach(ActionParameters action in queuedActions)
		{
			switch( action.verb )
			{
				case ActionType.CONVERT:
					attemptConvert(action);
					break;
			}
		}

		queuedActions.Clear();
	}

	public void requestAction(Agent initiator, Agent target, ActionType verb)
	{
		queuedActions.Add(new ActionParameters(initiator,target,verb));
	}

	#endregion Interface
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Action implementations


	private void attemptConvert(ActionParameters action)
	{
		float distance = Vector2.Distance(action.subject.getLocation(),
		                                  action.directObject.getLocation());
		if( distance <= convertDistance )
		{
			// TODO: Is conversion guaranteed? Perhaps some just die, become corpses?
			action.directObject.configureAs(Agent.AgentType.ZOMBIE);
		}
	}

	#endregion Action implementations
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region ActionParameters class
	private class ActionParameters
	{
		public Agent subject = null;
		public Agent directObject = null;
		public ActionType verb = ActionType.CONVERT;

		public ActionParameters(Agent newSubj, Agent newDO, ActionType newVerb)
		{
			subject = newSubj;
			directObject = newDO;
			verb = newVerb;
		}
	}
	#endregion ActionParameters class
	//////////////////////////////////////////////////////////////////
}
