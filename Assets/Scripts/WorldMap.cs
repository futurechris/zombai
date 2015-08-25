using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldMap
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
	#region Bookkeeping
	
	private List<Agent> agents = new List<Agent>();
	private List<int> workUnits = new List<int>();

	// Obv. just a PH, but this is a list of the rectangular (axis-aligned) rectangular buildings
	//   filling up the world.
	private List<Rect> structures = new List<Rect>();

	private float worldWidth = 0;
	private float worldHeight = 0;

	private bool agentStateChanged = true;
	private int livingCount = 0;
	private int undeadCount = 0;
	private int corpseCount = 0;

	#endregion Bookkeeping
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region World initialization, generation, etc.

	// Horrible. Temporary. Will refactor rendering/worldmap data-ness out soon.
	public WorldMap(int width, int height, int buildingCount,
	                GameObject aContainerGO, GameObject sContainerGO,
	                GameObject aPrefab, GameObject sPrefab)
	{
		agentsGO = aContainerGO;
		structuresGO = sContainerGO;
		agentPrefab = aPrefab;
		structurePrefab = sPrefab;

		initializeWorld(width,height,buildingCount);
		instantiateWorld();
	}
	
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

	public void populateTestWorld()
	{
		GameObject humanGO = GameObject.Instantiate(agentPrefab) as GameObject;
		humanGO.transform.parent = agentsGO.transform;
		humanGO.name = "Living 1";

		Agent humanAgent = humanGO.GetComponent<Agent>();
		humanAgent.setAgentColor(Color.green);
		humanAgent.setLocation(new Vector2(worldWidth/2.0f + 40.0f, worldHeight/2.0f + -40.0f));
		humanAgent.configureAs(Agent.AgentType.HUMAN);
		humanAgent.setSightRange(50.0f);
		humanAgent.setFieldOfView(90.0f);
		humanAgent.setDirection(0.0f);
		humanAgent.setSpeedMultiplier(1.15f);

		FallThroughBehavior ftb = new FallThroughBehavior();
		ftb.addBehavior( new NoopBehavior());
		humanAgent.setBehavior( ftb );
		agents.Add(humanAgent);
		livingCount++;


		humanGO = GameObject.Instantiate(agentPrefab) as GameObject;
		humanGO.transform.parent = agentsGO.transform;
		humanGO.name = "Living 2";

		humanAgent = humanGO.GetComponent<Agent>();
		humanAgent.setAgentColor(Color.green);
//		humanAgent.setLocation(new Vector2(worldWidth/2.0f + 20f, worldHeight/2.0f + -34.64101616f)); // 30 degrees
		humanAgent.setLocation(new Vector2(worldWidth/2.0f, worldHeight/2.0f-40.0f));
		humanAgent.configureAs(Agent.AgentType.HUMAN);
		humanAgent.setSightRange(50.0f);
		humanAgent.setFieldOfView(90.0f);
		humanAgent.setDirection(0.0f);
		humanAgent.setSpeedMultiplier(1.15f);
		
		ftb = new FallThroughBehavior();
		ftb.addBehavior( new NoopBehavior());
		humanAgent.setBehavior( ftb );
		agents.Add(humanAgent);
		livingCount++;




		GameObject zombieGO = GameObject.Instantiate(agentPrefab) as GameObject;
		zombieGO.transform.parent = agentsGO.transform;
		zombieGO.name = "Undead 1";

		Agent zombieAgent = zombieGO.GetComponent<Agent>();
		zombieAgent.setAgentColor(Color.magenta);
		zombieAgent.setLocation(new Vector2(worldWidth/2.0f, worldHeight/2.0f));
		zombieAgent.configureAs(Agent.AgentType.ZOMBIE);
		zombieAgent.setSightRange(100.0f);
		zombieAgent.setFieldOfView(120.0f);
		zombieAgent.setDirection(0.0f);

		FallThroughBehavior tempFTB = new FallThroughBehavior();
		tempFTB.addBehavior( new ZombifyBehavior() );
		tempFTB.addBehavior( new PursueBehavior() );
		tempFTB.addBehavior( new NecrophageBehavior() );
		tempFTB.addBehavior( new WanderBehavior() );
		tempFTB.addBehavior( new RandomLookBehavior() );
		zombieAgent.setBehavior(tempFTB);
		
//		FallThroughBehavior zombieFTB = new FallThroughBehavior();
//		zombieFTB.addBehavior( new NoopBehavior());
//		zombieAgent.setBehavior( zombieFTB );

		agents.Add( zombieAgent );
		undeadCount++;
	}

	// Create agents, give them behaviors and locations, turn them loose.
	public void populateWorld(int numLiving, int numCorpses, int numUndead)
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
			tempAgent.configureAs(Agent.AgentType.HUMAN);
			tempAgent.setLocation(tempPosition);

			agents.Add(tempAgent);
			livingCount++;
		}

		for(int i=0; i<numUndead; i++)
		{
			tempGO = GameObject.Instantiate(agentPrefab) as GameObject;
			tempGO.transform.parent = agentsGO.transform;
			tempGO.name = "Undead "+i;
			
			tempPosition = getValidAgentPosition();

			tempAgent = tempGO.GetComponent<Agent>();
			tempAgent.configureAs(Agent.AgentType.ZOMBIE);
			tempAgent.setLocation(tempPosition);

			agents.Add(tempAgent);
			undeadCount++;
		}

		for(int i=0; i<numCorpses; i++)
		{
			tempGO = GameObject.Instantiate(agentPrefab) as GameObject;
			tempGO.transform.parent = agentsGO.transform;
			tempGO.name = "Corpse "+i;
			
			tempPosition = getValidAgentPosition();
			
			tempAgent = tempGO.GetComponent<Agent>();
			tempAgent.configureAs(Agent.AgentType.CORPSE);
			tempAgent.setLocation(tempPosition);

			agents.Add(tempAgent);
			corpseCount++;
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

	public bool isValidPosition(Vector2 testPos)
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
	public List<AgentPercept> getPercepts(Agent agent, float perfectVisionRange)
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
			if(canPerceivePosition(agent, agents[i].getLocation(), perfectVisionRange))
			{
				tempPercept 		= new AgentPercept();
				tempPercept.type 	= AgentPercept.PerceptType.AGENT;
				tempPercept.locOne 	= agents[i].getLocation();
				tempPercept.living 	= agents[i].getIsAlive();

				// TODO: Agent object should not be part of percept, fix!
				// See AgentPercept.perceivedAgent for more.
				tempPercept.perceivedAgent = agents[i];

				apList.Add(tempPercept);
				tempPercept 		= null;
			}
		}

		List<AgentPercept> structureList = null;
		for(int i=0; i<structures.Count; i++)
		{
			structureList = perceiveStructure(agent, structures[i]);
			if(structureList != null)
			{
				apList.AddRange(structureList);
			}
			structureList = null;
		}


		return apList;
	}

	public bool canPerceivePosition(Agent who, Vector2 where, float perfectVisionRange)
	{
		// first, distance between who and where: <= vision distance?
		Vector2 delta = where - who.getLocation();
		
		if( delta.magnitude > who.getSightRange())
		{
			return false;
		}

		// short-range hack
		if( delta.magnitude < perfectVisionRange)
		{
			return true;
		}

		// second: calculate angle between who and where, compare to
		//		   agent's facing, see if it's within FOV
		float whoToWhereAngle = 90 - Mathf.Rad2Deg * Mathf.Atan2(delta.x, delta.y);
		float shortest = Mathf.Abs(Mathf.DeltaAngle(whoToWhereAngle, who.getDirection()));

		if(shortest <= (who.getFieldOfView()/2.0f))
		{
			return true;
		}

		return false;
	}

	public List<AgentPercept> perceiveStructure(Agent who, Rect what)
	{
		// TODO: Calculate chunks of walls that are visible, return those.
		return null;
	}


	#endregion Agent-World Interactions
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Getters/Setters

	public List<Agent> getAgents()
	{
		return agents;
	}

	public int getLivingCount()
	{
		return livingCount;
	}

	public int getUndeadCount()
	{
		return undeadCount;
	}

	public int getCorpseCount()
	{
		return corpseCount;
	}

	public void agentCountChange(int deltaLiving, int deltaUndead, int deltaCorpse)
	{
		livingCount += deltaLiving;
		undeadCount += deltaUndead;
		corpseCount += deltaCorpse;
	}

	#endregion Getters/Setters
	//////////////////////////////////////////////////////////////////
}
