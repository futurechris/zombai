using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WorldObjectRenderer : MonoBehaviour {
	
	//////////////////////////////////////////////////////////////////
	#region Parameters & properties
	
	public WorldObject worldObject;
	public Image objectImage;
	
	#endregion Parameters & properties
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Bookkeeping

	private bool _dirty = true;

	#endregion Bookkeeping
	//////////////////////////////////////////////////////////////////


	//////////////////////////////////////////////////////////////////
	#region MonoBehaviour methods & helpers
	// Use this for initialization
	//void Start(){}
	
	// Update is called once per frame
	void Update()
	{
		if(_dirty)
		{
			fullUpdate();
			_dirty = false;
		}
	}
	#endregion MonoBehaviour methods & helpers
	//////////////////////////////////////////////////////////////////
	
	//////////////////////////////////////////////////////////////////
	#region Getters & Setters
	
	public void setObject(WorldObject newObject)
	{
		worldObject = newObject;
		worldObject.Renderer = (this);
		fullUpdate();
	}

	public void updateLocation()
	{
		this.transform.localPosition = worldObject.Location;
	}
	
	public void fullUpdate()
	{
		updateLocation();
		objectImage.SetAllDirty();
	}

	public void setDirty()
	{
		_dirty = true;
	}

	#endregion Getters & Setters
	//////////////////////////////////////////////////////////////////
	
	//////////////////////////////////////////////////////////////////
	#region Helpers
	

	
	#endregion Helpers
	//////////////////////////////////////////////////////////////////
}
