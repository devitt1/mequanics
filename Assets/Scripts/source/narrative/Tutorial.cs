using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using narr;
using tools;
using pcs;
using ui;

/**
 * Class to control the UI and issue events during the in-game tutorial.
 * Predecessor of a system for scripted events.
 * */
public class Tutorial : Narrative
{	
	//
	//
	
	public Tutorial(IInputHandler wrapped, GameInterface gi) : base(wrapped, gi)
	{
		m_uriCircuit = "CircuitsTutorial/Tutorial.cir";
			
		m_events = new LinkedList<narr.RTEvent>();
		
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialogOK("Hello there, fellow mequanic! \nLook at this piece of hardware here!")
					, new ActHighlightLocation(new Vector3(15.5f, 0f, -14f), 25f)
				},
				new Dictionary<UInput.Typ, UInput>(){
				}
			));
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialog("This structure could power-up your quantum space vessel! \nRight-click and drag to move around it.")
					, new ActHighlightLocation(new Vector3(15.5f, 0f, -14f), 25f)
					, new ActHighlightNothing()
					, new ActUserRotateCamera()
				},
				new Dictionary<UInput.Typ, UInput>(){
				}
			));
			
			//[user interacts]
			
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialogOK("It's a working quantum computer circuit, but it still takes up way too much space!") 
					, new ActHighlightLocation(new Vector3(15.5f, 0f, -14f), 25f)	
					//TODO highlight size index in GUI here
				},
				new Dictionary<UInput.Typ, UInput>(){
				}
			));
				
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialogOK("Particularly this useless dead end here.")  
					, new ActHighlightPiece(new VectorInt3(-15, 0, -26), false)
				},
				new Dictionary<UInput.Typ, UInput>(){
				}
			));
		
		//[UI highlights particular defect strain]
			
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialog("Click the end box of the line and drag it towards the rest of the circuit."),
					new ActUserMovePiece(new VectorInt3(-15, 0, -26), false, new VectorInt3(0, 0, -26), new VectorInt3(20, 0, -26))
					, new ActHighlightPiece(new VectorInt3(-15, 0, -26), false)
				},
				new Dictionary<UInput.Typ, UInput>(){
					{ UInput.Typ.SelectPiece, new UISelectPiece(new VectorInt3(-15, 0, -26), false, false) },
					{ UInput.Typ.HitPieceRay, null },
			
//					{ UInput.Typ.UnselectAll, null },
					{ UInput.Typ.GetProminentAxes, null }, 
//					{ UInput.Typ.SetCursorAxisActive, new UISetCursorAxisActive(Axis.X) },
					{ UInput.Typ.SetCursorAxisActive, null },
					{ UInput.Typ.SetCursorAxisNormal, null },
					{ UInput.Typ.UpdateCursorPos, null },
					{ UInput.Typ.MatchAxis, null },
					{ UInput.Typ.ShowCursor, null },
					
					{ UInput.Typ.BeginMove, null },
					{ UInput.Typ.MoveSelection, null },
					{ UInput.Typ.EndMove, null }
					
				}
			));
			
			//[user interacts]
			
			//Exclamation: "Nice!"
			
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialogOK("Wow, you already cut down the circuit size by 20%!")
					, new ActHighlightGameObject("guiScore", 0.1f)
//					, new ActHighlightNothing()
					, new ActExclamation(Gui3DExclamationsCS.ItemsToSay.NICE)
				},
				new Dictionary<UInput.Typ, UInput>(){
				
				}
			));
			
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialogOK("Lets see, what else can we reduce in size ... ")  
				},
				new Dictionary<UInput.Typ, UInput>(){
				}
			));
			
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialogOK( "This noose here is much longer than it needs to be!")
					, new ActHighlightLocation(new Vector3(4, 0, -28), 6f)
				},
				new Dictionary<UInput.Typ, UInput>(){
				}
			));
			//[UI highlights particular loop]
			
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialog("Click this out-lier tube and make the loop more tight.")
					, new ActHighlightPiece(new VectorInt3(0, 0, -30), false)
					, new ActUserMovePiece(new VectorInt3(0, 0, -30), false, new VectorInt3(1, 0, -30), new VectorInt3(20, 0, -30))
				},
				new Dictionary<UInput.Typ, UInput>(){
					{ UInput.Typ.SelectPiece, new UISelectPiece(new VectorInt3(0, 0, -30), false, false) },
					{ UInput.Typ.HitPieceRay, null },
			
					{ UInput.Typ.UnselectAll, null },
					{ UInput.Typ.GetProminentAxes, null }, 
//					{ UInput.Typ.SetCursorAxisActive, new UISetCursorAxisActive(Axis.X) },
					{ UInput.Typ.SetCursorAxisActive, null },
					{ UInput.Typ.SetCursorAxisNormal, null },
					{ UInput.Typ.UpdateCursorPos, null },
					{ UInput.Typ.MatchAxis, null },
					{ UInput.Typ.ShowCursor, null },
					
					{ UInput.Typ.BeginMove, null },
					{ UInput.Typ.MoveSelection, null },
					{ UInput.Typ.EndMove, null }
				}
			));
			
			//[user interacts]
			
			//Exclamation: "Good!"
			
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialogOK("See, down another 10%! \nWe can almost fit it in your ship now.")  
					, new ActHighlightNothing()
					, new ActExclamation(Gui3DExclamationsCS.ItemsToSay.GOOD)
				} ,
				new Dictionary<UInput.Typ, UInput>(){
				}
			));
			
			//[Interface displays the quantum space vessel]
			
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialogOK("Now, this special piece can be moved to a more convenient place.")
					, new ActHighlightPiece(new VectorInt3(31, 0, -11), false)
				},
					new Dictionary<UInput.Typ, UInput>(){
				}
			));
			//[UI highlights an injector on a noose]  
			
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialog("Select this Injector and hit A or push the 'Move Injector' button to relocate that piece.")
					, new ActUserStartInjectorMove()
					, new ActHighlightPiece(new VectorInt3(31, 0, -11), false)
				},
				new Dictionary<UInput.Typ, UInput>(){
					{ UInput.Typ.SelectPiece, new UISelectPiece(new VectorInt3(31, 0, -11), false, false) },
					{ UInput.Typ.HitPieceRay, null },
					{ UInput.Typ.GetProminentAxes, null }, 
					{ UInput.Typ.UnselectAll, null },
					{ UInput.Typ.StartInjectorMove, null }
				}
			));
			
			//[user interacts]
			
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialog( "Good! Now find a better spot on the same strain for it and click there.")  
					, new ActUserCommitInjectorMove()
					, new ActHighlightPiece(new VectorInt3(0, 0, -20), false)
				},
				new Dictionary<UInput.Typ, UInput>(){
					{ UInput.Typ.HitPieceRay, null },
					{ UInput.Typ.PreviewInjectorAt, new UIPreviewInjectorAt(new VectorInt3(0, 0, -20), false, Vector3.zero) },
					{ UInput.Typ.CommitInjectorMove, null },
				}
			));
			//[UI highlights a destination for the injector]
			
			//[user interacts]
		
			//Exclamation: "Well done!"
			
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialog( 
						"Now drag the useless noose to straighten up that line.")  
						, new ActUserMovePiece(new VectorInt3(31, 0, -11), false, new VectorInt3(31, 0, -16))
						, new ActHighlightPiece(new VectorInt3(31, 0, -11), false)
						, new ActExclamation(Gui3DExclamationsCS.ItemsToSay.WELLDONE)
				},
				new Dictionary<UInput.Typ, UInput>(){	
			
					{ UInput.Typ.SelectPiece, new UISelectPiece(new VectorInt3(31, 0, -11), false, false) },
					{ UInput.Typ.HitPieceRay, null },
			
					{ UInput.Typ.UnselectAll, null },
					{ UInput.Typ.GetProminentAxes, null }, 
//					{ UInput.Typ.SetCursorAxisActive, new UISetCursorAxisActive(Axis.Z) },
					{ UInput.Typ.SetCursorAxisActive, null },
					{ UInput.Typ.SetCursorAxisNormal, null },
					{ UInput.Typ.UpdateCursorPos, null },
					{ UInput.Typ.MatchAxis, null },
					{ UInput.Typ.ShowCursor, null },
					
					{ UInput.Typ.BeginMove, null },
					{ UInput.Typ.MoveSelection, null },
					{ UInput.Typ.EndMove, null }
				}
			));
			//[UI highlights the former location of the injector]
			
			//[user interacts]
			
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialog( "Now hit the X key or press the 'Simplify' button to remove the obsolete boxes!")  
					, new ActUserSimplify() 
					, new ActHighlightPiece(new VectorInt3(30, 0, -16), false)
				},
				new Dictionary<UInput.Typ, UInput>(){
					{ UInput.Typ.Simplify, null }
				}
			));
			
			//[user interacts]
			
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialogOK( "Nice! You made the circuit more simple! \nNow you can easily telescope this noose as well.")  
					, new ActHighlightLocation(new Vector3(30, 0, -18), 6f)
				},
				new Dictionary<UInput.Typ, UInput>(){
				}
			));
				
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialog( 
						"From now try to get the overall size below 64.")  
					, new ActHighlightNothing()
					, new ActUserAchievementThreshold(64)
				},
				new Dictionary<UInput.Typ, UInput>(){	
					{ UInput.Typ.SelectPiece, null },
					{ UInput.Typ.HitPieceRay, null },
			
					{ UInput.Typ.UnselectAll, null },
					{ UInput.Typ.GetProminentAxes, null }, 
					{ UInput.Typ.SetCursorAxisActive, null },
					{ UInput.Typ.SetCursorAxisNormal, null },
					{ UInput.Typ.UpdateCursorPos, null },
					{ UInput.Typ.MatchAxis, null },
					{ UInput.Typ.ShowCursor, null },
					
					{ UInput.Typ.BeginMove, null },
					{ UInput.Typ.MoveSelection, null },
					{ UInput.Typ.EndMove, null },
			
					{ UInput.Typ.Simplify, null }
				}
			));
			//[Interface highlights a tube that became eligible for telescoping]
			
			//[user interacts and underpasses the next achievement threshold]
			
			//Exclamation: "Awesome!"
			
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialogOK("You reduced overall size to less than 64! \nThat's one out of 3 stars for you!")  
					//TODO highlight stars 
					, new ActHighlightNothing()
					, new ActExclamation(Gui3DExclamationsCS.ItemsToSay.AWESOME)
					, new ActHighlightGameObject("guiStars", 0.3f)
				},
				new Dictionary<UInput.Typ, UInput>(){
				}
			));
	
			//[Interface highlights first star becoming active]
			
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialogOK("At last the most powerful tool for you, \nthat's Teleportation:")  
					, new ActHighlightNothing()
				},
				new Dictionary<UInput.Typ, UInput>(){
				}
			));
			
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialog("You can remove an entire loop if it carries a single injector or cap! \nSelect this piece!") 
					, new ActUserSelect(1)
					, new ActHighlightPiece(1)
				},
				new Dictionary<UInput.Typ, UInput>(){
					{ UInput.Typ.SelectPiece, new UISelectPiece(new VectorInt3(30, 0, -30), false, false) },
					{ UInput.Typ.HitPieceRay, null },
					{ UInput.Typ.GetProminentAxes, null },
					{ UInput.Typ.UnselectAll, null }
				}
			));
			
			//[UI highlights an injector]
			
			//[user interacts]
			
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialog( "Now hit S or press the 'Teleport' button.")  
					, new ActUserTeleport()
					, new ActHighlightNothing()
				},
				new Dictionary<UInput.Typ, UInput>(){
					{ UInput.Typ.Teleport, null }
				}
			));
			
			//[user interacts]
			
			//Exclamation: "Great!"
			
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialogOK("See, the loop is gone while the injector got teleported to the braided circuitry line.")  
					, new ActExclamation(Gui3DExclamationsCS.ItemsToSay.GREAT)
				},
				new Dictionary<UInput.Typ, UInput>(){
				}
			));
			
			//[UI highlights the teleported injector]
			
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialogOK("Great! We vastly simplified this structure!")  
				},
				new Dictionary<UInput.Typ, UInput>(){
				}
			));
			
		m_events.AddLast(
			new narr.RTEvent(
				new List<Act>(){ 
					new ActDialogOK("This is all for now. \nYou have the skills now to reach 2 or even 3 stars on this puzzle!")  
				},
				new Dictionary<UInput.Typ, UInput>(){
				}
			));
		
		//
		
		m_piecesVIP = new LinkedList<PieceDescVip>();
		
		m_piecesVIP.AddLast(
			new PieceDescVip(new VectorInt3(31, 0, -11), false, 1)
		);
	}

}
