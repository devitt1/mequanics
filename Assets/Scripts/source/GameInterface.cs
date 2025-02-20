using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

using tools;
using pcs;
using narr;
using ui;


/**
 * Class interfacing between StatePlay, GuiStatePlayCS and Game. 
 * Receives input events from EventHandlerGame and administers user interaction. 
 * Manages cursors and camera navigation.
 * Coordinates with Event scripting system (for Tutorial)
 * */
public class PieceHit
{
	public bool hitSomething;
	public VectorInt3 posPiece;
	public bool alignment;
	public Vector3 posHit;
	public int idHit;
	
	public PieceHit(VectorInt3 pp, bool align, Vector3 ph, int id)
	{
		hitSomething = true;
		posPiece = pp;
		alignment = align;
		posHit = ph;
		idHit = id;
	}
	
	public PieceHit()
	{
		hitSomething = false;
	}
}

	
public struct AxisHit
{
	public tools.Axis axis;
	public Vector3 pos;
	
	public AxisHit(tools.Axis a, Vector3 po){
		axis = a;
		pos = po;
	}
}

public class GameInterface : IInputHandler
{
	protected Game m_game;
	public EventInterpreterPlay m_interpreter;
	protected StatePlay m_state;
	
	/// The 3-arrowed cursor for moving
	protected CursorFlat m_cursor;
	protected CameraPlay m_camera;
	
	// filtering user input at times of real time events
	protected Narrative m_narrative;
	
	//
	//
	
	public GameInterface(StatePlay state)
	{	
		m_state = state;
		
		m_camera = new CameraPlay();
		m_cursor = new CursorFlat();
		
		m_interpreter = new EventInterpreterPlay(this, m_state.Gui);
		
		m_game = new Game(this, m_state);
		
		// register buttons with GUI
		m_state.Gui.RegisterActionButton("Undo", m_interpreter.SoftButtonPressedCB, KeyCode.U, true);
		m_state.Gui.RegisterActionButton("Move Injector", m_interpreter.SoftButtonPressedCB, KeyCode.A, true);
		m_state.Gui.RegisterActionButton("Simplify", m_interpreter.SoftButtonPressedCB, KeyCode.X, true);
		m_state.Gui.RegisterActionButton("Split Tube", m_interpreter.SoftButtonPressedCB, KeyCode.Z, true);
		m_state.Gui.RegisterActionButton("Teleport", m_interpreter.SoftButtonPressedCB, KeyCode.S, true);
		m_state.Gui.RegisterActionButton("Rotate Injector", m_interpreter.SoftButtonPressedCB, KeyCode.C, true);
		m_state.Gui.RegisterActionButton("Bridge", m_interpreter.SoftButtonPressedCB, KeyCode.V, true);
		m_state.Gui.RegisterActionButton("Switching", m_interpreter.SoftButtonPressedCB, KeyCode.LeftAlt, true, m_interpreter.SoftButtonReleasedCB);
//		m_state.Gui.RegisterActionButton ("Screenshot", m_interpreter.SoftButtonPressedCB, KeyCode.M, true);
		
//		m_state.Gui.RegisterActionButton (Locale.Str(PID.ShowHelp), "F1", m_interpreter.SoftButtonPressedCB, KeyCode.F1, true, HudPos.BOTTOMRIGHT);
	}
	
	public void Update(float seconds)
	{	
		if (m_narrative != null && !m_narrative.WasStarted()){
			if (m_game.HasCircuit()) {
				m_narrative.Start();
			}
		}
		
		m_interpreter.Update(seconds);
		
		if (m_narrative != null){
			m_narrative.Update(seconds);
		}
		m_game.Update(seconds);
		
	  	m_camera.Update(seconds);
	  	CircuitRenderer.Update(seconds);
		
		UpdateVisual(seconds);
	}
	
	public void UpdateVisual(float seconds) 
	{
		m_game.UpdateVisual(seconds);
		m_cursor.UpdateVisual(m_game.GetOffset());
	}
	
	public void RequestContent(Content content)
	{	
		m_game.LoadCircuit(content.uriCircuit);
		m_narrative = content.narrative;
		if (m_narrative != null){
			m_interpreter.SetInputHandler(m_narrative);
		}
	}
	
	public void RequestProgress(int slot)
	{
		m_game.LoadProgress(slot);
	}
	
	public void Cleanup()
	{
		if (m_narrative != null){
			m_narrative.Stop();
		}
	}
	
	/** 
	 * Calculate world space translation vector from viewport space mouse move coords 
	 * */
	protected float GetOffsetByAxis(Vector2 v, Axis axis)
	{
		// Determine length of axis gizmo on screen. 
		KeyValuePair<Vector3, Vector3> arrow3D = GetCursorArrow(axis);
		KeyValuePair<Vector2, Vector2> arrow2D = 
				new KeyValuePair<Vector2, Vector2>(m_interpreter.Project(arrow3D.Key), m_interpreter.Project(arrow3D.Value));
		
		Vector2 arrowProj = arrow2D.Value - arrow2D.Key;
		
		// project 2D mouse motion vector onto 2D direction of axis arrow 
		float dot = Vector2.Dot(arrowProj, v);
		float lenMoveSqrProj = dot / arrowProj.sqrMagnitude;
		
		return lenMoveSqrProj;
	}
	
	/**
	 * Center the camera and rotate it in some good way
	 *
	 * Used just after level loading.
	 */
	protected void CenterCamera(KeyValuePair<Vector3, Vector3> boundingBox)
	{
		var box = boundingBox;
		Vector3 boxCenter = (box.Key + box.Value) / 2.0f;
		
		this.m_camera.SetFocus(boxCenter);
		this.m_camera.SetBoundingBox(box);
		
		float boxMaxDimension = 
			new Vector3(box.Value.x - box.Key.x, box.Value.y - box.Key.y, box.Value.z - box.Key.z).magnitude;
		this.m_camera.SetDistance(boxMaxDimension);
	}

	
	public AxisHit HitAxis(Vector2 v)
	{
		// Perform a raycast, looking for axis gizmo
		Axis result = Axis.INVALID;
		Vector3 vResult = Vector3.zero;
		RaycastHit hit = new RaycastHit();
		Ray ray = UnityEngine.Camera.mainCamera.ViewportPointToRay(v);
		//
		if (Physics.Raycast(ray, out hit, 100000.0f, 1 << LayerMask.NameToLayer("cursorPicking"))){
			vResult = hit.point;
			GameObject go = hit.collider.gameObject;
			if (go != null){
				ArrowCS arrowCS = go.GetComponent<ArrowCS>();
				if (arrowCS != null){
					result = Tools.GetAxisFromDirection(arrowCS.dir);
				}
			}
		}
		return new AxisHit(result, vResult);
	}
	
	/***/
	protected void UpdateHud()
	{
		m_state.Gui.SetScores(m_game.Score1, m_game.Score2, m_game.IsInjectionOk(), m_game.IsStable());
	}
	
	public void NotifyCircuitChanged()
	{
		CenterCamera(m_game.GetBoundingBoxCircuit());
	}
	
	public bool CancelPlacement()
	{
		m_game.CancelPlacement();
		return true;
	}
	
	public bool SaveProgress(int slot)
	{
		m_game.SaveProgress(slot);
		return true;
	}
	
	public bool LoadProgress(int slot)
	{
		m_game.LoadProgress(slot);
		return true;
	}
	
	public void SetSoftKeyDown(KeyCode key, bool down)
	{
		m_interpreter.SetSoftKeyDown(key, down);
	}
	
	
	public void SelectionChanged()
	{
//		UpdateCursorPos();
	}
	
	public void SelectionMoved()
	{
//		UpdateCursorPos();
	}
	
	/**
	 * Get the cursor arrow segment
	 */
	protected KeyValuePair<Vector3, Vector3> GetCursorArrow(Axis a)
	{
		return m_cursor.GetArrow(a);
	}
	
	
	
	public bool HandleInput(UInput input)
	{
//		if (m_narrative != null){
//			return m_narrative.HandleInput(input);
//		} else {
			return ProcessInput(input);	
//		} 
	}
	
	protected bool ProcessInput(UInput input)
	{
		
		switch (input.Type)
		{
		// game-independent operations
		case UInput.Typ.SwitchCameraProjection: 
			return SwitchCameraProjection();
		case UInput.Typ.LookAtSceneFrom: 
			return LookAtSceneFrom(((UILookAtSceneFrom)input).dir);
		
		// Instantaneous operations (selection independent)
		case UInput.Typ.UndoLastAction: 
			return UndoLastAction();
		
		case UInput.Typ.SetTimeDirection: 
			return SetTimeDirection(((UISetTimeDirection)input).dir);
		
		case UInput.Typ.SelectPiece: 
//			return SelectPiece(((UISelectPiece)input).m_piece, ((UISelectPiece)input).m_invert);
			return SelectPiece(((UISelectPiece)input).m_pos, 
								((UISelectPiece)input).m_alignment, 
								((UISelectPiece)input).m_invert, 
								((UISelectPiece)input).m_id);
			
		case UInput.Typ.SelectBlock: 
			return SelectBlock(((UISelectBlock)input).m_piece, ((UISelectBlock)input).m_invert);	
		case UInput.Typ.SelectFrustrum: 
			return SelectFrustrum(((UISelectFrustrum)input).rectViewport, ((UISelectFrustrum)input).additive);	
		case UInput.Typ.UnselectAll: 
			return UnselectAll();
		
		//TODO
		case UInput.Typ.HitPieceRay: 
			return HitPieceRay(((UIHitPieceRay)input).ray, ref ((UIHitPieceRay)input).result);
		case UInput.Typ.GetProminentAxes: 
			return GetProminentAxes(((UIGetProminentAxes)input).p, ((UIGetProminentAxes)input).result);
//		case UInput.Typ.SetCursorAxes: 
//			return SetCursorAxes(((UISetCursorAxes)input).active, ((UISetCursorAxes)input).normal);
		case UInput.Typ.SetCursorAxisActive: 
			return SetCursorAxisActive(((UISetCursorAxisActive)input).active);
		case UInput.Typ.SetCursorAxisNormal: 
			return SetCursorAxisNormal(((UISetCursorAxisNormal)input).normal);
			
		case UInput.Typ.UpdateCursorPos: 
			return UpdateCursorPos();
		case UInput.Typ.MatchAxis: 
			return MatchAxis(((UIMatchAxis)input).pos, ((UIMatchAxis)input).offsetViewport, ((UIMatchAxis)input).candidates, ref ((UIMatchAxis)input).result);
		case UInput.Typ.ShowCursor: 
			return ShowCursor(((UIShowCursor)input).show, ((UIShowCursor)input).delay);
		case UInput.Typ.SaveProgress: 
			return SaveProgress(((UISaveProgress)input).slot);
		case UInput.Typ.LoadProgress: 
			return LoadProgress(((UILoadProgress)input).slot);
		case UInput.Typ.Screenshot: 
			return Screenshot();
		case UInput.Typ.ShowHelp: 
			return ShowHelp();
		case UInput.Typ.MoveCamera: 
			return MoveCamera(((UIMoveCamera)input).v);
		case UInput.Typ.MoveSelection: 
			return MoveSelection(((UIMoveSelection)input).v);
//		case UInput.Typ.MoveSelection: 
//			return MoveSelection(Axis axis, float distance);
			
		// Instantaneous operations (selection dependent)
		case UInput.Typ.Bridge: 
			return Bridge();
		case UInput.Typ.Teleport: 
			return Teleport();
		case UInput.Typ.Simplify: 
			return Simplify();
		case UInput.Typ.RotateInjector: 
			return RotateInjector();
		
		// Discrete timed operations
		case UInput.Typ.StartTubeSplit: 
			return StartTubeSplit();
		case UInput.Typ.StartInjectorMove: 
			return StartInjectorMove();
		case UInput.Typ.PreviewBoxAt: 
			return PreviewBoxAt(((UIPreviewBoxAt)input).pos);
		case UInput.Typ.PreviewInjectorAt: 
			return PreviewInjectorAt(((UIPreviewInjectorAt)input).pos, ((UIPreviewInjectorAt)input).alignment, ((UIPreviewInjectorAt)input).posHit);
		case UInput.Typ.CommitTubeSplit: 
			return CommitTubeSplit();
		case UInput.Typ.CommitInjectorMove: 
			return CommitInjectorMove();
		case UInput.Typ.CancelPlacement: 
			return CancelPlacement();
		
		// Continuous timed operations
		case UInput.Typ.BeginMove: 
			return BeginMove(((UIBeginMove)input).axis);
		case UInput.Typ.BeginMoveDuplicate: 
			return BeginMoveDuplicate(((UIBeginMoveDuplicate)input).axis);
		case UInput.Typ.BeginMoveCrossing: 
			return BeginMoveCrossing(((UIBeginMoveCrossing)input).axis);
		
		case UInput.Typ.EndMove: 
			return EndMove();
			
		default:
			return false;
		}
	}
	
	// 
	
	public bool CommitInjectorMove()
	{
	  	m_game.CommitInjectorMove();
		return true;
	}
	
	public bool CommitTubeSplit()
	{
	  	m_game.CommitTubeSplit();
		return true;
	}
		
//	public bool SelectPiece(Piece piece)
//	{
//		m_game.Select(piece, false);
//		return true;
//	}
	
//	public bool SelectPiece(Piece piece, bool invert)
//	{
//		m_game.Select(piece, invert);
//		return true;
//	}
	
	public bool SelectPiece(VectorInt3 pos, bool alignment)
	{
		return SelectPiece(pos, alignment, false);
	}
	
	public bool SelectPiece(VectorInt3 pos, bool alignment, bool invert)
	{
		return SelectPiece(pos, alignment, invert, -1);
	}
		
	public bool SelectPiece(VectorInt3 pos, bool alignment, bool invert, int id)
	{
		Piece p;
		if (id == -1){
			p = m_game.GetPiece(pos.ToVector3(), alignment);
		} else {
			p = m_game.GetPiece(id);
		}
		if (p == null) {
			return false;
		}
		m_game.Select(p, invert);
		return true;
	}
	

	public bool SelectFrustrum(Rect rectViewport, bool additive)
	{
		if (!additive){
			m_game.UnselectAll();
		}
		
		// normalize rectangle
		Vector2 center = rectViewport.center;
		rectViewport.width = Mathf.Abs(rectViewport.width);
		rectViewport.height = Mathf.Abs(rectViewport.height);
		rectViewport.center = center;
		
		Camera cam = Camera.mainCamera;
		Plane[] planes = new Plane[4];
		if (cam.orthographic){
			Vector3 BL = cam.ViewportToWorldPoint(new Vector3(rectViewport.xMin, rectViewport.yMin, cam.farClipPlane)); // bottom left
			Vector3 BR = cam.ViewportToWorldPoint(new Vector3(rectViewport.xMax, rectViewport.yMin, cam.farClipPlane)); // bottom right
			Vector3 TL = cam.ViewportToWorldPoint(new Vector3(rectViewport.xMin, rectViewport.yMax, cam.farClipPlane)); // top left
			Vector3 TR = cam.ViewportToWorldPoint(new Vector3(rectViewport.xMax, rectViewport.yMax, cam.farClipPlane)); // top right
			Vector3 TLN = cam.ViewportToWorldPoint(new Vector3(rectViewport.xMin, rectViewport.yMax, cam.nearClipPlane)); // top left near
			Vector3 BRN = cam.ViewportToWorldPoint(new Vector3(rectViewport.xMax, rectViewport.yMin, cam.nearClipPlane)); // bottom right near
			planes[0].Set3Points(TR, TL, TLN); // top
			planes[1].Set3Points(BL, BR, BRN); // bottom
			planes[2].Set3Points(TL, BL, TLN); // left
			planes[3].Set3Points(BR, TR, BRN); // right
			planes[0].normal.Normalize();
			planes[1].normal.Normalize();
			planes[2].normal.Normalize();
			planes[3].normal.Normalize();
		} else {
			Vector3 BL = cam.ViewportToWorldPoint(new Vector3(rectViewport.xMin, rectViewport.yMin, cam.farClipPlane)); // bottom left
			Vector3 BR = cam.ViewportToWorldPoint(new Vector3(rectViewport.xMax, rectViewport.yMin, cam.farClipPlane)); // bottom right
			Vector3 TL = cam.ViewportToWorldPoint(new Vector3(rectViewport.xMin, rectViewport.yMax, cam.farClipPlane)); // top left
			Vector3 TR = cam.ViewportToWorldPoint(new Vector3(rectViewport.xMax, rectViewport.yMax, cam.farClipPlane)); // top right
			planes[0].Set3Points(Camera.mainCamera.transform.position, TR, TL); // top
			planes[1].Set3Points(Camera.mainCamera.transform.position, BL, BR); // bottom
			planes[2].Set3Points(Camera.mainCamera.transform.position, TL, BL); // left
			planes[3].Set3Points(Camera.mainCamera.transform.position, BR, TR); // right
			planes[0].normal.Normalize();
			planes[1].normal.Normalize();
			planes[2].normal.Normalize();
			planes[3].normal.Normalize();
		}
		
		m_game.SelectFrustrum(planes);
		
		return true;
	}
	
	
	/**
	 * 	Move selection according to mouse drag (smoothly...)
	 * */
	public bool MoveSelection(Vector2 v)
	{
		if (m_game.MovingAxis != Axis.INVALID)
		{
//			float lenMoveSqrProj = GetCursorScale(v);
			float lenMoveSqrProj = GetOffsetByAxis(v, m_game.MovingAxis);
				
			// Request motion
			m_game.SmoothMoveSelection(lenMoveSqrProj, m_game.MovingAxis);
		}
		
		UpdateHud();
		return true;
	}	
	
	
	public bool ShowCursor(bool show, float delay)
	{
		m_cursor.Show(show, delay);
		return true;
	}
	
	public bool Screenshot()
	{
		string file = Application.persistentDataPath + "/" + QConfig.s_titleApp + "_" + UnityEngine.Time.time.ToString() + ".png";
		Application.CaptureScreenshot(file);
		return true;
	}
	
	public bool MoveCamera(Vector3 v)
	{
		// get base in world space
		var focus = m_camera.GetFocus();
		var up = focus + m_camera.GetUp();
		var right = focus + m_camera.GetRight();
		
		// transform in screen space
		var sfocus = m_interpreter.Project(focus);
		var sup = m_interpreter.Project(up);
		var sright = m_interpreter.Project(right);
		
		// get only the needed coordinates from vectors
		float fright = sright.x - sfocus.x;
		float fup = sup.y - sfocus.y;
		
		// get ratio
		Vector2 move = new Vector2(v.x, v.y);
		move.x /= fright;
		move.y /= fup;
		
		m_camera.Move(-move); // move to opposite drag direction 
		return true;
	}		
	
	public bool SwitchCameraProjection()
	{
	  	m_camera.SwitchCameraProjection();
		return true;
	}
	
	public bool LookAtSceneFrom(Direction d)
	{
		switch (d)
		{
		case Direction.FRONT:
		  m_camera.GoToPitch(0);
		  m_camera.GoToYaw(0);
		  break;
		case Direction.UP:
		  m_camera.GoToPitch(90);
		  m_camera.GoToYaw(0);
		  break;
		case Direction.RIGHT:
		  m_camera.GoToPitch(0);
		  m_camera.GoToYaw(-90);
		  break;
		case Direction.REAR:
		  m_camera.GoToPitch(0);
		  m_camera.GoToYaw(180);
		  break;
		case Direction.DOWN:
		  m_camera.GoToPitch(-90);
		  m_camera.GoToYaw(0);
		  break;
		case Direction.LEFT:
		  m_camera.GoToPitch(0);
		  m_camera.GoToYaw(90);
		  break;
		default:
		  System.Diagnostics.Debug.Assert(false);
		  break;
		
		}
		m_camera.StartMove();
		return true;
	}
	
	/**
	 * Returns a list of axes, ordered by the angle they form with the vector from camera to position p
	 * */ 
	public bool GetProminentAxes(Vector3 p, List<Axis> result)
	{
		// if 3 axes where hit, then discard the one at the smallest angle with the viewing direction
		List<KeyValuePair<Axis, float>> list = new List<KeyValuePair<Axis, float>>();
		Vector3 vCamToP = (p - m_camera.GetPosition()).normalized;
		foreach (Axis a in Enumerable.Range((int)Axis.X, 3)){
			float cos = Mathf.Abs(Vector3.Dot(vCamToP, Tools.GetVectorFromDirection(Tools.GetDirectionsFromAxis(a).Value).ToVector3()));
			list.Add(new KeyValuePair<Axis, float>(a, cos));
		}
		list.Sort((x, y) => x.Value.CompareTo(y.Value));
		result.Clear();
		foreach (var i in list){
			result.Add(i.Key);
		}
		return true;
	}
			
	/**
	 * Show help text on the screen with the Hud
	 */
	public bool ShowHelp()
	{
		string helpstring = GlobalMembersHelp.GetHelpString();
		Popups.Instance.CreateDialog(helpstring, "\n" + Locale.Str(PID.KeyBindings), HudPos.CENTER);
		return true;
	}
	
	/**
	 * Returns the first piece intersected by the ray from 
	 * camera towards mouse pointer pos and the 3D position of the 
	 * intersection.
	 * */
	public bool HitPieceRay(Ray ray, ref PieceHit pieceHit)
	{	
		Piece piece = null;
		Vector3 posIntersect = Vector3.zero;
		
		// Perform a raycast, looking for pieces
		RaycastHit hit = new RaycastHit();
		//
		if (Physics.Raycast(ray, out hit, 100000.0f, 1 << LayerMask.NameToLayer("blockPicking"))){
			posIntersect = hit.point;
			GameObject go = hit.collider.gameObject;
			if (go != null){
				PieceCS pieceCS = go.GetComponent<PieceCS>();
				if (pieceCS == null){ //TODO: this is dirty coding
					pieceCS = go.transform.parent.GetComponent<PieceCS>();	
				}
				if (pieceCS != null){
					piece = pieceCS.piece;
				}
			}
		}
		if (piece == null){
			pieceHit = new PieceHit();	
		} else {
			pieceHit = new PieceHit(piece.GetPosition(), piece.GetAlignment(), posIntersect, piece.Id);		
		}
		return true;
	}

	/**
	 * Returns the axis whose projection to screen space corresponds best 
	 * to a given mouse pointer 2D offset for a given 3D position
	 * */
	public bool MatchAxis(Vector3 pos, Vector2 offsetViewport, List<Axis> candidates, ref Axis result)
	{
		result = Axis.INVALID;
		
		float angleMouse = UnityEngine.Mathf.Atan2(offsetViewport.y, offsetViewport.x);
		List<Axis> axes = candidates; 
		if (candidates == null) {
			axes = Enum.GetValues(typeof(Axis)).Cast<Axis>().ToList().GetRange(0, 3);
		}
		
		// iterate axes and find the one forming the smallest angle with the mouse offset vector
		float angleDiff = Mathf.PI;
		foreach (Axis a in axes){
			KeyValuePair<Vector3, Vector3> arrow3D = 
				new KeyValuePair<Vector3, Vector3>( pos, pos + Tools.GetVectorFromDirection(Tools.GetDirectionsFromAxis(a).Value).ToVector3() );
			KeyValuePair<Vector2, Vector2> arrow2D = 
				new KeyValuePair<Vector2, Vector2>(m_interpreter.Project(arrow3D.Key), m_interpreter.Project(arrow3D.Value));
			
			float angleAxis = UnityEngine.Mathf.Atan2((arrow2D.Value.y - arrow2D.Key.y), arrow2D.Value.x - arrow2D.Key.x);
			float diff = Mathf.Deg2Rad * Mathf.Min(
					Mathf.Abs(Mathf.DeltaAngle(Mathf.Rad2Deg * angleMouse, Mathf.Rad2Deg * angleAxis)),
					Mathf.Abs(Mathf.DeltaAngle(Mathf.Rad2Deg * angleMouse, Mathf.Rad2Deg * (angleAxis + Mathf.PI)))
				);
			if (diff < angleDiff){
				angleDiff = diff;
				result = a;
			}
		}
		return true;
	}

	// selection
	
	public bool Select(Piece piece, bool invert)
	{		
		m_game.Select(piece, invert);
		return true;
	}
	
	public bool SelectBlock(Piece piece, bool invert)
	{	
		m_game.SelectBlock(piece, invert);
		return true;
	}
	
	
	// cursor

//	public bool SetCursorAxes(Axis active, Axis normal)
//	{
//		m_cursor.SetAxes(active, normal);
//		return true;
//	}
	
	public bool SetCursorAxisActive(Axis active)
	{
		m_cursor.SetAxisActive(active);
		return true;
	}
	
	public bool SetCursorAxisNormal(Axis normal)
	{
		m_cursor.SetNormal(normal);
		return true;
	}

	public bool UpdateCursorPos()
	{
		KeyValuePair<Vector3, Vector3> posSelectionCenter = m_game.GetSelectionCenter();
		m_cursor.SetPosition(posSelectionCenter.Key);
		m_cursor.SetPositionOffset(posSelectionCenter.Value);
		return true;
	}
	
	public bool EndMove()
	{
		m_game.EndMove();
		
		UpdateHud();
		return true;
	}

	public bool Bridge()
	{
		m_game.Bridge();
		return true;
	}
	
	public bool RotateInjector()
	{
		m_game.RotateInjector();
		return true;
	}
	
	public bool Simplify()
	{
		m_game.Simplify();
		return true;
	}
	
	public bool Teleport()
	{
		m_game.Teleport();
		return true;
	}
	
	public bool UndoLastAction()
	{
		m_game.UndoLastAction();
		return true;
	}
	
	public bool IsSelected(Piece piece)
	{
		return m_game.IsSelected(piece);
	}
	
	public Piece GetPiece(VectorInt3 pos, bool aligned)
	{
		return m_game.GetPiece(pos.ToVector3(), aligned);	
	}
	
	public Piece GetPiece(int id)
	{
		return m_game.GetPiece(id);	
	}
	
	public bool SetPieceID(Piece piece, int id)
	{	
		return m_game.SetPieceID(piece, id);
	}
	
	public bool UnselectAll()
	{
		m_game.UnselectAll();
		return true;
	}
	
	// discrete move actions
	
	public bool StartTubeSplit()
	{
		m_game.StartTubeSplit();
		return true;
	}
	
	public bool StartInjectorMove()
	{
		m_game.StartInjectorMove();
		return true;
	}
	
	// continuous move actions
	
	public bool BeginMove(Axis axis)
	{
		return m_game.BeginMove(axis);
	}
	
	public bool BeginMoveDuplicate(Axis axis)
	{
		return m_game.BeginMoveDuplicate(axis);
	}
	
	public bool BeginMoveCrossing(Axis axis)
	{
		return m_game.BeginMoveCrossing(axis);
	}
	
	public bool PreviewBoxAt(Vector3 pos)
	{
	  	m_game.PreviewBoxAt(pos);
		return true;
	}
	
	public bool PreviewInjectorAt(VectorInt3 pos, bool alignment, Vector3 posHit)
	{
	  	m_game.PreviewInjectorAt(pos, alignment, posHit);
		return true;
	}
	
	public bool SetTimeDirection(Direction direction)
	{
		m_game.SetTimeDirection(direction);
		return true;
	}
	
	public int GetScore1()
	{
		return m_game.Score1;
	}
}