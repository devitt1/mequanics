using UnityEngine;
using System;
using System.Collections.Generic;


/**
 * Manages the camera, model-view and projection matrices
 *
 * This camera is controlled by a point to look at (called focus), a zoom
 * (which is its distance from the point), a pitch and a yaw.
 *
 * It can achieve smooth rotations to a target point.
 * Incorporates smooth transitions between different perspectives
 */
public class CameraPlay
{
	UnityEngine.Camera m_camera;
	
	protected static Matrix4x4 m_matrixOther;
	
	public static float FOV = 75.0F;
	public static float ZMIN_ORTHO = -1000F;
	public static float ZMIN = 0.1F;
	public static float ZMAX = 100000F;
	public static float ROTATE_TIME = .5F;
	public static float PROJECT_TRANSITION_TIME = .5F;

	private float m_ratio;
	private float m_pitch;
	private float m_yaw;
	private float m_tPitch;
	private float m_tYaw;
	private float m_transit;
	private Vector3 m_focus = new Vector3();

	private Matrix4x4 m_projection = new Matrix4x4();
	private Matrix4x4 m_tProjection = new Matrix4x4();
	private float m_projTransit;
	private bool m_proj;
	
	protected MouseOrbitZoom m_cameraCS;
	protected KeyValuePair<Vector3, Vector3> m_bb; // keep camera target within this 
	//
	//
	
	public CameraPlay()
	{
		m_camera = Camera.mainCamera;
		this.m_pitch = 0F;
		this.m_yaw = 0F;
		this.m_tPitch = 0F;
		this.m_tYaw = 0F;
		this.m_transit = 0F;
		this.m_focus = new Vector3();
		this.m_projTransit = 0F;
		this.m_proj = false;
		
		this.m_ratio = m_camera.GetScreenWidth() / m_camera.GetScreenHeight();
		this.m_cameraCS = m_camera.GetComponent<MouseOrbitZoom>();
		
		this.m_bb = new KeyValuePair<Vector3, Vector3>(new Vector3(-50, -50, -50), new Vector3(50, 50, 50));
	}

	public void Update(float time)
	{
	  if (m_transit != 0F)
	  {
		m_transit += time / ROTATE_TIME;
		if (m_transit > 1)
		{
		  m_pitch = m_tPitch;
		  m_yaw = m_tYaw;
		  m_transit = 0F;
		}
		this.m_cameraCS.SetAngleX(GetCurYaw());
		this.m_cameraCS.SetAngleY(GetCurPitch());
	  }
		
	  if (m_projTransit != 0F)
	  {
		m_projTransit += time / PROJECT_TRANSITION_TIME;
		if (m_projTransit > 1)
		{
			m_projection = m_tProjection;
			m_projTransit = 0F;
			
			m_camera.orthographic = !m_proj;
		}
		
		m_camera.projectionMatrix = GetProjectionMatrix();
	  }
		
		if (m_camera.orthographic && m_cameraCS.orthoMatrixDirty) {
			m_camera.projectionMatrix = GetOrthographicProjection();
			m_cameraCS.orthoMatrixDirty = false;
		}
		
		
	}
	
	public void SetFocus(Vector3 v)
	{
	  	m_focus = v;
		m_cameraCS.SetTargetPos(m_focus);
	}
	
	public void SetDistance(float dist)
	{
		this.m_cameraCS.SetDistance(dist);
	}
	
	public void GoToYaw(float yaw)
	{
	  m_yaw = GetCurYaw();
	  m_tYaw = yaw;

	  // take the shortest path
	  float diff = m_tYaw - m_yaw;
	  if (diff > 180)
		m_yaw += 360;
	  else if (diff < -180)
		m_yaw -= 360;
	}
	
	public void GoToPitch(float pitch)
	{
	  m_pitch = GetCurPitch();
	  m_tPitch = pitch;

	  // take the shortest path
	  float diff = m_tPitch - m_pitch;
	  if (diff > 180)
		m_pitch += 360;
	  else if (diff < -180)
		m_pitch -= 360;
	}
	
	public void StartMove()
	{
		m_transit = float.Epsilon;
	}

	public void Move(Vector2 vec)
	{
	  	m_focus += vec.x * GetRight() + vec.y * GetUp();
		ApplyBoundingBox();
		m_cameraCS.SetTargetPos(m_focus);
	}

	public void SwitchCameraProjection()
	{
		m_projection = GetProjectionMatrix();
		if (m_proj){
			m_tProjection = GetOrthographicProjection();
		} else {
			m_camera.orthographic = false;
			m_camera.projectionMatrix = GetOrthographicProjection();
			m_tProjection = GetPerspectiveProjection();
		}
	  	m_projTransit = float.Epsilon;
		
		m_proj = !m_proj;	
	}

	/// Get the current projection matrix with possible interpolation if a
	/// transition is in progress

	public Matrix4x4 GetProjectionMatrix()
	{
		float correctedTransit;
		// we apply a correction to the transition to make it look more linear
		if (m_proj){
			correctedTransit = Mathf.Pow(m_projTransit, 5.0f);
		} else {
			correctedTransit = Mathf.Pow(m_projTransit, 1.0f / 5);
		}
		Matrix4x4 projection = MatrixLerp(m_projection, m_tProjection, correctedTransit);
		
		return projection;
	}
	
	protected Matrix4x4 MatrixLerp (Matrix4x4 fro, Matrix4x4 to, float t)
	{
		Matrix4x4 result = new Matrix4x4();
		for (int i = 0; i < 16; ++i){
			result[i] = fro[i] + t * (to[i] - fro[i]);
		}
		return result;
	}

	public Vector3 GetFocus()
	{
	  return m_focus;
	}

	public Vector3 GetUp()
	{
		return m_camera.camera.transform.up;
	}
	
	public Vector3 GetView()
	{
		return m_focus - m_camera.camera.transform.position;
	}

	public Vector3 GetRight()
	{
		return m_camera.camera.transform.right;
	}
	
	public Vector3 GetPosition()
	{
		return m_camera.camera.transform.position;
	}



	private Matrix4x4 GetPerspectiveProjection()
	{
		return Matrix4x4.Perspective(FOV, m_ratio, ZMIN, ZMAX);
	}

	private Matrix4x4 GetOrthographicProjection()
	{
		float halfVertSize = this.m_cameraCS.GetDistance() * Mathf.Tan((FOV / 2.0f) * Mathf.Deg2Rad);
		float halfHoriSize = halfVertSize * m_ratio;
		return Matrix4x4.Ortho(-halfHoriSize, halfHoriSize, -halfVertSize, halfVertSize, ZMIN_ORTHO, ZMAX);
	}

	private float GetCurPitch()
	{
		return m_pitch * (1-m_transit) + m_tPitch * m_transit;
	}

	private float GetCurYaw()
	{
  		return m_yaw * (1-m_transit) + m_tYaw * m_transit;
	}
	
	// clamps target vector to bounding box
	protected void ApplyBoundingBox()
	{
		m_focus = Vector3.Max(m_focus, m_bb.Key);
		m_focus = Vector3.Min(m_focus, m_bb.Value);
		m_cameraCS.SetTargetPos(m_focus);
	}
	
	public void SetBoundingBox(KeyValuePair<Vector3, Vector3> bb)
	{
		m_bb = bb;
		
		ApplyBoundingBox();
	}
}
