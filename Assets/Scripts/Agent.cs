using UnityEngine;
using System.Collections;

public class Agent : MonoBehaviour
{
	//////////////////////////////////////////////////////////////////
	#region Agent traits

	private System.Guid guid	 	= System.Guid.NewGuid();
	private Color agentColor 		= Color.cyan;
	private bool isAlive			= true;
	private AgentBehavior behavior	= new NoopBehavior();

	private Vector2 location		= Vector2.zero;
	private float direction			= 0;
	private float fieldOfView		= 0;
	private float sightRange		= 0;

	#endregion Agent traits
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region MonoBehaviour methods
	
	void Start ()
	{
	}
	
	void Update ()
	{	
	}
	#endregion MonoBehaviour methods
	//////////////////////////////////////////////////////////////////
}
