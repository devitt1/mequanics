using UnityEngine;
using System.Collections.Generic;


public enum HudPos {
	TOPLEFT,
	TOPRIGHT,
	BOTTOMLEFT,
	BOTTOMRIGHT,
	BOTTOM,
	RIGHT,
	LEFT,
	TOP,
	CENTER
}

/**
 * Manages the HUD (Head-Up Display)
 *
 * Shows score
 */
public class Popups
{
	protected static Popups s_instance;
	public static Popups Instance { get{ 
			if (s_instance == null){ s_instance = new Popups(); }
			return s_instance;
		}
	}
	protected HashSet<QBException.Type> m_exceptionPopups;

	//
	//

	public Popups()
	{
		m_exceptionPopups = new HashSet<QBException.Type>();
	}
		
	public Popups(uint width, uint height) : this()
	{
	}

	/**
	 * Show a popup on the screen which disappears automatically after a
	 * certain time
	 */
	public void CreateDialog(string msg)
	{
		CreateDialog(msg, "Notification", HudPos.CENTER);
	}

	public void CreateDialog(string msg, string title)
	{
		CreateDialog(msg, title, HudPos.CENTER);
	}
	
	public void CreateDialog(string msg, string title, HudPos pos)
	{
		CreateDialog(msg, title, pos, null);
	}
		
	public GuiDialog3DCS CreateDialog(string msg, string title, HudPos pos, CallbackParameterless cb)
	{
		return CreateDialog(msg, title, pos, null, true);
	}
	/**
	 * Show a popup on the screen which disappears automatically after a
	 * certain time
	 */
	public GuiDialog3DCS CreateDialog(string msg, string title, HudPos pos, CallbackParameterless cb, bool showButton)
	{	
		GuiDialog3DCS popupCS = PrefabHubCS.Instance.GetDialog3D();
		popupCS.Title = title;
		popupCS.Message = msg;
		popupCS.Pos = pos;
		popupCS.Callback = cb;
		popupCS.ShowButton = showButton;

		return popupCS;
	}
	
	public void CreateDialog(QBException e)
	{
		if (!m_exceptionPopups.Contains(e.Ty)){
			m_exceptionPopups.Add(e.Ty);
			string strMessage = e.GetMessage();
			string strTitle = e.GetTitle();
			CreateDialog(strMessage, strTitle, HudPos.CENTER, () => m_exceptionPopups.Remove(e.Ty));
		}
	}
	
	
	//
	// moved from GuiErrorCS
			
	public static void GenerateError(string message)
	{
		GenerateError(message, 3.0f);
	}
	
	public static void GenerateError(string message, float durationDisplay)
	{	
//		Instance.CreateDialog(message, "Error", HudPos.CENTER, durationDisplay);
		Instance.CreateDialog(message, "Error", HudPos.CENTER);
	}
	
	public void CreateExclamation(string text)
	{
		
	}
	
}
