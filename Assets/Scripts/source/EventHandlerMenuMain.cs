using UnityEngine;
using System.Collections.Generic;

using tools;


/**
 * Interface that allows the user to control the game with the keyboard
 */
public class EventHandlerMenuMain : IEventHandler
{
	protected StateMenuMain m_state;

	//
	
	public EventHandlerMenuMain(StateMenuMain state)
	{
		this.m_state = state;
	}

	public bool HandleEvent(Event e)
	{
		if (e.type == EventType.KeyDown) {
			
			if(e.keyCode == KeyCode.Escape) {
				
				m_state.AbortSplash();
			}
			return true;
		}
		return false;
	}
}


