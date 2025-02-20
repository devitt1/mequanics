using UnityEngine;
using System.Collections.Generic;

using pcs;
using ui;

namespace narr
{
	
/// <summary>
/// 
/// </summary>
public class RTEvent
{
	protected List<narr.Act> m_acts;
	protected Dictionary<UInput.Typ, UInput> m_inputs;	

	protected bool m_done;
	public bool IsDone { 
		get { 
			foreach(Act a in m_acts){
				if (!a.Done){ 
					return false; 
				}
			}
			return true;
		}
	}
	
	protected GameInterface m_gi;
	
	//
	//
	
	public RTEvent(List<Act> acts)
	{		
		m_acts = acts;
		m_inputs = new Dictionary<UInput.Typ, UInput>();
	}
		
	public RTEvent(List<Act> acts, Dictionary<UInput.Typ, UInput> inputs)
	{		
		m_acts = acts;
		m_inputs = inputs;
	}
	
	public void Start(GameInterface gameInterface)
	{
		m_gi = gameInterface;
			
		m_done = false;
			
		// activate/begin acts
		foreach(Act a in m_acts){
			a.Start(m_gi);
		}
	}
	
	public void Update(float seconds)
	{
		// check completion criteria
		if (!m_done){
			m_done = true;
			foreach(Act a in m_acts){
				a.Progress(seconds);
				if (!a.Done){
					m_done = false;
				}
			}
		}
	}
	
	public void Stop()
	{
		// stop all acts
		foreach(Act a in m_acts){
			a.Stop();
		}
		
	}
	
	public Act GetAct(System.Type type)
	{
		foreach(Act a in m_acts){
			if (a.GetType() == type){
				return a;
			}
		}
		return null;
	}
		
		
	public bool CheckInput(UInput input)
	{
		if (input.Type == UInput.Typ.Screenshot) { return true; }
		if (input.Type == UInput.Typ.MoveCamera) { return true; }
			
		if (m_inputs.ContainsKey(input.Type)){
			if (m_inputs[input.Type] == null) {
				return true;
			} else {
				if (input.Equals(m_inputs[input.Type])){
					return true;
				}
			}
		} 
		return false;
	
	}
}
	
}