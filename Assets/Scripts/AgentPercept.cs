using UnityEngine;
using System.Collections;

// Certainly anaemic, but might be enough to learn what we'll really need.
public class AgentPercept
{
	public enum PerceptType { AGENT, WALL };
	public enum LivingState { INANIMATE, ALIVE, DEAD, UNDEAD };

	public PerceptType type = PerceptType.WALL;
	public LivingState living = LivingState.INANIMATE;

	public Vector2 locOne = Vector2.zero;
	public Vector2 locTwo = Vector2.zero;
}
