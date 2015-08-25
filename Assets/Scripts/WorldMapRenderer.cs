using UnityEngine;
using System.Collections;

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

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
