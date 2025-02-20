using UnityEngine;
using System.Collections;

public class PersistentCS : MonoBehaviour {

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	// 
	void OnEnable () {
		DontDestroyOnLoad(gameObject);
	}
	
}
