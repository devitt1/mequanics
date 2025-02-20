using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using pcs;
using tools;


/**
 * Class to make dynamic prefab instantiation convenient for non-MonoBehavior objects
 * 
 * Singleton class
 * */
public class AsyncCS : MonoBehaviour 
{
	protected static AsyncCS s_async;
	public static AsyncCS Instance {
		get { return s_async; }
	}

	//
	//
	
	void OnEnable()
	{
		if (AsyncCS.s_async == null){
			s_async = this;
		} else {
			System.Diagnostics.Debug.Assert(false);
		}
	}
	
	void OnDisable()
	{
		AsyncCS.s_async = null;
	}
	
	void Start() {}
	
	void Update() {}
	
	//
	//
		
	public void HostCoroutine(IEnumerator cr)
	{
		StartCoroutine(cr);
	}
}

