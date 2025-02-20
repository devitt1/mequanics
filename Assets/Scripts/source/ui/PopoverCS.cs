using UnityEngine;
using System.Collections;

public class PopoverCS : MonoBehaviour 
{
	public float m_duration;
	
	// Use this for initialization
	void Start () 
	{
		m_duration = 3f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (m_duration > 0)
		{
		 	m_duration -= Time.deltaTime;
			return;
		} else {
			GameObject.Destroy(gameObject);
		}		
	}
	
}
