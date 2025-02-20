using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using pcs;
using tools;


/**
 * Class to make dynamic prefab instantiation convenient for non-MonoBehavior objects
 * 
 * Singleton class
 * */
public class PrefabHubCS : MonoBehaviour 
{
	protected static PrefabHubCS s_prefabHub;
	public static PrefabHubCS Instance {
		get { return s_prefabHub; }
	}
	
	public GameObject tube;
	public GameObject cube;
	public GameObject injector;
	public GameObject cap;
	public GameObject arrow;
	public GameObject cursorFlat;
	public GameObject cursorPlastic;
	public GameObject cubeGhost;
	public GameObject injectorGhost;
	public GameObject dialog2D;
	public GameObject dialog3D;
	public GameObject boundingBox;
	public GameObject activityIndicator;
	public GameObject mask;
	public GameObject activityIndicator3D;
	
	//
	//
	
	void OnEnable()
	{
		if (PrefabHubCS.s_prefabHub == null){
			s_prefabHub = this;
		} else {
			System.Diagnostics.Debug.Assert(false);
		}
	}
	
	void OnDisable()
	{
		PrefabHubCS.s_prefabHub = null;
	}
	
	void Start() {}
	
	void Update() {}
	
	//
	//
	
	public PieceCS GetPiece(PieceType type)
	{
		GameObject prefab = null;
		
		switch (type){
		case PieceType.TUBE:
			prefab = tube;
			break;
		case PieceType.BOX:
			prefab = cube;
			break;
		case PieceType.INJECTOR:
			prefab = injector;
			break;
		case PieceType.CAP:
			prefab = cap;
			break;
		case PieceType.BOXGHOST:
			prefab = cubeGhost;
			break;
		case PieceType.INJECTORGHOST:
			prefab = injectorGhost;
			break;
		default:
			prefab = cube;
			break;
		}
		
		GameObject go = (GameObject)UnityEngine.Object.Instantiate(prefab);
		PieceCS pieceCS = go.GetComponent<PieceCS>();
//		System.Diagnostics.Debug.Assert(pieceCS != null);
		List<Vector3> wireframe = CircuitRenderer.GetWireframe(type, pieceCS.GetMesh());
//		System.Diagnostics.Debug.Assert(wireframe != null);
		pieceCS.SetWireframe(wireframe);
		
		return pieceCS;
	}
	
	public CursorFlatCS GetPrefabCursorFlat()
	{
		GameObject go = (GameObject)Instantiate(cursorFlat);
		go.name = "cursorFlat";
		return go.GetComponent<CursorFlatCS>();
	}
	
	public CursorPlasticCS GetPrefabCursorPlastic()
	{
		GameObject go = (GameObject)Instantiate(cursorPlastic);
		go.name = "cursorPlastic";
		return go.GetComponent<CursorPlasticCS>();
	}
	
	public GuiDialogCS GetDialog()
	{
		GameObject go = (GameObject)Instantiate(dialog2D);
		return go.GetComponent<GuiDialogCS>();
	}
	
	public GuiDialog3DCS GetDialog3D()
	{
		GameObject go = (GameObject)Instantiate(dialog3D);
		go.transform.parent = Camera.mainCamera.transform;
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.identity;
		return go.GetComponent<GuiDialog3DCS>();
	}
	
	public BoundingBoxCS GetBoundingBox()
	{
		GameObject go = (GameObject)Instantiate(boundingBox);
		return go.GetComponent<BoundingBoxCS>();
	}
	
	public ActivityIndicatorCS GetActivityIndicator()
	{
		GameObject go = (GameObject)Instantiate(activityIndicator);
		return go.GetComponent<ActivityIndicatorCS>();
	}
	
	public GameObject GetActivityIndicator3D()
	{
		GameObject go = (GameObject)Instantiate(activityIndicator3D);
		go.transform.parent = Camera.mainCamera.transform;
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.identity;
		go.SetActive(false);
		return go;
	}
	
	public MaskCS GetMask()
	{
		GameObject go = (GameObject)Instantiate(mask);
		go.SetActive(false);
		return go.GetComponent<MaskCS>();;
	}
}

