using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using pcs;

/**
 * 
 * */
public class TubeCS : PieceCS
{

	public override MeshFilter GetMesh()
	{
		if (m_meshFilter == null) {
			m_meshFilter = GetTransformMesh().GetComponent<MeshFilter>();	
		}
		return m_meshFilter;
	}
	
	public override Transform GetTransformMesh()
	{
		if (m_transformMesh == null) {
			m_transformMesh = transform.Find("mesh");
		}
		return m_transformMesh;
	}
	
	public override Material GetMaterial()
	{
		if (m_material == null) {
		if (Piece.s_WireframesOGL){
				m_material = GetTransformMesh().renderer.material;	
			} else {
				m_material = GetTransformMesh().renderer.sharedMaterial;	
			}
		}
		return m_material;
	}	
	
	public override Collider GetColliderMesh()
	{	
		if (m_colliderMesh == null){
			m_colliderMesh = GetTransformMesh().collider;
		}
		return m_colliderMesh;
	}
	
}
