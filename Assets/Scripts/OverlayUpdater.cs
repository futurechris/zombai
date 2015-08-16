using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OverlayUpdater : MonoBehaviour {

	public WorldMap map;

	public Text	livingCount;
	public Text undeadCount;
	public Text corpseCount;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		updateLabels();
	}

	//////////////////////////////////////////////////////////////////
	#region Update helpers

	private void updateLabels()
	{
		if(map != null)
		{
			livingCount.text = "Living: "+map.getLivingCount();
			livingCount.SetAllDirty();

			undeadCount.text = "Undead: "+map.getUndeadCount();
			undeadCount.SetAllDirty();

			corpseCount.text = "Dead: "+map.getCorpseCount();
			corpseCount.SetAllDirty();
		}
	}

	#endregion Update helpers
	//////////////////////////////////////////////////////////////////
}
