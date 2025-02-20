using UnityEngine;

public class designBGCS : MonoBehaviour 
{

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		this.transform.position = Camera.mainCamera.transform.position;
	}
}
