//-GabeV
//Player test script

using UnityEngine;
using System.Collections;

public class TestPlayer : MonoBehaviour 
{
	#region vars

	#endregion

	void Awake()
	{
		Spider.playerCharacter = this.gameObject;
	}

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		float dt = Time.deltaTime * TimeScale.timeScale;
		float speed = 1.0f;

		float x = 0.0f;
		float y = 0.0f;
		if (Input.GetKey (KeyCode.W)) { y += 1.0f; }
		if (Input.GetKey (KeyCode.A)) { x -= 1.0f; }
		if (Input.GetKey (KeyCode.S)) { y -= 1.0f; }
		if (Input.GetKey (KeyCode.D)) { x += 1.0f; }
		//TODO: arctan to make unit circle speeds
		x = x * speed * dt;
		y = y * speed * dt;

		if (Input.GetKeyDown (KeyCode.Space)) { Spider.cameraController.Shake ( new Vector2(0.25f, 0.25f), 0.20f); }

		this.gameObject.transform.position += new Vector3(x, y, 0.0f);
	}
}
