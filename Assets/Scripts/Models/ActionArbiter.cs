using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionArbiter {

	private static ActionArbiter _instance;

	private ActionArbiter(){}

	public static ActionArbiter Instance
	{
		get
		{
			if(_instance == null)
				_instance = new ActionArbiter();
			
			return _instance;
		}
	}

	//////////////////////////////////////////////////////////////////
	#region Arbitration parameters

	[SerializeField]
	private float _extractionDistance = 5.0f;
	public float ExtractionDistance	{ get { return _extractionDistance; } }

	#endregion Arbitration parameters
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Bookkeeping

	[SerializeField]
	private WorldMap _map = null;
	public WorldMap WorldMap { set { _map = value; } }
	
	#endregion Bookkeeping
	//////////////////////////////////////////////////////////////////

	public enum ActionType { CONVERT, EXTRACT };

	private List<ActionParameters> queuedActions = new List<ActionParameters>();

	//////////////////////////////////////////////////////////////////
	#region Interface

	// Order of actions determines which take precedence
	// e.g. currently, if you get Converted at the same time you're trying
	// to get Extracted, no luck. You're a zombie/corpse.
	public void resolveActions()
	{
		// priority 1 actions like Convert
		foreach(ActionParameters action in queuedActions)
		{
			switch( action.verb )
			{
				case ActionType.CONVERT:
					attemptConvert(action);
					break;
			}
		}

		// priority 2 actions like Extract
		foreach(ActionParameters action in queuedActions)
		{
			switch( action.verb )
			{
				case ActionType.EXTRACT:
					attemptExtract(action);
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
		float distance = Vector2.Distance(action.subject.Location,
		                                  action.directObject.Location);

		if( distance <= action.subject.ConvertRange )
		{
			// TODO: Is conversion guaranteed? Perhaps some just die, become corpses?
			// certainly possible to get multiple zombies attempting to convert in one cycle
			// so, prevent that
			if(action.directObject.LivingState == AgentPercept.LivingState.ALIVE)
			{	
				action.directObject.configureAs(Agent.AgentType.ZOMBIE, true);
				if(_map != null)
				{
					_map.agentCountChange(-1,1,0);
				}
			}
		}
	}

	private void attemptExtract(ActionParameters action)
	{
		// for now we assume you're in range
		if(action.subject.LivingState == AgentPercept.LivingState.ALIVE)
		{
			_map.extractAgent(action.subject);
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
