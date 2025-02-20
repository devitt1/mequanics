using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using System;

using tools;
using pcs;


/**
 * Manages the game itself
 *
 * This class manages the game itself, after a level has been loaded. It
 * handles the possibles actions and make the Circuit react in consequence.
 */
public class Game
{
	static Game s_instance;
	public Game Instance { get{ return s_instance; } }
	
	private static bool s_helpNoteShown = true;

	protected CircuitRenderer m_circuitRend;

	//
	//
	
	protected string m_circuitSource;
	protected Circuit m_circuit;
	
	protected uint m_width;
	protected uint m_height;

	protected float m_time;
	
	protected GameInterface m_interface;
	protected Achievements m_achievements;

	protected GridRenderer m_gridRend;

	/// Keep trace of which arrow was clicked for a move
	protected Axis m_movingAxis;
	public Axis MovingAxis { get { return m_movingAxis; } }
	
	protected StatePlay m_state;
	
	public int Score1 { 
		//TODO Don't compute circuits bounding box every frame!
		get { 
			if(m_circuit != null) {
				var box = m_circuit.GetBoundingBox();
				Vector3 size = box.Value - box.Key;
				return Score.ScoreFromSize(new VectorInt3(size));
			} else {
				return 0;
			}
		}
	}
	public int Score2 { 
		get { 
			if (m_circuit != null) {
				return Score.GetCrossSectionScore(m_circuit); 
			} else {
				return 0;
			} 
		}
	}
	
	public KeyValuePair<Vector3, Vector3> CircuitBB 
	{
		get { 
			if (m_circuit != null) { 
				return m_circuit.GetBoundingBox(); 
			} else { 
				return new KeyValuePair<Vector3, Vector3>(Vector3.zero, Vector3.zero); 
			}
		}
	}
	
	//
	//
	
	public Game(GameInterface inter, StatePlay state)
	{
		s_instance = this;
		
		this.m_interface = inter;
		this.m_state = state;
		
		this.m_time = 0F;
		this.m_movingAxis = Axis.INVALID;
		
		m_achievements = new Achievements(this);
		
		m_circuitRend = new CircuitRenderer();
		m_circuitSource = null;
		
		InitVisual();
	}
	
	public void InitVisual()
	{
		m_gridRend = new GridRenderer();
		m_gridRend.InitVisual();
		m_gridRend.SetCircuitBB(new Vector3(-50f, -50f, -50f), new Vector3(50f, 50.0f, 50.0f));
	}
	
	//
	//

	public void LoadCircuit(string uri)
	{	
		m_state.ShowBusy(true);
		
		bool isUrl = uri.Contains("www."); 
//		isUrl = isUrl && System.Uri.IsWellFormedUriString(uri, UriKind.Absolute); //TODO3 Check properly if uri is an URL	
		if (isUrl){
			// Load from web
				SaveLoad.Instance.LoadFileText(uri, IoLoc.WEB, this.ChangeCircuit);
		} else {
			if (Application.isWebPlayer || !uri.Contains(Application.persistentDataPath)) {
				// Load from bundled circuits
				SaveLoad.Instance.LoadFileText(uri, IoLoc.BUNDLED, this.ChangeCircuit);
#if ! UNITY_WEBPLAYER	
			} else {
				// Load from user's persistant data path on local machine 
				SaveLoad.Instance.LoadFileText(uri, IoLoc.LOCAL, this.ChangeCircuit);
#endif
			}
		}
	}
	
	public IEnumerator LoadCircuitCR(string uri)
	{	
		bool isUrl = uri.Contains("www."); 
//		isUrl = isUrl && System.Uri.IsWellFormedUriString(uri, UriKind.Absolute); //TODO3 Check properly if uri is an URL	
		if (isUrl){
			// Load from web
				yield return AsyncCS.Instance.StartCoroutine(SaveLoad.Instance.LoadFileTextCR(uri, IoLoc.WEB, this.ChangeCircuit));
		} else {
			if (Application.isWebPlayer || !uri.Contains(Application.persistentDataPath)) {
				// Load from bundled circuits
				yield return AsyncCS.Instance.StartCoroutine(SaveLoad.Instance.LoadFileTextCR(uri, IoLoc.BUNDLED, this.ChangeCircuit));
#if ! UNITY_WEBPLAYER	
			} else {
				// Load from user's persistant data path on local machine 
				yield return AsyncCS.Instance.StartCoroutine(SaveLoad.Instance.LoadFileTextCR(uri, IoLoc.LOCAL, this.ChangeCircuit));
#endif
			}
		}
		yield break;
	}
	
	/**
	 * Read the circuit from a string
	 */
	public bool ReadCircuit(string str, Circuit circuit)
	{
		circuit.m_timeLocked = false;
		circuit.m_timeDir = Direction.INVALID;
   
		bool success = CircuitParser.Generate(circuit, str, true);
		if (true){
			foreach (var error in CircuitParser.s_errorsLast){
				string strErr = error.Message + "\n(at position " + error.Position +")";
				Popups.Instance.CreateDialog(strErr, "Circuit data parsing yielded the following error:", HudPos.BOTTOMRIGHT);
			}
		} 
		
		if (success){
			circuit.GenerateIds();
			circuit.m_injectionBad = false;
		}
		return success;
	}
	
	public bool ChangeCircuit(string filename, string source)
	{
		bool sourceContainsCir;
			
		// translate source from .tex format if given
		if (filename.EndsWith(".tex"))
		{
			Popups.Instance.CreateDialog(".tex formatted circuit data cannot be processed at the moment", "Apologies");
			sourceContainsCir = false;
			
			// Code diabled for licencing reasons
//			StringWriter swDest = new StringWriter();
//			TexImporter importer = new TexImporter();
//			importer.Import(source, swDest);
//			source = swDest.ToString();
//			sourceContainsCir = true;
			
#if UNITY_STANDALONE || UNITY_EDITOR
//			//DEBUG spit out resulting .cir to file
//			// Works only for files in persistent data path so far
//			string filenameCir = Application.persistentDataPath + "/" + System.IO.Path.GetFileName(filename) + ".cir";
//			StreamWriter fs = new StreamWriter(filenameCir);
//			fs.Write(source);
//			fs.Close();
#endif			
		} else {
			sourceContainsCir = true;
		}
		
		bool success = false;
		
		// Create circuit from cir source
		Circuit circuit = null;
		if (sourceContainsCir) {
			circuit = new Circuit();
			if (ReadCircuit(source, circuit)){
				success = true;
			} else {
				circuit = null;
			}
		}
		
		if (circuit != null) 
		{
			if (m_circuit != null){
				m_circuit.UnselectAll();
				m_circuit.UninitVisual();
				m_circuit = null;
			}
			
			m_circuit = circuit;
	  		m_uriCircuit = filename;
			
			// 
			m_circuit.SelectAll();
			m_circuit.Simplify(false); // 
			
			//
			var bb = this.m_circuit.GetBoundingBox();
			UpdateGrid(bb);
			
			int[] thresholds;
			if (AchievementsDatabase.ThresholdsCustom.ContainsKey(filename)){
				//TODO load custom thresholds here
				thresholds = AchievementsDatabase.ThresholdsCustom[filename];
			} else {
				int score = this.Score1;
				thresholds = new int[]{(int)(score * 0.8f), (int)(score * 0.4f), (int)(score * 0.1f)};
			}
			m_achievements.SetThresholds(thresholds); //TODO make this dynamic
	  		UpdateAchievements();
			
			m_interface.NotifyCircuitChanged();
			SelectionChanged();
		} else {
			if (m_circuit == null){ // go back to selection menu if previously there was no circuit
				StateMachine.Instance.RequestStateChange(EState.MENUCIRCUITSELECT);
			}
		}
		
		m_state.ShowBusy(false);
		m_state.NotifyUriCircuit(this.m_uriCircuit);
		
		return success;
	}

	/**
	 * Save the game to a file
	 */
	public void SaveProgress(int slot)
	{
		m_state.ShowBusy(true);
		
#if ! UNITY_WEBPLAYER
		// everything but webplayer: store savegame to file
	
		string filename = Application.persistentDataPath + "/" + "save" + slot;
		FileStream s = new FileStream(filename, FileMode.Create, FileAccess.Write);
		BinaryWriter bw = new BinaryWriter(s);
	
		WriteProgress(bw);
	
		s.Close();
#else
		// webplayer: store savegame to memory
		
		MemoryStream s = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(s);
		
		WriteProgress(bw);
		
		byte[] buffer = s.GetBuffer();
		SaveLoad.Instance.SetSaveData(slot, buffer);
			
		s.Close();
#endif
		
		m_state.ShowBusy(false);
	}
	
	protected void WriteProgress(BinaryWriter bw)
	{
		// Store circuit name
		bw.Write(m_uriCircuit.Length);
		bw.Write(m_uriCircuit.ToCharArray());
		
		// store step history
		m_circuit.SaveSteps(bw);
	}
	
	
	/**
	 *  Load a circuit and player progress from file
	 */	
	public void LoadProgress(int slot)
	{
#if UNITY_WEBPLAYER
		{
			// webplayer: load savegame from memory
			byte[] buffer = SaveLoad.Instance.GetSaveData(slot);
			if (buffer == null)
			{
				throw new QBException(QBException.Type.SAVE_SLOTUNUSED);
			}
			MemoryStream s = new MemoryStream(buffer);
			BinaryReader br = new BinaryReader(s);
			
			AsyncCS.Instance.StartCoroutine(ReadProgressCR(br));
		}
#else
		{
			// everything but webplayer: load savegame from file
			FileStream s;
			try
			{
				string filename = Application.persistentDataPath + "/" + "save" + slot;
				s = new FileStream(filename, FileMode.Open, FileAccess.Read);	
			} 
			catch (FileNotFoundException)
			{
				throw new QBException(QBException.Type.SAVE_SLOTUNUSED);
			}
			BinaryReader br = new BinaryReader(s);
	
			AsyncCS.Instance.StartCoroutine(ReadProgressCR(br));
		}
#endif
	}
	
	protected IEnumerator ReadProgressCR(BinaryReader br)
	{
		m_state.ShowBusy(true);
		
		// Read circuit name
		int uriLength = br.ReadInt32();
		string uri = new string(br.ReadChars(uriLength));
		
		// Load circuit
	  	yield return AsyncCS.Instance.StartCoroutine(LoadCircuitCR(uri));
		
		// Read step history
		m_circuit.ReadSteps(br);
		
		br.BaseStream.Close();
		
		m_state.ShowBusy(false);
		
		m_achievements.Update();
	  	UpdateAchievements();
	}
	
	public void Update(float seconds)
	{	
		if (m_circuit != null){
			m_circuit.Update();
				
//			if (UnityEngine.Debug.isDebugBuild){
//			  	m_circuit.FullCoherenceCheck();
//			}
			
			
			// Display help note the first time a circuit is loaded
			if (!s_helpNoteShown){
				#if ! (UNITY_ANDROID || UNITY_ANDROID)
				Popups.Instance.CreateDialog(Locale.Str(PID.PressF1ForHelp), "", HudPos.CENTER); 
				#endif
				s_helpNoteShown = true;
			}
			
		}
	}
	
	public void UpdateVisual(float seconds)
	{
		if (m_circuit != null){
			m_circuit.UpdateVisual();	
		
	  		m_circuitRend.UpdateVisual(m_circuit);
		}	
	}
	
	/**
	 * Show an error on the screen with the Hud
	 */
	public void ShowPopup(string str)
	{
		Popups.Instance.CreateDialog(str, "\n" + Locale.Str(PID.ItDoesntWorkLikeThat), HudPos.CENTER);
	}
	
	protected void UpdateGrid(KeyValuePair<Vector3, Vector3> boundingBox)
	{
		m_gridRend.SetCircuitBB(boundingBox.Key, boundingBox.Value);
	}
	
	public void StartTubeSplit()
	{
		if (m_circuit != null){
			m_circuit.StartTubeSplit();
		}
	}
	
	
	public void SelectBlock(Piece piece, bool invert)
	{	
		if (m_circuit != null){
			m_circuit.SelectBlock(piece, invert);
			
			SelectionChanged();
		}
	}
		
	/***/
	public void Select(Piece piece)
	{
		Select(piece, false);
	}
	
	/***/
	public void Select(Piece piece, bool invert)
	{	
		if (m_circuit != null){
			if (invert)
			{
				if (piece != null){
					if (piece.IsSelected()){
						m_circuit.Unselect(piece);
					} else {
						m_circuit.Select(piece);
					}
				}
			}
			else
			{
				if (piece != null) {
			  		m_circuit.Select(piece);
				} else {
					m_circuit.UnselectAll();
				}
		  	}
			
			SelectionChanged();
		}
	}
	
	public void SelectFrustrum(Plane[] planes)
	{	
		if (m_circuit != null){
			m_circuit.SelectFrustrum(planes);
			
			SelectionChanged();
		}
	}
	
	public bool IsSelected(Piece piece)
	{
		return piece.IsSelected();
	}
	
	public Piece GetPiece(Vector3 pos, bool aligned)
	{
		if (m_circuit != null){
			return m_circuit.GetPiece(aligned, new VectorInt3(pos));	
		} else {
			return null;
		}
	}
	
	public Piece GetPiece(int id)
	{
		if (m_circuit != null){
			return m_circuit.GetPiece(id);	
		} else {
			return null;
		}
	}
	
	
	public bool SetPieceID(Piece piece, int id)
	{
		if (m_circuit != null){
			return m_circuit.SetPieceID(piece, id);
		} else {
			return false;
		}
	}

	//
	// 
	
	public void StartInjectorMove()
	{
		if (m_circuit != null){
			m_circuit.StartInjectorMove();
		}
	}
	
	public void PreviewInjectorAt(VectorInt3 posPiece, bool alignment, Vector3 posHit)
	{
	  	m_circuit.PreviewInjectorAt(posPiece, alignment, posHit);
	}
	
	public void CommitInjectorMove()
	{
		m_circuit.CommitInjectorMove();
		UpdateAchievements();
	}
	
	/**
	 * 	Move selection by one grid unit instantaenously
	 * */
	public void MoveSelection(Direction d)
	{
		m_circuit.MoveSelection(d);
		m_circuit.ResetOffsets();
		m_circuit.CheckCollisions();
		
		SelectionMoved();
		UpdateAchievements();
	}
	
	public void UpdateCursorSize(VectorInt2 v)
	{
	}
	
	public KeyValuePair<Vector3, Vector3> GetSelectionCenter()
	{
		if (m_circuit != null){
			return m_circuit.GetSelectionCenter();
		} else {
			return new KeyValuePair<Vector3, Vector3>();
		}
	}
	
	
	public void CommitTubeSplit()
	{
		m_circuit.CommitTubeSplit();
	}
	
	public void EndMove()
	{
		if (m_movingAxis != Axis.INVALID){
			m_circuit.CommitMove();
			m_movingAxis = Axis.INVALID;
		}
		SelectionMoved();
		
		UpdateAchievements();
	}
	
	
	public void PreviewBoxAt(Vector2 v)
	{
		Ray ray = UnityEngine.Camera.mainCamera.ViewportPointToRay(v);
	  	m_circuit.PreviewBoxAt(ray);
	}
	
	public void UndoLastAction()
	{
		if (m_circuit != null){
			m_circuit.UndoLastAction();
			
			SelectionChanged();
			UpdateAchievements();
		}
	}
	
	public void RotateInjector()
	{
		if (m_circuit != null){
			m_circuit.RotateInjector();
			SelectionChanged();
		}
	}
	
	public void Simplify()
	{
		if (m_circuit != null){
			m_circuit.Simplify();
			SelectionChanged();
		}
	}
	
	public void Bridge()
	{
		if (m_circuit != null){
			m_circuit.Bridge();
			SelectionChanged();
		}
	}
	
	public void Teleport()
	{
		if (m_circuit != null){
			m_circuit.Teleport();
			SelectionChanged();
			UpdateAchievements();
		}
	}
	
	public void CancelPlacement()
	{
		m_circuit.CancelPlacement();
		
		UpdateAchievements();
	}
	
	public void SetTimeDirection(Direction d)
	{
		if (m_circuit != null){
			m_circuit.SetTimeDirection(d);
			UpdateAchievements();
		}
	}

	/**
	 * Circuit file name
	 *
	 * Only used when loading a save yet, because we need to reload the circuit
	 * before loading a save.
	 */
	protected string m_uriCircuit;
	
	public void UnselectAll()
	{
		if (m_circuit != null){
			m_circuit.UnselectAll();
			
			SelectionChanged();
		}
		
	}
	
	public void SelectPiece(Piece piece)
	{
		if (m_circuit != null){
			m_circuit.Select(piece);
		}
	}
	
	
	public bool BeginMoveCrossing(Axis axis)
	{	
		if (m_circuit != null){
			m_movingAxis = axis;
			m_circuit.StartCrossing();
		}
		return true;
	}
			
	public bool BeginMoveDuplicate(Axis axis)
	{	
		bool success = true;
		if (m_circuit != null){
			try 
			{
				m_circuit.StartDuplicate(axis);
			} 
			catch (QBException) 
			{
				success = false;
				throw new QBException(QBException.Type.DUP_IMPOSSIBLE);
			}
			
			if (success){
				m_movingAxis = axis;
			}
		}
		return success;
	}

	public bool BeginMove(Axis axis)
	{
		m_movingAxis = axis;
		return true;
	}
	
	protected void UpdateAchievements()
	{
		m_achievements.Update();
	}
	
	/**
	 * Return true if circuit has injectors ordered correctly
	 */
	public bool IsInjectionOk()
	{
		if (m_circuit != null){
			return m_circuit.IsInjectionOk();
		} else {
			return true;
		}
	}
	
	public bool IsStable()
	{
		if (m_circuit != null){
			return m_circuit.IsStable();		
		} else {
			return true;
		}
	}
	
	public float GetOffset()
	{
		if (m_circuit != null){
			return m_circuit.Offset;		
		} else {
			return 0;
		}
	}
	
	
	public void SmoothMoveSelection(float offset, Axis a)
	{
		if (m_circuit != null){
			bool movedAStep = m_circuit.SmoothMoveSelection(offset, a);
			if (movedAStep) {
				SelectionMoved();
			}
		}
		
	}
	
	public KeyValuePair<Vector3, Vector3> GetBoundingBoxCircuit()
	{
		if (m_circuit != null){
			return m_circuit.GetBoundingBox();
		} else {
			return new KeyValuePair<Vector3, Vector3>();
		}
	}
	
	protected void SelectionChanged()
	{
		m_interface.SelectionChanged();
	}
	
	protected void SelectionMoved()
	{
		m_interface.SelectionMoved();
	}
	
	public int GetSelectionCount()
	{
		if (m_circuit != null){
			return m_circuit.SelectionCount;
		} else {
			return 0;
		}
	}
	
	public void AchievementChanged(int onetwothree)
	{
		m_state.Gui.Stars = onetwothree;
	}
	
	public bool HasCircuit()
	{
		return m_circuit != null;
	}
}
