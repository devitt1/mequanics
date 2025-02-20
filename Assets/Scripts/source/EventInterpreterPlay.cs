using UnityEngine;
using System;
using System.Collections.Generic;

using tools;
using pcs;
using ui;


public class EventInterpreterPlay : IEventInterpreter
{
	public enum Mode
	{	
		IDLE,
		CAMERA_ROTATE,
		CAMERA_MOVE,
		SELECT,
		SELECT_ADD,
		SELECT_ADD_DRAG,
		SELECT_DRAG,
		INJECTOR_MOVE,
		TUBE_SPLIT,
		MOVE_SELECTION,
		MOVE_DUPLICATE,
		MOVE_CROSSING
	} 
	
	protected enum SoftKey 
	{
		SHIFT = 0,
		CTRL = 1,
		ALT = 2
	}
	
	//
	//
	
	public bool Ctrl { get { return (Input.GetKey(KeyCode.LeftControl) || 
										Input.GetKey(KeyCode.RightControl) || 
										m_SoftKeysDown[(int)SoftKey.CTRL]); } }
	public bool Alt { get { return (Input.GetKey(KeyCode.LeftAlt) || 
										Input.GetKey(KeyCode.RightAlt) || 
										m_SoftKeysDown[(int)SoftKey.ALT]); } }
	public bool Shift { get { return (Input.GetKey(KeyCode.LeftShift) || 
										Input.GetKey(KeyCode.RightShift) || 
										m_SoftKeysDown[(int)SoftKey.SHIFT]); } }
	protected bool[] m_SoftKeysDown;
	
	
	protected Mode m_mode;
	public IEventHandler m_eventHandler;
	protected ICoordinateTranlator m_coordTrans;
	protected IInputHandler m_inter;
	protected Gui3DStatePlayCS m_gui;
	
	
	protected Vector2 m_posLast;
	protected Vector2 m_posDownLast;
	
	public List<Axis> m_axesProminent;
	
	protected PieceHit m_hit;
	protected AxisHit m_hitAxis;
	
	UInput input;
		
	//
	//
	
	public EventInterpreterPlay(IInputHandler handler, Gui3DStatePlayCS gui)
	{
		this.m_inter = handler;
		this.m_gui = gui;
		this.m_axesProminent = new List<Axis>();
		
		m_SoftKeysDown = new bool[3]{false, false, false};
		
		EventTranslator et = new EventTranslator(this);
		m_eventHandler = et;
		m_coordTrans = et;
		
		m_mode = Mode.IDLE;
	}
	
	public void SetInputHandler(IInputHandler ih)
	{
		m_inter = ih;
	}
	
	
	public void Update(float seconds)
	{	
		if (m_mode == Mode.TUBE_SPLIT){
			HandleMove(m_coordTrans.ScreenToViewport(Input.mousePosition));
		} else if (m_mode == Mode.INJECTOR_MOVE){
			HandleMove(m_coordTrans.ScreenToViewport(Input.mousePosition));
		}
	}
	
	//
	//
	
	/**
	 * Mouse button event handler
	 * */
	public void HandleButtonDown(Vector2 pos, int button)
	{	
		try
		{
			switch (m_mode){
			case Mode.TUBE_SPLIT:
				m_inter.HandleInput(new UICommitTubeSplit());
				SetMode(Mode.IDLE);
				break;
			case Mode.INJECTOR_MOVE:
				m_inter.HandleInput(new UICommitInjectorMove());
				SetMode(Mode.IDLE);
				break;
			default:
				if (button == 0)	// Left mouse button
				{	
					m_posLast = pos;
					
					if (Shift){
						// move camera
						SetMode(Mode.CAMERA_MOVE);
					} else {
						// buffer hit screen position
						m_posDownLast = pos;
						
						if (Ctrl && !Alt) { 
							// add-select
							SetMode(Mode.SELECT_ADD);
						} else { 
							input = new UIHitPieceRay(Camera.mainCamera.ViewportPointToRay(pos));
							m_inter.HandleInput(input);			
							m_hit = ((UIHitPieceRay)input).result;
							if (m_hit.hitSomething){
								// select hit piece for once
								if (m_hit.idHit <= 0){
									m_inter.HandleInput(new UISelectPiece(m_hit.posPiece, m_hit.alignment, false));
								} else {
									m_inter.HandleInput(new UISelectPiece(m_hit.idHit, false));
								}
								
								// display move cursor
								m_axesProminent.Clear();
								input = new UIGetProminentAxes(m_hit.posHit);
								m_inter.HandleInput(input);
								m_axesProminent = ((UIGetProminentAxes)input).result;
								
								m_inter.HandleInput(new UISetCursorAxisActive(Axis.INVALID));
								if (m_axesProminent.Count >= 3){
									m_inter.HandleInput(new UISetCursorAxisNormal(m_axesProminent[2]));
								}
								m_inter.HandleInput(new UIUpdateCursorPos());
								m_inter.HandleInput(new UIShowCursor(true, 0));
								// select (change to various move modes as soon as mouse goes moving)
								SetMode(Mode.SELECT);
							} else {
								BeginDragSelect();
								SetMode(Mode.SELECT_DRAG);
							}
						}
					}	
				} 
				else if (button == 1) // Right mouse button
				{ 
					m_posLast = pos;
					
					SetMode(Mode.CAMERA_ROTATE);
				}
				break;
			}
		}
		catch (QBException e)
		{
			UnityEngine.Debug.LogError(e.Message);
			ShowPopupException(e);
		}
	}
	
	/**
	 * Mouse button event handler
	 * */
	public void HandleButtonUp(Vector2 pos, int button)
	{
		try
		{
			if (button == 1){ // right mouse button	
				switch (m_mode)
				{
				case Mode.CAMERA_ROTATE: // RMB-release apllies to Camera-rotate mode only
					SetMode(Mode.IDLE);
					break;
				default:
					break;
				}
			} else if (button == 0){ // left mouse button
				switch (m_mode)
				{
					case Mode.MOVE_SELECTION:
					case Mode.MOVE_DUPLICATE:
					case Mode.MOVE_CROSSING:
						m_inter.HandleInput(new UIEndMove());
//						m_inter.HandleInput(new UISetCursorAxes(Axis.INVALID, Axis.INVALID));
						m_inter.HandleInput(new UISetCursorAxisActive(Axis.INVALID));
						m_inter.HandleInput(new UISetCursorAxisNormal(Axis.INVALID));
						break;
					case Mode.SELECT:
						if (m_hit.hitSomething){
							m_inter.HandleInput(new UIUnselectAll());
							m_inter.HandleInput(new UISelectPiece(m_hit.posPiece, m_hit.alignment, false));
						}
						break;
					case Mode.SELECT_ADD:
						input = new UIHitPieceRay(Camera.mainCamera.ViewportPointToRay(pos));
						m_inter.HandleInput(input);
						m_hit = ((UIHitPieceRay)input).result;
						if (m_hit.hitSomething){
							m_inter.HandleInput(new UISelectPiece(m_hit.posPiece, m_hit.alignment, true));
						}
						break;
					case Mode.SELECT_DRAG:
					case Mode.SELECT_ADD_DRAG:
						if (m_mode == Mode.SELECT_DRAG){
							m_inter.HandleInput(new UIUnselectAll());
						}
						EndDragSelect();
						break;
					default:
						break;
				}
	
				SetMode(Mode.IDLE);
			}
		}
		catch (QBException e)
		{
			ShowPopupException(e);
		}
	}
	
	/**
	 * Mouse move event handler
	 * */
	public void HandleMove(Vector2 pos)
	{			
	  	Vector2 offset = pos - m_posLast;
		
		try
		{
			if (m_mode == Mode.SELECT) 
			{
				Vector2 offsetJump = pos - m_posDownLast;
				if (m_coordTrans.ViewportToScreen(offsetJump).LengthAccum() >= 8) {
					input = new UIMatchAxis(m_hit.posHit, offsetJump, m_axesProminent.GetRange(0, 2));
					m_inter.HandleInput(input);
					Axis a = ((UIMatchAxis)input).result;
//					m_inter.HandleInput(new UISetCursorAxes(a, m_axesProminent[2]));
					m_inter.HandleInput(new UISetCursorAxisActive(a));
					m_inter.HandleInput(new UISetCursorAxisNormal(m_axesProminent[2]));
					if (!Ctrl){
						if (!Alt){
							m_inter.HandleInput(new UIBeginMove(a));
							SetMode(Mode.MOVE_SELECTION);
							MoveSelection(offsetJump);
						} else {
							m_inter.HandleInput(new UIBeginMoveDuplicate(a));
							SetMode(Mode.MOVE_DUPLICATE);
							MoveSelection(offsetJump);
						}
					} else {
						if (Alt){
							m_inter.HandleInput(new UIBeginMoveCrossing(a));
							SetMode(Mode.MOVE_CROSSING);
							MoveSelection(offsetJump);
						} else {
							SetMode(Mode.IDLE);
						}
					}
				}
			} else {
				Vector2 offsetJump = pos - m_posDownLast;
				if (m_mode == Mode.SELECT_ADD) 
				{
					if (m_coordTrans.ViewportToScreen(offsetJump).LengthAccum() >= 3) {
						BeginDragSelect();
						SetMode(Mode.SELECT_ADD_DRAG);
					}
				}
				
				switch (m_mode)
				{
				case Mode.TUBE_SPLIT:
					m_inter.HandleInput(new UIPreviewBoxAt(pos));
					break;
				case Mode.INJECTOR_MOVE:
					input = new UIHitPieceRay(Camera.mainCamera.ViewportPointToRay(pos));
					m_inter.HandleInput(input);
					PieceHit pieceHit = ((UIHitPieceRay)input).result;
					if (pieceHit.hitSomething) {
						m_inter.HandleInput(new UIPreviewInjectorAt(pieceHit.posPiece, pieceHit.alignment, pieceHit.posHit));
					} else {
						m_inter.HandleInput(new UIPreviewInjectorAt(new VectorInt3(100000,0,0), false, Vector3.zero)); // put a location far away
					}
					break;
				case Mode.MOVE_SELECTION:
				case Mode.MOVE_DUPLICATE:
				case Mode.MOVE_CROSSING:
					// doing smooth movement		
					//CONTINUE
					MoveSelection(offset);
					m_inter.HandleInput(new UIUpdateCursorPos());
					break;
				case Mode.CAMERA_ROTATE:
					// taken care of by MouseOrbitZoom.js
					break;
				case Mode.SELECT_ADD_DRAG:
				case Mode.SELECT_DRAG:
					// taken care of by GuiStatePlayCS.cs
					break;
				case Mode.CAMERA_MOVE:
					MoveCamera(offset);
					break;	
				default:
					break;
				}
			}
		}
		catch (QBException e)
		{
			SetMode(Mode.IDLE);
			ShowPopupException(e);
		}
		
		m_posLast = pos;
	}
	
	/***/
	public void HandleKeyPressed(KeyCode code)
	{
		try
		{
			switch (code)
			{
			case KeyCode.Z:
				SetMode(Mode.TUBE_SPLIT);
				break;
			case KeyCode.X:
				m_inter.HandleInput(new UISimplify());
				break;
			case KeyCode.A:
				SetMode(Mode.INJECTOR_MOVE);
				break;
			case KeyCode.S:
				m_inter.HandleInput(new UITeleport());
				break;
			case KeyCode.C:
				m_inter.HandleInput(new UIRotateInjector());
				break;
			case KeyCode.V:
				m_inter.HandleInput(new UIBridge());
				break;
			case KeyCode.Escape:
				SetMode(Mode.IDLE);
				break;
			case KeyCode.U:
				m_inter.HandleInput(new UIUndoLastAction());
				break;
			case KeyCode.J:
				m_inter.HandleInput(new UISwitchCameraProjection());
				break;
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
				int slot = Convert.ToInt32(code.ToString().Substring(5, 1));
				if (Input.GetKey(KeyCode.LeftAlt)){
					m_inter.HandleInput(new UISaveProgress(slot));
				} else {
					m_inter.HandleInput(new UILoadProgress(slot));
				}
				break;
			case KeyCode.T:
				m_inter.HandleInput(new UILookAtSceneFrom(Direction.REAR));
				break;
			case KeyCode.Y:
				m_inter.HandleInput(new UILookAtSceneFrom(Direction.FRONT));
				break;
			case KeyCode.G:
				m_inter.HandleInput(new UILookAtSceneFrom(Direction.DOWN));
				break;
			case KeyCode.H:
				m_inter.HandleInput(new UILookAtSceneFrom(Direction.UP));
				break;
			case KeyCode.B:
				m_inter.HandleInput(new UILookAtSceneFrom(Direction.LEFT));
				break;
			case KeyCode.N:
				m_inter.HandleInput(new UILookAtSceneFrom(Direction.RIGHT));
				break;
			case KeyCode.I:
				m_inter.HandleInput(new UISetTimeDirection(Direction.REAR));
				break;
			case KeyCode.O:
				m_inter.HandleInput(new UISetTimeDirection(Direction.FRONT));
				break;
			case KeyCode.K:
				m_inter.HandleInput(new UISetTimeDirection(Direction.DOWN));
				break;
			case KeyCode.L:
				m_inter.HandleInput(new UISetTimeDirection(Direction.UP));
				break;
			case KeyCode.Comma:
				m_inter.HandleInput(new UISetTimeDirection(Direction.LEFT));
				break;
			case KeyCode.Period:
				m_inter.HandleInput(new UISetTimeDirection(Direction.RIGHT));
				break;
			case KeyCode.M:
				m_inter.HandleInput(new UIScreenshot());
				break;
			case KeyCode.F1:
				m_inter.HandleInput(new UIShowHelp());
				break;
			default:
				break;
			}
		}
		catch (QBException e)
		{
			ShowPopupException(e);
		}
	}
	
	/**
	 * Key Release event handler
	 * */
	public void HandleKeyRelease(KeyCode code)
	{
	  switch (code)
	  {
		case KeyCode.LeftShift:
		case KeyCode.RightShift:
		case KeyCode.LeftAlt:
		case KeyCode.RightAlt:
		case KeyCode.LeftControl:
		case KeyCode.RightControl:
//		  if (!m_modeInterface.IsMouseActionInProgress())
//			SetMode(Mode.SELECT);
		  break;

		default:
		  break;
	  }
	}	
	
	
	protected void SetMode(Mode mode)
	{
		if (mode == m_mode){
			return;
		}
		
		// clean up previous mode
		switch (m_mode)
		{
		case Mode.MOVE_SELECTION:
		case Mode.MOVE_DUPLICATE:
		case Mode.MOVE_CROSSING:
			m_inter.HandleInput(new UIEndMove());
			
			break;
		case Mode.TUBE_SPLIT:
		case Mode.INJECTOR_MOVE:
			m_inter.HandleInput(new UICancelPlacement());
			break;
		default:
			break;
		}
		
		// hack
		if (mode != Mode.MOVE_DUPLICATE && 
			mode != Mode.SELECT){
			m_gui.ReleaseDuplicateMode();
		}
		
		// init new mode
		switch (mode)
		{
		case Mode.CAMERA_ROTATE:
			Cursor2D.SetCursor(Cursor2D.MODE.CAMERA_ROTATE);
			break;
		case Mode.TUBE_SPLIT:
			m_inter.HandleInput(new UIStartTubeSplit());
			break;
		case Mode.INJECTOR_MOVE:
			m_inter.HandleInput(new UIStartInjectorMove());
			break;
		case Mode.IDLE:
			m_axesProminent.Clear();
			m_inter.HandleInput(new UIShowCursor(false, 0));
			Cursor2D.SetCursor(Cursor2D.MODE.DEFAULT);
			break;
		default:
			Cursor2D.SetCursor(Cursor2D.MODE.DEFAULT);
			break;
		}

	  	m_mode = mode;
	}
	
	public void SetSoftKeyDown(KeyCode key, bool down)
	{
		switch(key){
		case KeyCode.LeftControl:
			m_SoftKeysDown[(int)SoftKey.CTRL] = down;
			break;
		case KeyCode.LeftAlt:
			m_SoftKeysDown[(int)SoftKey.ALT] = down;
			break;
		case KeyCode.LeftShift:
			m_SoftKeysDown[(int)SoftKey.SHIFT] = down;
			break;
		}
	}
		
	public void SoftButtonPressedCB(KeyCode keycode)
	{
		if (keycode == KeyCode.LeftAlt){
			SetSoftKeyDown(keycode, true);
		} else {
			HandleKeyPressed(keycode);
		}
	}
	
	public void SoftButtonReleasedCB(KeyCode keycode)
	{
		if (keycode == KeyCode.LeftAlt){
			SetSoftKeyDown(keycode, false);
		} else {
			HandleKeyPressed(keycode);
		}
	}
	
	
	
	/**
	 * Show an error on the screen with the Hud
	 */
	public void ShowPopupException(QBException e)
	{
		Popups.Instance.CreateDialog(e);
	}
	/***/
	public void BeginDragSelect()
	{
		m_gui.BeginDragSelect();
	}
	
	/**
	 * 
	 * */
	public void EndDragSelect()
	{
//		Camera cam = Camera.mainCamera;
		Rect rectScreen = m_gui.EndDragSelect();
		
		if (Mathf.Abs(rectScreen.width) > 2 && Mathf.Abs(rectScreen.height) > 2){	
			Rect rectViewport = m_coordTrans.ScreenToViewport(rectScreen);
			// select everything enclosed by the rect
			m_inter.HandleInput(new UISelectFrustrum(rectViewport, true));
		} else {
			// try hit piece by ray through rect center
			Vector2 pos = m_coordTrans.ScreenToViewport(rectScreen.center);
			input = new UIHitPieceRay(Camera.mainCamera.ViewportPointToRay(pos));
			m_inter.HandleInput(input);
			PieceHit pieceHit = ((UIHitPieceRay)input).result;
			
			if (pieceHit.hitSomething){
				m_inter.HandleInput(new UISelectPiece(pieceHit.posPiece, pieceHit.alignment, false));
			}
		}
	}
	
	/***/
	public void CancelDragSelect()
	{
		EndDragSelect();
	}
	
	/**
	 * Transform positions from world to screen space
	 * */
	public Vector3 Project(Vector3 v3)
	{
		Vector3 result = Camera.mainCamera.WorldToViewportPoint(v3);
		return result;
	}
	
	public void MoveCamera(Vector2 v)
	{
		m_inter.HandleInput(new UIMoveCamera(v));
	}
	
	/**
	 * 
	 * */
	public void MoveSelection(Vector2 offsetMouse)
	{
		m_inter.HandleInput(new UIMoveSelection(offsetMouse));
	}
	
}