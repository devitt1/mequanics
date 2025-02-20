
using UnityEngine;
using System.Collections.Generic;

using tools;
using pcs;
using ui;


/**
 * Narrative is used by GameInterface as a wrapper to filter all user input
 * */
public class InputFilter : IInputHandler
{
	protected IInputHandler m_wrapped;
		
	//
	//

	public InputFilter(IInputHandler wrapped)		
	{
		m_wrapped = wrapped;
	}
		
	public virtual bool HandleInput(UInput input)
	{
		return m_wrapped.HandleInput(input);
	}
		
	//
	//
	
}

