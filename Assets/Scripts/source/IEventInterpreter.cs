using UnityEngine;


/**
 * Interface for classes low-level Unity events to game-specific input
 * */
public interface IEventInterpreter
{
	void HandleButtonDown(Vector2 posViewport, int button);
	
	void HandleButtonUp(Vector2 posViewport, int button);
		
	void HandleMove(Vector2 posViewport);
	
	void HandleKeyPressed(KeyCode key);
	
	void HandleKeyRelease(KeyCode key);
}