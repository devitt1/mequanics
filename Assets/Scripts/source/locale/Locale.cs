using UnityEngine;
using System.Collections.Generic;
using System.Collections;


/**
 * Enum PID
 * */
public enum PID 
{
	BN_BACK = 0,
	BN_CANCEL,
	BN_OK,
	BN_QUIT,
	BN_START,
	
	ER_OTHER_ACTION_IN_PROGRESS_TITLE, ER_OTHER_ACTION_IN_PROGRESS,

	ER_UNDO_NO_MORE_TITLE, ER_UNDO_NO_MORE,
	
	ER_DUP_INVALID_SELECTION_TITLE, ER_DUP_INVALID_SELECTION,
	ER_DUP_INVALID_AXIS_TITLE, ER_DUP_INVALID_AXIS,
	ER_DUP_IMPOSSIBLE_TITLE, ER_DUP_IMPOSSIBLE,
	
	ER_ROTINJ_INVALID_SELECTION_TITLE,ER_ROTINJ_INVALID_SELECTION,
	ER_ROTINJ_INVALID_TARGET_TITLE,ER_ROTINJ_INVALID_TARGET,
	ER_ROTINJ_INVALID_PATH_TITLE, ER_ROTINJ_INVALID_PATH,
	ER_ROTINJ_COLLISION_TITLE, ER_ROTINJ_COLLISION,
	
	ER_MOVEINJ_INVALID_SELECTION_TITLE, ER_MOVEINJ_INVALID_SELECTION,
	
	ER_TPINJ_INVALID_SELECTION_TITLE, ER_TPINJ_INVALID_SELECTION,
	ER_TPINJ_NOT_SIMPLE_LOOP_TITLE, ER_TPINJ_NOT_SIMPLE_LOOP,
	ER_TPINJ_NOT_ONE_TUBE_TITLE, ER_TPINJ_NOT_ONE_TUBE,
	ER_TPINJ_TUBE_TOO_SHORT_TITLE, ER_TPINJ_TUBE_TOO_SHORT,
	
	ER_BRG_INVALID_SELECTION_TITLE, ER_BRG_INVALID_SELECTION,
	ER_BRG_NOT_ALIGNED_TITLE, ER_BRG_NOT_ALIGNED,
	ER_BRG_NOT_A_LOOP_TITLE, ER_BRG_NOT_A_LOOP,
	ER_BRG_SAME_PATH_TITLE, ER_BRG_SAME_PATH,
	ER_BRG_COLLISION_TITLE, ER_BRG_COLLISION,
	
	ER_STP_INVALID_TYPE_TITLE, ER_STP_INVALID_TYPE,
	
	ER_SAVE_SLOTUNUSED_TITLE, ER_SAVE_SLOTUNUSED,
	
	TX_EULA,
	TX_TUTORIAL,
	
	MainMenu,
	SelectACircuit,
	PressF1ForHelp,
	UnDo,
	Screenshot,
	Simplify,
	SplitTube,
	Teleport,
	RotateInjector,
	Bridge,
	InjectorMove,
	ShowHelp,
	Score,
	KeyBindings,
	HelpString,

	ItDoesntWorkLikeThat,
	
	PlayTutorial
}

public class Locale
{
	public enum LOC 
	{
		 ENG
		,JAP
	}
	
	protected static Locale s_instance;
	public static Locale Instance { 
		get { 
			if (Locale.s_instance == null)
			{ 
				Locale.s_instance = new Locale(); 
			} 
			return Locale.s_instance; 
		} 
	}
	
	protected Dictionary<PID, string> m_localeStrings;
	protected LOC m_localeCurrent;
	
	protected Dictionary<LOC, string[]> m_phraseDatabase;
	
	//
	//
	
	public Locale()
	{
		m_phraseDatabase = new Dictionary<LOC, string[]>();
		m_phraseDatabase.Add(LOC.ENG, LocaleENG.LocStrings);
		m_phraseDatabase.Add(LOC.JAP, LocaleENG.LocStrings);
			
		SwitchTo(LOC.ENG);
	}
	
	public void SwitchTo(LOC loc)
	{
		m_localeCurrent = loc;
		
		// Init text database
		UpdateDict();
	}
	
	public string Get(PID ls)
	{
		return m_localeStrings[ls];
	}
	
	public static string Str(PID ls)
	{
		return Locale.Instance.Get(ls);
	}

	protected void UpdateDict()
	{
		if (m_localeStrings == null){
			m_localeStrings = new Dictionary<PID, string>();
		}
		foreach(PID id in System.Enum.GetValues(typeof(PID)))
		{
			if (m_localeStrings.ContainsKey(id)){
				m_localeStrings[id] = m_phraseDatabase[m_localeCurrent][(int)id];
			} else {
				string[] es = m_phraseDatabase[m_localeCurrent];
				m_localeStrings.Add(id, es[(int)id]);
			}
		}
	}
}
