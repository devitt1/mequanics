var target : Transform;
var distance = 10.0;
var orthoMatrixDirty = true;

var distanceMin = 4.0;
var distanceMax = 200.0;

var xSpeed = 250.0;
var ySpeed = 120.0;
var dSpeed = 16.0;

var yMinLimit = -20;
var yMaxLimit = 80;

private var x = 0.0;
private var y = 0.0;

private var downLastFrame = false;
private var distPinch = -1;

function Start () 
{
    var angles = transform.eulerAngles;
    x = angles.y;
    y = angles.x;

	// Make the rigid body not change rotation
   	if (rigidbody)
		rigidbody.freezeRotation = true;
		
	// This will mirror the visual output horizontally
//	var mat = Camera.mainCamera.projectionMatrix;
//	mat *= Matrix4x4.Scale(new Vector3(-1, 1, 1));
//	Camera.mainCamera.projectionMatrix = mat;
}

function LateUpdate () 
{
    if (target) 
    {
    	//rotate
		if (Input.GetMouseButton(1)){
			if (downLastFrame) {
				x += Input.GetAxis("Mouse X") * xSpeed * 0.02;
				y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02;
			}
			downLastFrame = true;
		} else {
			downLastFrame = false;
		}
		y = ClampAngle(y, yMinLimit, yMaxLimit);
		
	    // Change camera distance 
		distance -= Input.GetAxis("Mouse ScrollWheel") * dSpeed;
	    distance = Mathf.Clamp(distance, distanceMin, distanceMax);
	    if (Camera.mainCamera.orthographic) {
			orthoMatrixDirty = true;
		}
#if UNITY_ANDROID || UNITY_IPHONE
	    if (Input.touchCount >= 2) {
		    var distPinchNew = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
	    	if (distPinch != -1){
	    		var ratio = distPinch / distPinchNew;
	    		distance *= ratio;
	    		distance = Mathf.Clamp(distance, distanceMin, distanceMax);
	    	}
	    	distPinch = distPinchNew;
	    } else {
	    	distPinch = -1;
	    }
#endif
		
		// determine & apply camera angle
		var rotation = Quaternion.Euler(y, x, 0);
		var position = rotation * Vector3(0.0, 0.0, -distance) + target.position;
		//
		transform.rotation = rotation;
		transform.position = position;
    }
}

static function ClampAngle (angle : float, min : float, max : float) 
{
	if (angle < -360)
		angle += 360;
	if (angle > 360)
		angle -= 360;
	return Mathf.Clamp (angle, min, max);
}

function SetAngleX(angle : float)
{
	x = angle;
}

function SetAngleY(angle : float)
{
	y = angle;
 	y = ClampAngle(y, yMinLimit, yMaxLimit);
}

function SetDistance(dist : float)
{
	distance = dist;
}

function GetX()
{
	return x;
}

function GetY()
{
	return y;
}

function GetDistance()
{
	return distance;
}

function GetTarget()
{
	return this.target;
}

function SetTargetPos( pos : Vector3)
{
	target.position = pos;
}



