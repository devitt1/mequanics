using UnityEngine;
using System.Collections;

public class OutputCamMatrices : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetMouseButtonDown(0))
		{
			Debug.Log("world-view\n" + Camera.mainCamera.worldToCameraMatrix);
			Debug.Log("view-world\n" + Camera.mainCamera.cameraToWorldMatrix);
		}
	
	}
}
