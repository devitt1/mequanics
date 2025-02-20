using UnityEngine;
using System.Collections;

public class RenderQueuesSelection : MonoBehaviour 
{
    public int RenderQueueNb;

	// Use this for initialization
	void Update ()
	{
	    renderer.material.renderQueue = RenderQueueNb;
	}
	
}
