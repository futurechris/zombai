using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldMap : MonoBehaviour
{
	//////////////////////////////////////////////////////////////////
	#region Parameters & properties
	private int minimumStreetWidth = 5;

	public GameObject agentsGO;		// object agents are placed under - just organizational
	public GameObject structuresGO;	// ditto for structures

	public GameObject agentPrefab;
	public GameObject structurePrefab;

	#endregion Parameters & properties
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Map State
	private bool paused	= false;
	#endregion Map State
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Bookkeeping
	// <agent.guid, agent>
	private Dictionary<System.Guid, Agent> agents = new Dictionary<System.Guid, Agent>();

	// <agent.guid, agent's most recent planned action
	// For now these are primitive. Eventually likely want compound actions.
	private Dictionary<System.Guid, Action> actions = new Dictionary<System.Guid, Action>();

	// Obv. just a PH, but this is a list of the rectangular (axis-aligned) rectangular buildings
	//   filling up the world.
	private List<Rect> structures = new List<Rect>();

	private float worldWidth = 0;
	private float worldHeight = 0;

	#endregion Bookkeeping
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region MonoBehaviour methods

	void Start ()
	{
		// hard-coding size for now - camera in scene is set to work for this size as well.
		initializeWorld(1024,768);
		populateWorld(1,100);
		instantiateWorld();
	}

	void Update ()
	{
		// 1. If world is paused, exit early
		if(paused)
		{
			return;
		}

		// 2. Else grab deltaTime, iterate over actions, run them for that duration
		float storedDeltaTime = Time.deltaTime;
		Action tempAction;

		foreach(System.Guid guid in actions.Keys)
		{
			if(actions.TryGetValue(guid, out tempAction))
			{
				executeAction(guid, tempAction, storedDeltaTime);
			}
		}

		// 3. Lastly, update any remaining rendery bits.
	}
	#endregion MonoBehaviour methods
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region World initialization, generation, etc.

	// For now, just generates rectangular buildings
	// width/height in ... pixels?
	private void initializeWorld(float mapWidth, float mapHeight)
	{
		worldWidth = mapWidth;
		worldHeight = mapHeight;

		// Probably get something closer to a city by carving streets out 
		//   rather than trying to fill with boxes.
		float streetWidth = Mathf.Max(minimumStreetWidth, Mathf.Floor(worldWidth * 0.01f));

		// But for current testing, just making some random rectangles.
		for(int i=0; i<250; i++)
		{
			float xDim = Random.Range(3.0f*streetWidth, 10.0f*streetWidth);
			float yDim = Random.Range(3.0f*streetWidth, 10.0f*streetWidth);

			float xPos = Random.Range(0.0f, worldWidth-xDim);
			float yPos = Random.Range(0.0f, worldHeight-yDim);

			structures.Add( new Rect(xPos,yPos, xDim,yDim) );
		}
	}
	
	// Create agents, give them behaviors and locations, turn them loose.
	private void populateWorld(int numLiving, int numUndead)
	{
		GameObject tempGO;
		Agent tempAgent;
		Vector2 tempPosition = Vector2.zero;
		for(int i=0; i<numLiving; i++)
		{
			tempGO = GameObject.Instantiate(agentPrefab) as GameObject;
			tempGO.transform.parent = agentsGO.transform;

			tempPosition = getValidAgentPosition();
			tempGO.transform.localPosition = tempPosition;

			tempAgent = tempGO.GetComponent<Agent>();
			tempAgent.setAgentColor(Color.green);

			agents.Add(tempAgent.getGuid(), tempAgent);
		}

		for(int i=0; i<numUndead; i++)
		{
			tempGO = GameObject.Instantiate(agentPrefab) as GameObject;
			tempGO.transform.parent = agentsGO.transform;
			
			tempPosition = getValidAgentPosition();
			tempGO.transform.localPosition = tempPosition;
			
			tempAgent = tempGO.GetComponent<Agent>();
			tempAgent.setAgentColor(Color.magenta);
			
			agents.Add(tempAgent.getGuid(), tempAgent);
		}
	}

	private Vector2 getValidAgentPosition()
	{
		Vector2 testPos = new Vector2(Random.Range(0.0f,worldWidth), Random.Range(0.0f,worldHeight));
		bool valid = false;
		int attempts = 0;
		int maxAttempts = 100; // heh
		while(!valid && (attempts < maxAttempts)){
			testPos = new Vector2(Random.Range(0.0f,worldWidth), Random.Range(0.0f,worldHeight));
			valid = true;
			for(int i=0; i<structures.Count; i++)
			{
				if(structures[i].Contains(testPos))
				{
					valid = false;
				}
			}
			attempts++;
		}
		if(attempts >= maxAttempts)
		{
			Debug.LogError("Exceeded maximum attempts");
		}
		return testPos;
	}

	private void instantiateWorld()
	{
		// Structures
		// loop over structures, instantiate prefabs, position them.
		foreach(Rect rect in structures)
		{
			GameObject structGO = GameObject.Instantiate(structurePrefab) as GameObject;
			structGO.transform.parent = structuresGO.transform;

			structGO.transform.localScale = new Vector3(rect.width, rect.height, 1.0f);
			structGO.transform.position = new Vector3(rect.x+(rect.width/2.0f), rect.y+(rect.height/2.0f), 5.0f);
		}


		// Agents
		// loop over agents, instantiate prefabs, configure them according to initial populate method

		// ...
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
		return new List<AgentPercept>();
	}

	#endregion Agent-World Interactions
	//////////////////////////////////////////////////////////////////


	//////////////////////////////////////////////////////////////////
	#region Map updates
	private void executeAction(System.Guid agentGuid, Action toExecute, float duration)
	{
		switch(toExecute.getActionType())
		{
			case Action.ActionType.STAY:
				return;
			case Action.ActionType.MOVE_TOWARDS:
				return;
			case Action.ActionType.TURN_BY_DEGREES:
				return;
			case Action.ActionType.TURN_TOWARDS:
				return;
		}
	}


	#endregion Map updates
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Map rendering
	#endregion Map rendering
	//////////////////////////////////////////////////////////////////
}
