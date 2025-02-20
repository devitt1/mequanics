using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;

using pcs;
using tools;

using parser;



/**
 * Manages a circuit
 *
 * This class holds a list of pieces and keep them in a coherent state. It
 * handles all the possible actions that one can make and supports smooth
 * animations for things like moving.
 */
public static class CircuitParser
{
	public static ParseErrors s_errorsLast;
	
	/**
	 * Read the circuit from a file
	 */	
	public static bool Generate(Circuit circuit, string input, bool invertZAxis = false)
	{
		System.Collections.Generic.Dictionary<VectorInt3, pcs.Box>[] boxMap = {
			new Dictionary<VectorInt3, pcs.Box>(new tools.Vec3Hash()), 
			new Dictionary<VectorInt3, pcs.Box>(new tools.Vec3Hash())
		};
		
		// run TinyPG
		Scanner scanner = new Scanner();
		Parser parser = new Parser(scanner);
		ParseTree tree = parser.Parse(input);
		s_errorsLast = tree.Errors;

		// UnityEngine.Debug.Log(tree.PrintTree());
		
		// Generate circuit objects
		Color colorCurrent = new Color();
		int injectionIdCurrent = -1;
		
		try {
			ParseNode nodeCircuit = tree.Nodes[0].Nodes[0];
			foreach (var n in nodeCircuit.Nodes){
				if (n.Token.Type == TokenType.Colorcommand){
					float r = Convert.ToSingle(n.Nodes[1].Token.Text);
					float g = Convert.ToSingle(n.Nodes[2].Token.Text);
					float b = Convert.ToSingle(n.Nodes[3].Token.Text);
					colorCurrent = new Color(r, g, b, 1f);
					if (colorCurrent == new Color(1,0,0)){
	//					UnityEngine.Debug.Log("Warning: Red colored blocks might make collision points hard to detect");	
					}
	//				UnityEngine.Debug.Log(colorCurrent.ToString());
				} else if (n.Token.Type == TokenType.Piececommand){
					PieceDescriptor pd = new PieceDescriptor();
					pd.color = colorCurrent;
					int inin = Convert.ToInt32(n.Nodes[1].Token.Text);
					pd.align = inin != 0;
					pd.injectionId = injectionIdCurrent;
					ParseNode nPos = n.Nodes[3];
					pd.position = new VectorInt3(
						Convert.ToInt32(nPos.Nodes[0].Token.Text), 
						Convert.ToInt32(nPos.Nodes[1].Token.Text), 
						Convert.ToInt32(nPos.Nodes[2].Token.Text));
					ParseNode nDesc = n.Nodes[2];
					if (nDesc.Token.Type == TokenType.Tubedescription){
						pd.type = PieceType.TUBE;
						pd.axis = (Axis)Enum.Parse(typeof(Axis), nDesc.Nodes[1].Token.Text.ToUpper());
						pd.length = Convert.ToInt32(nDesc.Nodes[2].Token.Text);
					} else if (n.Nodes[2].Token.Type == TokenType.Capdescription){
						pd.type = PieceType.CAP;
						pd.axis = (Axis)Enum.Parse(typeof(Axis), nDesc.Nodes[1].Token.Text.ToUpper());
						pd.length = 4;
					} if (n.Nodes[2].Token.Type == TokenType.Injectordescription){
						pd.type = PieceType.INJECTOR;
						pd.axis = (Axis)Enum.Parse(typeof(Axis), nDesc.Nodes[1].Token.Text.ToUpper());
						pd.length = 4;
					}

					// switch RH to LH for Unity
					if (invertZAxis){
						int sizeZ = pd.axis == Axis.Z ? pd.length : 1; // calculate the piece's size in z-direction
						pd.position += new VectorInt3(0, 0, sizeZ); // offset position z-coord by that
						pd.position.z = (short)-(pd.position.z); // invert position z-coord
					}
					
	//				UnityEngine.Debug.Log("Creating piece " + pd.ToString());
					circuit.CreatePiece(boxMap, pd);
				} 
				else if (n.Token.Type == TokenType.Injectioncommand){
	//				UnityEngine.Debug.Log("Injectionscommand");
					injectionIdCurrent = Convert.ToInt32(n.Nodes[1].Token.Text);
				} else if (n.Token.Type == TokenType.Tagscommand){
	//				UnityEngine.Debug.Log("Tagscommand");
				} else if (n.Token.Type == TokenType.Timecommand){
	//				UnityEngine.Debug.Log("Timecommand");
					Axis a = (Axis)Enum.Parse(typeof(Axis), n.Nodes[1].Token.Text.ToUpper());
					Direction d = Tools.GetDirectionsFromAxis(a).Value;
					if (n.Nodes[2].Token.Text == "-"){
						d = Tools.InvertDirection(d);
					}
					circuit.m_timeLocked = (true);
					circuit.m_timeDir = (d);
				} 
			}
		} catch (System.Exception e) {
			UnityEngine.Debug.Log("Circuit data parsing yielded error '" + e.Message + "'" );
			return false;
		}
		
//		UnityEngine.Debug.Log(" Generated circuit featuring " + 
//			(circuit.NumPieces0 + circuit.NumPieces1) + " pieces (" + 
//			circuit.NumPieces0 + " + " + circuit.NumPieces1 + ")");
		
		
		return true;
	}
		
}

