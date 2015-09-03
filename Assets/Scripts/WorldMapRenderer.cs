using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldMapRenderer : MonoBehaviour {

	//////////////////////////////////////////////////////////////////
	#region Parameters & properties
	private int minimumStreetWidth = 5;
	
	public GameObject 		agentsGO;		// object agents are placed under - just organizational
	public GameObject 		structuresGO;	// ditto for structures
	
	public GameObject 		agentPrefab;
	public GameObject 		structurePrefab;

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
		instantiateAgents();
		instantiateStructures();
		configureCamera();
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
		
		float calcHeight = myWorld.getHeight();
		if(screenAspectRatio < worldAspectRatio)
		{
			calcHeight /= screenAspectRatio;
		}
		// all other cases are handled automatically by calculating ortho from worldheight
		
		// ortho size is half target height, whatever that came out to be.
		Camera.main.orthographicSize = calcHeight/2.0f;
		Camera.main.transform.position = new Vector3(myWorld.getWidth()/2.0f, myWorld.getHeight()/2.0f, -10.0f);
	}
	
	#endregion Camera Helpers
	//////////////////////////////////////////////////////////////////
}
