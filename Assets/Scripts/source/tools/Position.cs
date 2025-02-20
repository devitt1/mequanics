using UnityEngine;
using System.Collections.Generic;

namespace tools 
{

public enum Direction
{
	LEFT,
	RIGHT,
	DOWN,
	UP,
	REAR,
	FRONT,
	INVALID 
}

public enum Axis
{
  X = 0,
  Y = 1,
  Z = 2,
  INVALID = 3
}

	
public static partial class Tools
{
	public static Direction[] GetDirectionSet(VectorInt3 v)
	{
		Direction x = (v.x == 0 ? Direction.INVALID : v.x > 0 ? Direction.RIGHT : Direction.LEFT);
		Direction y = (v.y == 0 ? Direction.INVALID : v.y > 0 ? Direction.UP : Direction.DOWN);
		Direction z = (v.z == 0 ? Direction.INVALID : v.z > 0 ? Direction.FRONT : Direction.REAR);
		return new tools.Direction[3]{x, y, z};
	}
		
	public static Axis GetAxisFromDirection(Direction d)
	{
	  switch (d)
	  {
		case Direction.UP:
		case Direction.DOWN:
		  return Axis.Y;
		case Direction.LEFT:
		case Direction.RIGHT:
		  return Axis.X;
		case Direction.FRONT:
		case Direction.REAR:
		  return Axis.Z;
		default:
		  throw new ex.InvalidValue();
	  }
	}

	public static Direction GetPositiveDirection(Direction d)
	{
	  switch (d)
	  {
		case Direction.UP:
		case Direction.DOWN:
		  return Direction.UP;
		case Direction.LEFT:
		case Direction.RIGHT:
		  return Direction.RIGHT;
		case Direction.FRONT:
		case Direction.REAR:
		  return Direction.FRONT;
		default:
		  throw new ex.InvalidValue();
	  }
	}

	public static Direction GetNegativeDirection(Direction d)
	{
	  switch (d)
	  {
		case Direction.UP:
		case Direction.DOWN:
		  return Direction.DOWN;
		case Direction.LEFT:
		case Direction.RIGHT:
		  return Direction.LEFT;
		case Direction.FRONT:
		case Direction.REAR:
		  return Direction.REAR;
		default:
		  throw new ex.InvalidValue();
	  }
	}

	public static VectorInt3 GetVectorFromDirection(Direction d)
	{
	  switch (d)
	  {
		case Direction.UP:
			return new VectorInt3(0, 1, 0);
		case Direction.DOWN:
			return new VectorInt3(0, -1, 0);
		case Direction.LEFT:
			return new VectorInt3(-1, 0, 0);
		case Direction.RIGHT:
			return new VectorInt3(1, 0, 0);
		case Direction.FRONT:
			return new VectorInt3(0, 0, 1);
		case Direction.REAR:
			return new VectorInt3(0, 0, -1);
		default:
		  throw new ex.InvalidValue();
	  }
	}

	public static KeyValuePair<Direction, Direction> GetDirectionsFromAxis(Axis a)
	{
	  switch (a)
	  {
		case Axis.X:
			return new KeyValuePair<Direction, Direction>(Direction.LEFT, Direction.RIGHT);
		case Axis.Y:
			return new KeyValuePair<Direction, Direction>(Direction.DOWN, Direction.UP);
		case Axis.Z:
			return new KeyValuePair<Direction, Direction>(Direction.REAR, Direction.FRONT);
		default:
		  throw new ex.InvalidValue();
	  }
	}

	public static Axis GetComplementaryAxis(Axis o1, Axis o2)
	{
	  switch (o1)
	  {
		case Axis.X:
		  switch (o2)
		  {
			case Axis.X:
				return Axis.INVALID;
			case Axis.Y:
				return Axis.Z;
			case Axis.Z:
				return Axis.Y;
			default:
				throw new ex.InvalidValue();
		  }
		case Axis.Y:
		  switch (o2)
		  {
			case Axis.X:
				return Axis.Z;
			case Axis.Y:
				return Axis.INVALID;
			case Axis.Z:
				return Axis.X;
			default:
				throw new ex.InvalidValue();
		  }
		case Axis.Z:
		  switch (o2)
		  {
			case Axis.X:
				return Axis.Y;
			case Axis.Y:
				return Axis.X;
			case Axis.Z:
				return Axis.INVALID;
			default:
				throw new ex.InvalidValue();
		  }
		default:
		  throw new ex.InvalidValue();
	  }
	}

	public static KeyValuePair<Axis, Axis> GetComplementaryAxis(Axis a)
	{
	  switch (a)
	  {
		case Axis.X:
		  return new KeyValuePair<Axis, Axis>(Axis.Y, Axis.Z);
		case Axis.Y:
		  return new KeyValuePair<Axis, Axis>(Axis.X, Axis.Z);
		case Axis.Z:
		  return new KeyValuePair<Axis, Axis>(Axis.X, Axis.Y);
		default:
		  throw new ex.InvalidValue();
	  }
	}

	public static byte GetIntFromAxis(Axis a)
	{
	  switch (a)
	  {
		case Axis.X:
			return 0;
		case Axis.Y:
			return 1;
		case Axis.Z:
			return 2;
		default:
		  throw new ex.InvalidValue();
	  }
	}

	public static Direction InvertDirection(Direction d)
	{
	  switch (d)
	  {
		case Direction.UP:
			return Direction.DOWN;
		case Direction.DOWN:
			return Direction.UP;
		case Direction.LEFT:
			return Direction.RIGHT;
		case Direction.RIGHT:
			return Direction.LEFT;
		case Direction.FRONT:
			return Direction.REAR;
		case Direction.REAR:
			return Direction.FRONT;
		default:
		  throw new ex.InvalidValue();
	  }
	}

	public static bool DirectionIsPositive(Direction d)
	{
	  switch (d)
	  {
		case Direction.UP:
			return true;
		case Direction.DOWN:
			return false;
		case Direction.LEFT:
			return false;
		case Direction.RIGHT:
			return true;
		case Direction.FRONT:
			return true;
		case Direction.REAR:
			return false;
		default:
		  throw new ex.InvalidValue();
	  }
	}
		
	public static string Vec3ToString(Vector3 v)
	{
		string result = '(' + v[0] + ", " + v[1] + ", " + v[2] + ')';
		return result;
	}
	
	public static string Vec2ToString(Vector2 v) 
	{
		string result = '(' + v[0] + ", " + v[1] + ')';
		return result;
	}
		
	public static float dot(Vector3 u, Vector3 v)
	{
	  	return u.x * v.x + u.y * v.y + u.z * v.z;
	}
		
	public static float dot(VectorInt3 u, VectorInt3 v)
	{
	  	return u.x * v.x + u.y * v.y + u.z * v.z;
	}
		
	
}
	
}