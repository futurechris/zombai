using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldMapRenderer : MonoBehaviour {

	//////////////////////////////////////////////////////////////////
	#region Parameters & properties
	private int minimumStreetWidth = 5;
	
	public GameObject 		agentsGO;		// object agents are placed under - just organizational
	public GameObject 		structuresGO;	// ditto for structures
	public GameObject		worldObjectsGO;
	
	public GameObject 		agentPrefab;
	public GameObject 		structurePrefab;
	public GameObject		worldObjectPrefab; // weird to have singular prefab for this, change later.

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
//	void Start(){}
	// Update is called once per frame
//	void Update(){}

	public void purge()
	{
		for(int i=agentsGO.transform.childCount-1; i>=0; i--)
		{
			Destroy(agentsGO.transform.GetChild(i).gameObject);
		}
		for(int i=structuresGO.transform.childCount-1; i>=0; i--)
		{
			Destroy(structuresGO.transform.GetChild(i).gameObject);
		}
	}
	#endregion MonoBehaviour methods & helpers
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Instantiation

	public void instantiateWorld()
	{
		instantiateAgents(myWorld.getAgents());
		instantiateStructures();
		configureCamera();
	}

	public void instantiateAgents(List<Agent> agents)
	{
		for(int i=0; i<agents.Count; i++)
		{
			instantiateSingleAgent(agents[i], "Agent "+agentsGO.transform.childCount);
		}
	}

	public void instantiateObjects(List<WorldObject> wObjs)
	{
		for(int i=0; i<wObjs.Count; i++)
		{
			instantiateSingleObject(wObjs[i]);
		}
	}

	private void instantiateSingleObject(WorldObject newObj)
	{
		GameObject tempGO;
		WorldObjectRenderer tempRenderer;

		tempGO = GameObject.Instantiate(worldObjectPrefab) as GameObject;

		tempGO.transform.SetParent(worldObjectsGO.transform);
		tempGO.name = newObj.WorldObjectType + " " + worldObjectsGO.transform.childCount;

		tempRenderer = tempGO.GetComponent<WorldObjectRenderer>();
		tempRenderer.setObject(newObj);
	}

	private void instantiateSingleAgent(Agent agent, string name)
	{
		GameObject tempGO;
		AgentRenderer tempRenderer;

		tempGO = GameObject.Instantiate(agentPrefab) as GameObject;
		
		tempGO.transform.parent = agentsGO.transform;
		tempGO.name = name;
		
		tempRenderer = tempGO.GetComponent<AgentRenderer>();
		tempRenderer.setAgent(agent);
	}

	private void instantiateStructures()
	{
		List<Rect> structures = myWorld.getStructures();
		RectTransform tempRT;
		foreach(Rect rect in structures)
		{
			GameObject structGO = GameObject.Instantiate(structurePrefab) as GameObject;
			tempRT = structGO.GetComponent<RectTransform>();

			tempRT.SetParent(structuresGO.transform);
			tempRT.localPosition = new Vector3(rect.x, rect.y, 0.0f);
			tempRT.localScale = new Vector3(rect.width, rect.height, 1.0f);
		}
	}

	#endregion Instantiation
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Getters & Setters
	public void setWorldMap(WorldMap newWorldMap)
	{
		myWorld = newWorldMap;
		configureCamera();
	}
	#endregion Getters & Setters
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Camera Helpers
	
	// This should probably be located in another class that manages camera
	// and screen events
	private void configureCamera()
	{	
		// Fit screen, need to figure out if width or height is the limiting factor
		// So calculate screen aspect ratio and world aspect ratio
		float screenAspectRatio = (float)Screen.width 		/ (float)Screen.height;
		float worldAspectRatio 	= (float)myWorld.getWidth()	/ (float)myWorld.getHeight();

		bool fitToHeight = true;
		if(worldAspectRatio > screenAspectRatio)
		{
			fitToHeight = false;
		}

		float size = 1.0f;
		if(fitToHeight)
		{
			size = (float)myWorld.getHeight() / 2.0f;
		}
		else
		{
			size = (float)myWorld.getWidth() / (2.0f * screenAspectRatio);
		}

		Camera.main.orthographicSize = size;
		Camera.main.transform.position = new Vector3(myWorld.getWidth()/2.0f, myWorld.getHeight()/2.0f, -10.0f);
	}
	
	#endregion Camera Helpers
	//////////////////////////////////////////////////////////////////
}
