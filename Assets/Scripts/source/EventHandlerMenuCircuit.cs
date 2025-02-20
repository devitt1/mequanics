using UnityEngine;
using System.Collections.Generic;

using tools;


/**
 * Interface that allows the user to control the game with the keyboard
 */
public class EventHandlerMenuCircuit : IEventHandler
{
	protected StateCircuitSelect m_state;

	//-------------------------------
	
	public EventHandlerMenuCircuit(StateCircuitSelect state)
	{
		this.m_state = state;
	}

	public bool HandleEvent(Event e)
	{
		switch (e.type){
		case EventType.KeyDown:
			this.HandleKeyPressed(e.keyCode);
			break;
		case EventType.KeyUp:
			this.HandleKeyRelease(e.keyCode);
			break;
		default:
			return false;
		}
		return true;
	}

	
	public void HandleKeyPressed(KeyCode code)
	{
	  try
	  {
		switch (code)
		{
		  case KeyCode.Alpha0:
		  case KeyCode.Alpha1:
		  case KeyCode.Alpha2:
		  case KeyCode.Alpha3:
		  case KeyCode.Alpha4:
		  case KeyCode.Alpha5:
		  case KeyCode.Alpha6:
		  case KeyCode.Alpha7:
		  case KeyCode.Alpha8:
		  case KeyCode.Alpha9:
				int slot = System.Convert.ToInt32(code.ToString().Substring(5, 1));
				m_state.HandleSavegameSelect(slot);
				break;
		  default:
			break;
		}
	  }
		catch (QBException e)
		{
			Popups.Instance.CreateDialog(e);
		}
	}
	
	/**
	 * Key Release event handler
	 * */
	public void HandleKeyRelease(KeyCode code)
	{
	  switch (code)
	  {
		default:
		  break;
	  }
	}
	
}


