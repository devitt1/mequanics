using UnityEngine;
using System.Collections;


/**
 * Class to act as a proxy for Unity input events, 
 * to relieve input handling classes to inherit 
 * MonoBehavior. 
 **/
public class EventReceiverCS : MonoBehaviour
{
	public IEventHandler m_handler;
	
	public EventReceiverCS(IEventHandler handler)
	{
		this.m_handler = handler;
		if (name != null) { this.name = name; }
	}
	
	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
	}
	
	/**
	 * 	Overrider this method to implement custom input handling.
	 * */
	void OnGUI ()
	{
		if (m_handler != null){
			m_handler.HandleEvent(UnityEngine.Event.current);
		}
	}
}

