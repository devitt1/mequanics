using UnityEngine;



public interface ICoordinateTranlator
{
	Vector2 GuiToScreen(Vector2 pos);
	
	Vector2 ScreenToViewport(VectorInt2 pos);

	Vector2 ScreenToViewport(Vector2 pos);

	Rect GuiToScreen(Rect r);

	Rect ScreenToViewport(Rect r);
	
	VectorInt2 ViewportToScreen(Vector2 pos);
	
}
