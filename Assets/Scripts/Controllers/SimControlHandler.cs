using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SimControlHandler : MonoBehaviour {

	public Text simSpeedLabel;
	public Slider simSpeedSlider;
	public WorldMap map;

	private bool initialSet = false;

	// Use this for initialization
//	void Start () {}
	// Update is called once per frame
	void Update()
	{
		if(!initialSet)
		{
			simulationSpeedChanged();
			initialSet = true;
		}
	}

	public void simulationSpeedChanged()
	{
		float multiplier = simSpeedSlider.value;

		if(multiplier > 1.0f)
		{
			multiplier *= multiplier;
		}

		simSpeedLabel.text = string.Format("{0,-3:N1}x", Mathf.Ceil(10.0f*multiplier)/10.0f);
		simSpeedLabel.SetAllDirty();

//		map.setSimulationSpeed(multiplier);
		Time.timeScale = multiplier;

	}
}
