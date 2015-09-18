using UnityEngine;
using System.Collections;

// Certainly anaemic, but might be enough to learn what we'll really need.
public class AgentPercept
{
	public enum PerceptType { AGENT, WALL, EXTRACT };
	public enum LivingState { INANIMATE, ALIVE, DEAD, UNDEAD };

	public PerceptType type = PerceptType.WALL;
	public LivingState living = LivingState.INANIMATE;

	public Vector2 locOne = Vector2.zero;
	public Vector2 locTwo = Vector2.zero;

	public float facingDirection = 0.0f;

	// I don't like exposing this here - not only is a percept
	// not always an agent, but the whole point of this class
	// is that agents only have partial knowledge/perception,
	// so letting an agent have direct access to the object 
	// messes with encapsulation and could break the partial
	// knowledge ideal.
	//
	// But, need it until I decide on a better way to handle
	// agent-targeting actions
	// TODO: Improve percept encapsulation
	public Agent perceivedAgent = null;
}
