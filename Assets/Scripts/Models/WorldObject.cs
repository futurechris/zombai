using UnityEngine;
using System.Collections;

public class WorldObject
{
	//////////////////////////////////////////////////////////////////
	#region WorldObject Types/Defaults

	public enum ObjectType { EXTRACT_POINT };

	#endregion WorldObject Types/Defaults
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Bookkeeping
	private	System.Guid guid				= System.Guid.NewGuid();
	private bool needsInitialization		= true;

	[SerializeField]
	private WorldObjectRenderer _myRenderer;
	public WorldObjectRenderer Renderer { set { _myRenderer = value; } }

	#endregion Bookkeeping
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Object Traits

	[SerializeField]
	private ObjectType _objType				= ObjectType.EXTRACT_POINT;
	public ObjectType WorldObjectType { get {return _objType;} set { _objType = value; } }

	[SerializeField]
	private Vector2 _location				= Vector2.zero;
	public Vector2 Location { get {return _location;} set { _location = value; } }

	#endregion Object Traits
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Constructor/Init

	public WorldObject(ObjectType newType)
	{
		configureAs(newType,true);
	}

	#endregion Constructor/Init
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Object Type Definitions

	public void configureAs(ObjectType newType)
	{
		configureAs(newType, false);
	}

	public void configureAs(ObjectType newType, bool resetFirst)
	{
		if(resetFirst)
		{
			configureDefault();
		}

		needsInitialization = false;

		switch(newType)
		{
			case ObjectType.EXTRACT_POINT:
				configureAsExtractPoint();
				break;
		}
	}

	private void configureDefault(){}

	private void configureAsExtractPoint(){}

	#endregion Object Type Definitions
	//////////////////////////////////////////////////////////////////
}
