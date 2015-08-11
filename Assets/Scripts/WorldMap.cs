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

	// Eventually these will be used to calculate how much time to allot to each agent's AI calcs
	private int	_targetFramerate = 15; // fps
	public 	int targetFramerate  = 15;

	// how many pixels should be moved per second for an agent on the go.
	private float moveSpeed = 10.0f;

	#endregion Parameters & properties
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Map State
	private bool paused	= false;
	#endregion Map State
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Bookkeeping

	private List<Agent> agents = new List<Agent>();
	private List<int> workUnits = new List<int>();

	// Obv. just a PH, but this is a list of the rectangular (axis-aligned) rectangular buildings
	//   filling up the world.
	private List<Rect> structures = new List<Rect>();

	private float worldWidth = 0;
	private float worldHeight = 0;
	
	#endregion Bookkeeping
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region MonoBehaviour methods & helpers

	void Start ()
	{
		targetFramerate = _targetFramerate;

		// hard-coding size for now - camera in scene is set to work for this size as well.
		initializeWorld(1024,768,1);
		instantiateWorld();
		populateWorld(10,10,200);
//		populateTestWorld();
	}

	void Update ()
	{
		// 0. Update parameters
		updateParameters();

		// 1. If world is paused, exit early
		if(paused)
		{
			return;
		}

		// 2. Else grab deltaTime, iterate over agents, ask them for action, run for deltaTime
		// Storing it here because documentation doesn't make it clear if this will change.
		// I don't expect so, but until I can verify experimentally, I'd like to ensure time
		// equity between all agents.
		float storedDeltaTime = Time.deltaTime;

		foreach(Agent agent in agents)
		{
			executeAction(agent, storedDeltaTime);
		}

		// 3. Update any remaining rendery bits.
		// ...

		// 4. Allocate time to agents to process percepts and update plans
		foreach(Agent agent in agents)
		{
			// Currently the '1' work unit is meaningless - agents will just do their little
			// calculation and be done with it.
			if(		agent.getIsAlive() == AgentPercept.LivingState.ALIVE
			   || 	agent.getIsAlive() == AgentPercept.LivingState.UNDEAD)
			{
				agent.setLookInUse(false);
				agent.setMoveInUse(false);
				agent.getBehavior().updatePlan( getPercepts(agent), 1 );
			}
		}
	}

	private void updateParameters()
	{
		if(targetFramerate != _targetFramerate)
		{
			_targetFramerate = targetFramerate;
		}
	}


	#endregion MonoBehaviour methods & helpers
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region World initialization, generation, etc.

	// For now, just generates rectangular buildings
	// width/height in ... pixels?
	private void initializeWorld(float mapWidth, float mapHeight, int numBuildings)
	{
		worldWidth = mapWidth;
		worldHeight = mapHeight;

		// Probably get something closer to a city by carving streets out 
		//   rather than trying to fill with boxes.
		float streetWidth = Mathf.Max(minimumStreetWidth, Mathf.Floor(worldWidth * 0.01f));

		// But for current testing, just making some random rectangles.
		for(int i=0; i<numBuildings; i++)
		{
			float xDim = Random.Range(3.0f*streetWidth, 10.0f*streetWidth);
			float yDim = Random.Range(3.0f*streetWidth, 10.0f*streetWidth);

			float xPos = Random.Range(0.0f, worldWidth-xDim);
			float yPos = Random.Range(0.0f, worldHeight-yDim);

			structures.Add( new Rect(xPos,yPos, xDim,yDim) );
		}
	}

	private void populateTestWorld()
	{
		GameObject humanGO = GameObject.Instantiate(agentPrefab) as GameObject;
		humanGO.transform.parent = agentsGO.transform;
		humanGO.name = "Living 1";

		Agent humanAgent = humanGO.GetComponent<Agent>();
		humanAgent.setAgentColor(Color.green);
		humanAgent.setLocation(new Vector2(worldWidth/2.0f, worldHeight/2.0f));
		humanAgent.setIsAlive(AgentPercept.LivingState.ALIVE);
		humanAgent.setSightRange(50.0f);
		humanAgent.setFieldOfView(120.0f);
		humanAgent.setDirection(0.0f);
		humanAgent.setSpeedMultiplier(1.15f);

		FallThroughBehavior ftb = new FallThroughBehavior();
		ftb.addBehavior( new NoopBehavior());
		humanAgent.setBehavior( ftb );
		agents.Add(humanAgent);

		GameObject zombieGO = GameObject.Instantiate(agentPrefab) as GameObject;
		zombieGO.transform.parent = agentsGO.transform;
		zombieGO.name = "Undead 1";

		Agent zombieAgent = zombieGO.GetComponent<Agent>();
		zombieAgent.setAgentColor(Color.magenta);
		zombieAgent.setLocation(new Vector2(worldWidth/2.0f + 100.0f, worldHeight/2.0f));
		zombieAgent.setIsAlive(AgentPercept.LivingState.UNDEAD);
		zombieAgent.setSightRange(100.0f);
		zombieAgent.setFieldOfView(360.0f);
		zombieAgent.setDirection(0.0f);
//		zombieAgent.setSpeedMultiplier(1.0f);
		
		FallThroughBehavior zombieFTB = new FallThroughBehavior();
		zombieFTB.addBehavior( new NoopBehavior());
		zombieAgent.setBehavior( zombieFTB );
		agents.Add( zombieAgent );

	}

	// Create agents, give them behaviors and locations, turn them loose.
	private void populateWorld(int numLiving, int numCorpses, int numUndead)
	{
		GameObject tempGO;
		Agent tempAgent;
		Vector2 tempPosition = Vector2.zero;
		AgentBehavior tempAgentBehavior;
		FallThroughBehavior tempFTB;
		for(int i=0; i<numLiving; i++)
		{
			tempGO = GameObject.Instantiate(agentPrefab) as GameObject;
			tempGO.transform.parent = agentsGO.transform;
			tempGO.name = "Living "+i;

			tempPosition = getValidAgentPosition();

			tempAgent = tempGO.GetComponent<Agent>();
			tempAgent.setAgentColor(Color.green);
			tempAgent.setLocation(tempPosition);
			tempAgent.setIsAlive(AgentPercept.LivingState.ALIVE);
			tempAgent.setSightRange(36.0f);
			tempAgent.setFieldOfView(180.0f); // roughly full range of vision
			tempAgent.setDirection(Random.Range(-180.0f, 180.0f));
			tempAgent.setSpeedMultiplier(1.15f);

			tempFTB = new FallThroughBehavior();
			tempFTB.addBehavior( new FleeBehavior() );
			tempFTB.addBehavior( new RandomWalkBehavior() );
			tempFTB.addBehavior( new RandomLookBehavior() );

			tempAgent.setBehavior(tempFTB);

			agents.Add(tempAgent);
		}

		for(int i=0; i<numUndead; i++)
		{
			tempGO = GameObject.Instantiate(agentPrefab) as GameObject;
			tempGO.transform.parent = agentsGO.transform;
			tempGO.name = "Undead "+i;
			
			tempPosition = getValidAgentPosition();

			tempAgent = tempGO.GetComponent<Agent>();
			tempAgent.setAgentColor(Color.magenta);
			tempAgent.setLocation(tempPosition);
			tempAgent.setIsAlive(AgentPercept.LivingState.UNDEAD);
			tempAgent.setSightRange(25.0f);
			tempAgent.setFieldOfView(90.0f);
			tempAgent.setDirection(Random.Range(-180.0f, 180.0f));

			tempFTB = new FallThroughBehavior();
			tempFTB.addBehavior( new PursueBehavior() );
			tempFTB.addBehavior( new NecrophageBehavior() );
			tempFTB.addBehavior( new RandomWalkBehavior() );
			tempFTB.addBehavior( new RandomLookBehavior() );

			tempAgent.setBehavior(tempFTB);

			agents.Add(tempAgent);
		}

		
		for(int i=0; i<numCorpses; i++)
		{
			tempGO = GameObject.Instantiate(agentPrefab) as GameObject;
			tempGO.transform.parent = agentsGO.transform;
			tempGO.name = "Corpse "+i;
			
			tempPosition = getValidAgentPosition();
			
			tempAgent = tempGO.GetComponent<Agent>();
			tempAgent.setAgentColor(Color.cyan);
			tempAgent.setLocation(tempPosition);
			tempAgent.setIsAlive(AgentPercept.LivingState.DEAD);
			tempAgent.setSightRange(0.0f);
			tempAgent.setFieldOfView(0.0f);
			tempAgent.setDirection(Random.Range(-180.0f, 180.0f));
			
			tempAgentBehavior = new NoopBehavior();
			tempAgent.setBehavior(tempAgentBehavior);
			
			agents.Add(tempAgent);
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
			valid = isValidPosition( testPos );
			attempts++;
		}
		if(attempts >= maxAttempts)
		{
			Debug.LogError("Exceeded maximum attempts");
		}
		return testPos;
	}

	private bool isValidPosition(Vector2 testPos)
	{
		if(		testPos.x < 0
		   || 	testPos.x >= worldWidth
		   ||	testPos.y < 0
		   ||	testPos.y >= worldHeight)
		{
			return false;
		}

		for(int i=0; i<structures.Count; i++)
		{
			if(structures[i].Contains(testPos))
			{
				return false;
			}
		}
		return true;
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
	}

	#endregion World initialization, generation, etc.
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Agent-World Interactions
	
	// Agent perceives world through this function alone.
	// N^2 or worse for now, oh well.
	private List<AgentPercept> getPercepts(Agent agent)
	{
		List<AgentPercept> apList = new List<AgentPercept>();

		AgentPercept tempPercept;

		for(int i=0; i<agents.Count; i++)
		{
			// For now not doing self-aware. No Skynet on *my* watch!
			if(agent.getGuid() == agents[i].getGuid())
			{
				continue;
			}
			if(canPerceivePosition(agent, agents[i].getLocation()))
			{
				tempPercept 		= new AgentPercept();
				tempPercept.type 	= AgentPercept.PerceptType.AGENT;
				tempPercept.locOne 	= agents[i].getLocation();
				tempPercept.living 	= agents[i].getIsAlive();
				apList.Add(tempPercept);
				tempPercept 		= null;
			}
		}

		List<AgentPercept> structureList = null;
		for(int i=0; i<structures.Count; i++)
		{
			structureList = canPerceiveStructure(agent, structures[i]);
			if(structureList != null)
			{
				apList.AddRange(structureList);
			}
			structureList = null;
		}


		return apList;
	}

	private bool canPerceivePosition(Agent who, Vector2 where)
	{
		// first, distance between who and where: <= vision distance?
		Vector2 delta = where - who.getLocation();
		
		if( delta.magnitude > who.getSightRange())
		{
			return false;
		}

		// second: calculate angle between who and where, compare to
		//		   agent's facing, see if it's within FOV
		float whoToWhereAngle = Mathf.Rad2Deg * Mathf.Atan2(delta.x, delta.y);
		float shortest = Mathf.Abs(Mathf.DeltaAngle(whoToWhereAngle, who.getDirection()));

		if(shortest <= (who.getFieldOfView()/2.0f))
		{
			return true;
		}

		return false;
	}

	private List<AgentPercept> canPerceiveStructure(Agent who, Rect what)
	{
		// TODO: Calculate chunks of walls that are visible, return those.
		return null;
	}

	private void executeAction(Agent agent, float duration)
	{
		List<Action> planList = agent.getBehavior().getCurrentPlans();
		for(int i=0; i<planList.Count; i++)
		{
			switch(planList[i].getActionType())
			{
				case Action.ActionType.STAY:
					break;
					
				case Action.ActionType.MOVE_TOWARDS:
					Vector2 newPoint = agent.getLocation() + (planList[i].getTargetPoint() - agent.getLocation()).normalized * duration * moveSpeed * agent.getSpeedMultiplier();
					
					if(isValidPosition(newPoint))
					{
						agent.setLocation(newPoint);
					}
					
					break;
					
				case Action.ActionType.TURN_BY_DEGREES:
					break;
					
				case Action.ActionType.TURN_TO_DEGREES:
					agent.setDirection( planList[i].getDirection() );
					break;
					
				case Action.ActionType.TURN_TOWARDS:
					break;
			}
		}
	}

	#endregion Agent-World Interactions
	//////////////////////////////////////////////////////////////////
}
