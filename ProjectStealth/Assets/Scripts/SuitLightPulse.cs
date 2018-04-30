using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuitLightPulse : MonoBehaviour 
{
	public Light light_reference;
	private float timer;

	// Use this for initialization
	void Start () 
	{
		timer = 0.0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		timer += Time.deltaTime * TimeScale.timeScale;
		float alpha = 0.6f + 0.4f * Mathf.Sin( timer * 2.0f * Mathf.PI );

		Color color = this.GetComponent<SpriteRenderer> ().color;
		color.a = alpha;

		light_reference.intensity = alpha * 5.0f;

		this.GetComponent<SpriteRenderer> ().color = color;
	}
}
