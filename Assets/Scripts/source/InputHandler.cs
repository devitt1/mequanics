using UnityEngine;
using System.Collections.Generic;

using tools;
using pcs;

using ui;

/**
 * Interface for classes handling preprocessed user input
 * Prominently implemented by class GameInterface and called by EventInterpreterPlay.
 * 
 * */
public interface IInputHandler
{
	/**
	 * Accepts user input commands complying to the interface defined by subclasses of UInput.
	 * returns true if the input was handled,  false if it was ignored.
	 * */
	bool HandleInput(UInput input);
}

