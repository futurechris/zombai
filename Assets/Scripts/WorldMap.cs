using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldMap : MonoBehaviour
{

	// <agent.guid, agent>
	public Dictionary<System.Guid, Agent> agents = new Dictionary<System.Guid, Agent>();

	// <agent.guid, agent's most recent planned action
	public Dictionary<System.Guid, Action> actions = new Dictionary<System.Guid, Action>();

	// Obv. just a PH, but this is a list of the rectangular (axis-aligned) buildings
	//   filling up the world.
	public List<Rect> buildings = new List<Rect>();

	//////////////////////////////////////////////////////////////////
	#region MonoBehaviour methods

	void Start ()
	{
		initializeWorld(500,500);
		populateWorld(1,100);
	}

	void Update ()
	{
		// 1. If world is paused, exit early
		// 2. Else grab deltaTime, iterate over actions, run them for that duration

	}
	#endregion MonoBehaviour methods
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region World initialization, generation, etc.

	// For now, just generates buildings
	// width/height in ... pixels?
	private void initializeWorld(int width, int height)
	{
		// Probably get something closer to a city by carving streets out 
		//   rather than trying to fill with boxes.
	}


	// Create agents, give them behaviors and locations, turn them loose.
	private void populateWorld(int numLiving, int numUndead)
	{
		// Initially, just going to test this with
	}

	#endregion World initialization, generation, etc.
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Agent-World Interactions

	public void registerAction(System.Guid agentGuid, Action newAction)
	{

	}

	// Agent perceives world through this function alone.
	public List<AgentPercept> getPercepts(Agent agent)
	{

	}

	#endregion Agent-World Interactions
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Map rendering
	#endregion Map rendering
	//////////////////////////////////////////////////////////////////
}
