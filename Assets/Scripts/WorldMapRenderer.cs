using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldMapRenderer : MonoBehaviour {

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

	private WorldMap myWorld;

	#endregion Bookkeeping
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region MonoBehaviour methods & helpers
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	#endregion MonoBehaviour methods & helpers
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Instantiation

	public void instantiateWorld()
	{
		instantiateAgents();
		instantiateStructures();
	}

	private void instantiateAgents()
	{
		GameObject tempGO;
		AgentRenderer tempRenderer;

		List<Agent> agents = myWorld.getAgents();

		for(int i=0; i<agents.Count; i++)
		{
			tempGO = GameObject.Instantiate(agentPrefab) as GameObject;
			tempGO.transform.parent = agentsGO.transform;
			tempGO.name = "Agent "+i;

			tempRenderer = tempGO.GetComponent<AgentRenderer>();
			tempRenderer.setAgent(agents[i]);
		}
	}

	private void instantiateStructures()
	{
		List<Rect> structures = myWorld.getStructures();
		foreach(Rect rect in structures)
		{
			GameObject structGO = GameObject.Instantiate(structurePrefab) as GameObject;
			structGO.transform.parent = structuresGO.transform;
			
			structGO.transform.localScale = new Vector3(rect.width, rect.height, 1.0f);
			structGO.transform.position = new Vector3(rect.x+(rect.width/2.0f), rect.y+(rect.height/2.0f), 5.0f);
		}
	}

	#endregion Instantiation
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Getters & Setters
	public void setWorldMap(WorldMap newWorldMap)
	{
		myWorld = newWorldMap;
	}
	#endregion Getters & Setters
	//////////////////////////////////////////////////////////////////
}
