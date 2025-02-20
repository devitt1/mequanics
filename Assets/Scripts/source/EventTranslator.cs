using UnityEngine;
using System.Collections.Generic;
using System;

using tools;
using pcs;



/**
 * Class that handles input events from keyboard, mouse, touchscreen
 * All 2D coordinates pixel-based with origin at lower left corner of the screen
 */
public class EventTranslator : IEventHandler, ICoordinateTranlator
{
	IEventInterpreter m_interpreter;

	//
	//
	
	public EventTranslator(IEventInterpreter interpreter)
	{
		m_interpreter = interpreter;
	}
	
	public bool HandleEvent(Event e)
	{
		switch (e.type){
		case EventType.MouseDown:
			m_interpreter.HandleButtonDown(ScreenToViewport(GuiToScreen(e.mousePosition)), e.button);
			break;
		case EventType.MouseUp:
			m_interpreter.HandleButtonUp(ScreenToViewport(GuiToScreen(e.mousePosition)), e.button);
			break;
		case EventType.MouseDrag:
			m_interpreter.HandleMove(ScreenToViewport(GuiToScreen(e.mousePosition)));
			break;
		case EventType.KeyDown:
			m_interpreter.HandleKeyPressed(e.keyCode);
			break;
		case EventType.KeyUp:
			m_interpreter.HandleKeyRelease(e.keyCode);
			break;
		default:
			return false;
		}
		return true;
	}
	
	public Vector2 GuiToScreen(Vector2 pos)
	{
		return new Vector2(pos.x, Camera.mainCamera.pixelHeight - pos.y);
	}
	
	public Vector2 ScreenToViewport(VectorInt2 pos)
	{
		return Camera.main.ScreenToViewportPoint(pos.ToVector2());
	}

	public Vector2 ScreenToViewport(Vector2 pos)
	{
		return Camera.main.ScreenToViewportPoint(pos);
	}
	
	public VectorInt2 ViewportToScreen(Vector2 pos)
	{
		Vector3 v = Camera.main.ViewportToScreenPoint(pos);
		return new VectorInt2((int)v.x, (int)v.y);
	}

	public Rect GuiToScreen(Rect r)
	{
		Vector2 pos = GuiToScreen(new Vector2(r.x, r.y));
		Vector2 size = GuiToScreen(new Vector2(r.width, r.height));
		return new Rect(pos.x, pos.y, size.x, size.y);
	}

	public Rect ScreenToViewport(Rect r)
	{
		Vector2 pos = ScreenToViewport(new Vector2(r.x, r.y));
		Vector2 size = ScreenToViewport(new Vector2(r.width, r.height));
		return new Rect(pos.x, pos.y, size.x, size.y);
	}
	
}


