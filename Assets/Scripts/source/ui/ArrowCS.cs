using UnityEngine;
using System.Collections;

using tools;

/**
 * 
 * */
public class ArrowCS : MonoBehaviour 
{
	public Direction dir;
	
	// Use this for initialization
	void Start () 
	{
		switch (dir)
		{
		case Direction.LEFT:
		case Direction.RIGHT:
			transform.GetChild(0).renderer.material.color = Color.red;
			transform.GetChild(1).renderer.material.color = Color.red;
			break;
		case Direction.DOWN:
		case Direction.UP:
			transform.GetChild(0).renderer.material.color = Color.green;
			transform.GetChild(1).renderer.material.color = Color.green;
			break;
		case Direction.REAR:
		case Direction.FRONT:
			transform.GetChild(0).renderer.material.color = Color.blue;
			transform.GetChild(1).renderer.material.color = Color.blue;
			break;
		default:
			break;
		} 
	}
	
	// Update is called once per frame
	void Update () 
	{
	}
	
	public void SetScale(float s)
	{
		this.transform.localScale = new Vector3(s, s, s);
	}
}
