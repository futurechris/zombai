// ScalingExperiment_One: Goal is to isolate some factors I think
// are affecting performance in the main simulation, so that I
// can better identify where *unnecessary* bottlenecks are.


using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class SE_One_Controller : MonoBehaviour {

	public Canvas renderCanvas;
	public RectTransform canvasRT;

	public GameObject entityPrefab;

	public int entityCount = 1000;

	private float minX = 0.0f;
	private float maxX = 1.0f;
	private float minY = 0.0f;
	private float maxY = 1.0f;

	private float moveSpeed = 100.0f; // distance in pixels per second

	private List<Transform> entities;

	float nextCycle = 0.0f;
	float cycleDuration = 0.0001f;

	// Use this for initialization
	void Start()
	{
		entities = new List<Transform>(entityCount);

		// instantiate entityCount entities
		initializeCoords();
		instantiateEntities();

		DOTween.defaultEaseType = Ease.Linear;

		DOTween.Init(true, false, LogBehaviour.Default).SetCapacity(2*entityCount+10,0);
	}
	
	// Update is called once per frame
	void Update()
	{
		if(Time.time > nextCycle)
		{
			float x, y;
			float distance = moveSpeed * Time.deltaTime;
			for(int i=0; i<entities.Count; i++)
			{
				x = Mathf.Clamp(Random.Range(-distance, distance)+entities[i].localPosition.x, minX, maxX);
				y = Mathf.Clamp(Random.Range(-distance, distance)+entities[i].localPosition.y, minY, maxY);

//				entities[i].DOLocalMove(new Vector3(x,y,0), cycleDuration, false);
//				DOTween.To(() => entities[i].localPosition, val => entities[i].localPosition = val, new Vector3(x,y,0), cycleDuration);
				entities[i].localPosition = new Vector3(x,y,0);
			}

			nextCycle = Time.time+cycleDuration;
		}
	}


	private void instantiateEntities()
	{
		float x, y;

		for(int i=0; i<entityCount; i++)
		{
			x = Random.Range(minX, maxX);
			y = Random.Range(minY, maxY);

			GameObject entGO = GameObject.Instantiate(entityPrefab) as GameObject;
			entGO.transform.SetParent(canvasRT);

			entGO.transform.localPosition = new Vector3(x,y,0);
			entGO.SetActive(true);

			entities.Add(entGO.transform);
		}
	}

	private void initializeCoords()
	{
		minX = -Camera.main.pixelWidth;
		maxX =  Camera.main.pixelWidth;
		minY = -Camera.main.pixelHeight;
		maxY =  Camera.main.pixelHeight;
	}
}
