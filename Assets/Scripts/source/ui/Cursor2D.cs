using UnityEngine;
using System.Collections.Generic;

public class Cursor2D
{
	public enum MODE
	{
		SELECT,
		LOADING,
		CAMERA_ROTATE,
		
		DEFAULT
	}
	
	protected struct CursorInfo 
	{
		public string tex;
		public Vector2 hotSpot;
		public Vector2 size;
		
		public CursorInfo(string t, Vector2 hs, Vector2 siz)
		{
			tex = t;
			hotSpot = hs;
			size = siz;
		}
	}
	
	//
	//
	
	protected static Dictionary<MODE, CursorInfo> s_cursors = new Dictionary<MODE, CursorInfo>() 
	{
		{MODE.CAMERA_ROTATE, new CursorInfo("cam", new Vector2(8f, 8f), new Vector2(16f, 16f))},
		{MODE.SELECT, new CursorInfo("cursor", new Vector2(0f, 0f), new Vector2(16f, 16f))},
		{MODE.LOADING, new CursorInfo("icon loading", new Vector2(8f, 8f), new Vector2(16f, 16f))},
		
		{MODE.DEFAULT, new CursorInfo(null, Vector2.zero, Vector2.zero)}
	};
	
	//
	//
	
	protected static CursorInfo GetCursorInfo(MODE mode)
	{
		CursorInfo ci = s_cursors[MODE.DEFAULT];
		if (s_cursors.ContainsKey(mode)){
			ci = s_cursors[mode];
		}
		return ci;
	}
	
	public static void SetCursor(Cursor2D.MODE mode)
	{
		CursorInfo ci = GetCursorInfo(mode);
		Texture2D tex = (Texture2D)Resources.Load("Textures/Cursor/" + ci.tex);
		Cursor.SetCursor(tex, ci.hotSpot, CursorMode.ForceSoftware);
			
	}
	
}