using UnityEngine;
using System.Collections;

public interface IEventHandler
{
	/**
	 * 	Implement this method to become a 
	 * 	eligible listener to class EventReceiverCS
	 * */
	bool HandleEvent(UnityEngine.Event e);
}

