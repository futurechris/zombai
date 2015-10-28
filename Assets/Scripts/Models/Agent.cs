using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Agent
{
	//////////////////////////////////////////////////////////////////
	#region Agent Types/Defaults
	// Regularly used agent types to speed up initialization
	// and enable conversion between agent types
	public enum AgentType { CUSTOM, HUMAN, ZOMBIE, CORPSE, HUMAN_PLAYER, ZOMBIE_PLAYER };

	private static Color humanColor = Color.green;
	private static Color zombieColor = Color.red;
	private static Color corpseColor = Color.cyan;
	private static Color humanPlayerColor = Color.yellow;
	private static Color zombiePlayerColor = Color.magenta;

	#endregion Agent Types/Defaults
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Bookkeeping
	[SerializeField]
	private	System.Guid _guid				= System.Guid.NewGuid();
	public System.Guid Guid { get { return _guid; } }

	[SerializeField]
	private bool _needsInitialization		= true;

	[SerializeField]
	private AgentRenderer _myRenderer;
	public AgentRenderer Renderer { set { _myRenderer = value; } }

	[SerializeField]
	protected List<AgentPercept> _perceptPool	= new List<AgentPercept>();
	public List<AgentPercept> PerceptPool { get { return _perceptPool; } }

	private int _perceptIndex = 0;

	#endregion Bookkeeping
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Agent traits
	// Several of these wouldn't "const" correctly, so for now non-DRYly
	// duplicating the 'defaults' down in configureDefault()
	[SerializeField]
	private AgentPercept.LivingState _living= AgentPercept.LivingState.ALIVE;
	public AgentPercept.LivingState LivingState { get { return _living; } set { _living = value; } }

	[SerializeField]
	private AgentType _agentType			= AgentType.HUMAN;
	public AgentType Type { get { return _agentType; } set { _agentType = value; } }

	[SerializeField]
	private AgentBehavior _behavior			= new NoopBehavior();
	public AgentBehavior Behavior
	{ 
		get { return _behavior; }
		set
		{
			_behavior = value;
			value.setAgent(this);
		}
	}

	[SerializeField]
	private Color _agentColor				= Color.cyan;
	public Color AgentColor
	{ 
		get { return _agentColor; } 
		set
		{
			_agentColor = value; 
			if(_myRenderer != null)
			{
				_myRenderer.updateColor();
			}
		}
	}

	[SerializeField]
	private Vector2 _location				= Vector2.zero;
	public Vector2 Location
	{
		get { return _location; }
		set
		{
			_location = value;
			if(_myRenderer != null)
			{
				_myRenderer.updateLocation();
			}
		}
	}

	[SerializeField]
	private float _direction				= 0.0f;
	public float Direction
	{
		get { return _direction; }
		set
		{
			_direction = value;
			if(_direction < 0)
			{
				_direction += 360;
			}
			if(_direction >= 360)
			{
				_direction -= 360;
			}
			if(_myRenderer != null)
			{
				_myRenderer.recalculateFOVImage();
			}
		}
	}

	[SerializeField]
	private float _fieldOfView				= 0.0f;
	public float FieldOfView
	{
		get { return _fieldOfView; }
		set
		{
			_fieldOfView = value;
			if(_myRenderer != null)
			{
				_myRenderer.recalculateFOVImage();
			}
		}
	}

	[SerializeField]
	private float _sightRange				= 0.0f;
	public float SightRange
	{
		get { return _sightRange; }
		set
		{
			_sightRange = value;
			if(_myRenderer != null)
			{
				_myRenderer.updateFOVScale();
			}
		}
	}

	[SerializeField]
	private float _convertRange				= 0.0f;
	public float ConvertRange { get { return _convertRange; } set { _convertRange = value; } }

	[SerializeField]
	private float _speedMultiplier			= 1.0f;
	public float SpeedMultiplier { get { return _speedMultiplier; } set { _speedMultiplier = value; } }

	[SerializeField]
	private float _turnSpeedMultiplier		= 1.0f;
	public float TurnSpeedMultiplier { get { return _turnSpeedMultiplier; } set { _turnSpeedMultiplier = value; } }

	// These are *definitely* going to need iteration. :P
	[SerializeField]
	private bool _moveInUse					= false;
	public bool MoveInUse { get { return _moveInUse; } set { _moveInUse = value; } }

	[SerializeField]
	private bool _lookInUse					= false;
	public bool LookInUse { get { return _lookInUse; } set { _lookInUse = value; } }

	#endregion Agent traits
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Constructor/init

	public Agent(): this(AgentType.HUMAN){}

	public Agent(AgentType newType)
	{
		configureAs(newType,true);
	}

	#endregion MonoBehaviour methods
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Agent type definitions

	// since we don't seem to have defaults in whatever C# I'm getting via Mono...
	public void configureAs(AgentType newType)
	{
		configureAs(newType, false);
	}

	public void configureAs(AgentType newType, bool resetFirst)
	{
		if(resetFirst)
		{
			configureDefault();
		}

		_needsInitialization = false;

		switch(newType)
		{
			case AgentType.CUSTOM:
				configureAsCustom();
				break;
			case AgentType.CORPSE:
				configureAsCorpse();
				break;
			case AgentType.HUMAN:
				configureAsHuman();
				break;
			case AgentType.HUMAN_PLAYER:
				configureAsPlayableHuman();
				break;
			case AgentType.ZOMBIE:
				configureAsZombie();
				break;
			case AgentType.ZOMBIE_PLAYER:
				configureAsPlayableZombie();
				break;
		}
	}

	private void configureAsCustom()
	{
		Type = (AgentType.CUSTOM);
	}

	private void configureAsCorpse()
	{
		AgentColor = (corpseColor);
		LivingState = (AgentPercept.LivingState.DEAD);
		Type = (AgentType.CORPSE);

		SightRange = (0.0f);
		FieldOfView = (0.0f);
		Direction = (Random.Range(-180.0f, 180.0f));

		Behavior = (new NoopBehavior());
	}

	private void configureAsHuman()
	{
		AgentColor = (humanColor);
		LivingState = (AgentPercept.LivingState.ALIVE);
		Type = (AgentType.HUMAN);
		SightRange = (36.0f);
		FieldOfView = (180.0f); // roughly full range of vision
		Direction = (Random.Range(-180.0f, 180.0f));
		SpeedMultiplier = (1.15f);
		
		FallThroughBehavior tempFTB = new FallThroughBehavior();
		tempFTB.addBehavior( new FleeBehavior() );
		tempFTB.addBehavior( new WanderBehavior() );
		tempFTB.addBehavior( new RandomLookBehavior() );
		
		Behavior = (tempFTB);
//		Behavior = new RandomWalkBehavior();
	}

	private void configureAsPlayableHuman()
	{
		AgentColor = (humanPlayerColor);
		LivingState = (AgentPercept.LivingState.ALIVE);
		Type = (AgentType.HUMAN_PLAYER);
		SightRange = (49.0f); // longer range to help with reaction time
		FieldOfView = (135.0f); // slightly expanded vision range, same
		Direction = (Random.Range(-180.0f, 180.0f));
		SpeedMultiplier = (1.25f); // and slightly faster
		TurnSpeedMultiplier = (2.0f);
		
		FallThroughBehavior tempFTB = new FallThroughBehavior();
		tempFTB.addBehavior( new PlayerControlBehavior() );
		tempFTB.addBehavior( new ExtractionBehavior() );
		tempFTB.addBehavior( new RandomLookBehavior() ); // leaving this in for now, useful.
		
		Behavior = (tempFTB);
	}

	private void configureAsZombie()
	{
		AgentColor = (zombieColor);
		LivingState = (AgentPercept.LivingState.UNDEAD);
		Type = (AgentType.ZOMBIE);
		SightRange = (25.0f);
		FieldOfView = (120.0f);
		Direction = (Random.Range(-180.0f, 180.0f));
		SpeedMultiplier = (1.0f);
		ConvertRange = (2.0f);
		
		FallThroughBehavior tempFTB = new FallThroughBehavior();
		// General Zombie:
		tempFTB.addBehavior( new ZombifyBehavior() );
		tempFTB.addBehavior( new PursueBehavior() );
		tempFTB.addBehavior( new NecrophageBehavior() );
		tempFTB.addBehavior( new WanderBehavior() );
		tempFTB.addBehavior( new RandomLookBehavior() );

		// Boid Zombies
//		tempFTB.addBehavior( new BoidsBehavior());
//		tempFTB.addBehavior( new WanderBehavior() );
//		FieldOfView = (351.0f); // add a "tail wedge" to show direction, until something better is in
		
		Behavior = (tempFTB);
	}

	private void configureAsPlayableZombie()
	{
		AgentColor = (zombieColor);
		LivingState = (AgentPercept.LivingState.UNDEAD);
		Type = (AgentType.ZOMBIE_PLAYER);
		SightRange = (36.0f); // slightly expanded, to account for reaction time and lack of overwhelming numbers
		FieldOfView = (135.0f); // super zombie
		Direction = (Random.Range(-180.0f, 180.0f));
		SpeedMultiplier = (1.15f); // same as a human
		TurnSpeedMultiplier = (2.0f); // just a lot more responsive this way
		ConvertRange = (5.0f); // significant boost, to help account for lag/control timing
		
		FallThroughBehavior tempFTB = new FallThroughBehavior();
		tempFTB.addBehavior( new ZombifyBehavior() );
		tempFTB.addBehavior( new PlayerControlBehavior() );
		tempFTB.addBehavior( new RandomLookBehavior() );
		
		Behavior = (tempFTB);
	}
	


	private void configureDefault()
	{
		LivingState = (AgentPercept.LivingState.ALIVE);
		Type = (AgentType.HUMAN);
		Behavior = (new NoopBehavior());
		AgentColor = (humanColor);

		Direction = (0.0f);
		FieldOfView = (0.0f);
		SightRange = (0.0f);
		ConvertRange = (0.0f);

		SpeedMultiplier = (1.0f);
		TurnSpeedMultiplier = (1.0f);

		MoveInUse = (false);
		LookInUse = (false);
	}

	#endregion Agent type definitions
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Agent state updates

	public void extractSuccess()
	{
		_myRenderer.gameObject.SetActive(false);
	}

	public void resetPercepts()
	{
		_perceptIndex = 0;
	}

	public AgentPercept getNextPercept()
	{
		if(_perceptIndex >= _perceptPool.Count)
		{
			_perceptPool.Add(new AgentPercept());
			return _perceptPool[_perceptPool.Count-1];
		}
		return _perceptPool[_perceptIndex++];
	}

	#endregion Agent state updates
	//////////////////////////////////////////////////////////////////
}
