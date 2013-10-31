#define DEBUG
using UnityEngine;
using System;
using System.Collections.Generic;


[RequireComponent( typeof( BoxCollider2D ) )]
public class CharacterController2D : MonoBehaviour
{
	#region internal types

	private enum MoveDirection : int
	{
		Right = 1,
		Left = -1,
		Up = 1,
		Down = -1
	}

	private struct CharacterRaycastOrigins
	{
		public Vector3 topRight;
		public Vector3 topLeft;
		public Vector3 bottomRight;
		public Vector3 bottomLeft;
	}

	public class CharacterCollisionState2D
	{
		public bool right;
		public bool left;
		public bool above;
		public bool below;
		public bool becameGroundedThisFrame;
		
		
		public void reset()
		{
			right = left = above = below = becameGroundedThisFrame = false;
		}
		
		
		public override string ToString()
		{
			return string.Format( "[CharacterCollisionState2D] r: {0}, l: {1}, a: {2}, b: {3}", right, left, above, below );
		}	
	}

	#endregion


	#region properties and fields

	public event Action<RaycastHit2D> onControllerCollidedEvent;

	/// <summary>
	/// toggles if the RigidBody2D velocity should be used for movement or if Transform.Translate will be used
	/// </summary>
	public bool usePhysicsForMovement = false;

	/// <summary>
	/// defines how far in from the edges of the collider rays are cast from. If cast with a 0 extent it will often result in ray hits that are
	/// not desired (for example a foot collider casting horizontally from directly on the surface can result in a hit)
	/// </summary>
	[Range( 0, 0.3f )]
	public float skinWidth = 0.02f;

	/// <summary>
	/// mask with all layers that the player should interact with
	/// </summary>
	public LayerMask platformMask = 0;

	/// <summary>
	/// mask with all layers that should act as one-way platforms. Note that one-way platforms should always be EdgeCollider2Ds
	/// </summary>
	public LayerMask oneWayPlatformMask = 0;

	[Range( 0, 90f )]
	public float slopeLimit = 30f;

	/// <summary>
	/// curve for multiplying speed based on slope (negative = downwards)
	/// </summary>
	public AnimationCurve slopeSpeedMultiplier = new AnimationCurve( new Keyframe( 0, 1 ), new Keyframe( 90, 0 ) );

	[Range( 2, 20 )]
	public int totalHorizontalRays = 8;
	[Range( 2, 20 )]
	public int totalVerticalRays = 4;


	[HideInInspector]
	public new Transform transform;
	[HideInInspector]
	public BoxCollider2D boxCollider;

	[HideInInspector]
	[NonSerialized]
	public CharacterCollisionState2D collisionState = new CharacterCollisionState2D();
	[HideInInspector]
	[NonSerialized]
	public Vector3 velocity;
	public bool isGrounded { get { return collisionState.below; } }

	#endregion


	/// <summary>
	/// holder for our raycast origin corners (TR, TL, BR, BL)
	/// </summary>
	private CharacterRaycastOrigins _raycastOrigins;

	/// <summary>
	/// stores our raycast hit during movement
	/// </summary>
	private RaycastHit2D _raycastHit;

	// horizontal/vertical movement data
	private float _verticalDistanceBetweenRays;
	private float _horizontalDistanceBetweenRays;


	#region Monobehaviour

	void Awake()
	{
		// add our one-way platforms to our normal platform mask so that we can land on them from above
		platformMask |= oneWayPlatformMask;

		// cache some components
		transform = GetComponent<Transform>();
		boxCollider = GetComponent<BoxCollider2D>();

		// figure out the distance between our rays in both directions
		// horizontal
		var colliderUseableHeight = boxCollider.size.y * Mathf.Abs( transform.localScale.y ) - ( 2f * skinWidth );
		_verticalDistanceBetweenRays = colliderUseableHeight / ( totalHorizontalRays - 1 );

		// vertical
		var colliderUseableWidth = boxCollider.size.x * Mathf.Abs( transform.localScale.x ) - ( 2f * skinWidth );
		_horizontalDistanceBetweenRays = colliderUseableWidth / ( totalVerticalRays - 1 );
	}

	#endregion


	[System.Diagnostics.Conditional( "DEBUG" )]
	private void DrawRay( Vector3 start, Vector3 dir, Color color )
	{
		Debug.DrawRay( start, dir, color );
	}


	#region Movement

	/// <summary>
	/// resets the raycastOrigins to the current extents of the box collider inset by the skinWidth. It is inset
	/// to avoid casting a ray from a position directly touching another collider which results in wonky normal data.
	/// </summary>
	/// <param name="futurePosition">Future position.</param>
	/// <param name="deltaMovement">Delta movement.</param>
	private void primeRaycastOrigins( Vector3 futurePosition, Vector3 deltaMovement )
	{
		_raycastOrigins.topRight = transform.position + new Vector3( boxCollider.size.x, boxCollider.size.y );
		_raycastOrigins.topRight.x -= skinWidth;
		_raycastOrigins.topRight.y -= skinWidth;

		_raycastOrigins.topLeft = transform.position + new Vector3( -boxCollider.size.x, boxCollider.size.y );
		_raycastOrigins.topLeft.x += skinWidth;
		_raycastOrigins.topLeft.y -= skinWidth;

		_raycastOrigins.bottomRight = transform.position + new Vector3( boxCollider.size.x, -boxCollider.size.y );
		_raycastOrigins.bottomRight.x -= skinWidth;
		_raycastOrigins.bottomRight.y += skinWidth;

		_raycastOrigins.bottomLeft = transform.position + new Vector3( -boxCollider.size.x, -boxCollider.size.y );
		_raycastOrigins.bottomLeft.x += skinWidth;
		_raycastOrigins.bottomLeft.y += skinWidth;
	}


	/// <summary>
	/// we have to use a bit of trickery in this one. The rays must be cast from a small distance inside of our
	/// collider (skinWidth) to avoid zero distance rays which will get the wrong normal. Because of this small offset
	/// we have to increase the ray distance skinWidth then remember to remove skinWidth from deltaMovement before
	/// actually moving the player
	/// </summary>
	private void moveHorizontally( ref Vector3 deltaMovement )
	{
		var isGoingRight = deltaMovement.x > 0;
		var rayDistance = Mathf.Abs( deltaMovement.x ) + skinWidth;
		var rayDirection = isGoingRight ? Vector2.right : -Vector2.right;
		var initialRayOrigin = isGoingRight ? _raycastOrigins.bottomRight : _raycastOrigins.bottomLeft;

		for( var i = 0; i < totalHorizontalRays; i++ )
		{
			var ray = new Vector2( initialRayOrigin.x, initialRayOrigin.y + i * _verticalDistanceBetweenRays );

			DrawRay( ray, rayDirection, Color.red );
			_raycastHit = Physics2D.Raycast( ray, rayDirection, rayDistance, platformMask & ~oneWayPlatformMask );
			if( _raycastHit )
			{
				// the bottom ray can hit slopes but no other ray can so we have special handling for those cases
				if( i == 0 && handleHorizontalSlope( ref deltaMovement, Vector2.Angle( _raycastHit.normal, Vector2.up ), isGoingRight ) )
				{
					if( onControllerCollidedEvent != null )
						onControllerCollidedEvent( _raycastHit );

					break;
				}

				// set our new deltaMovement and recalculate the rayDistance taking it into account
				deltaMovement.x = _raycastHit.point.x - ray.x;
				rayDistance = Mathf.Abs( deltaMovement.x );

				// remember to remove the skinWidth from our deltaMovement
				if( isGoingRight )
				{
					deltaMovement.x -= skinWidth;
					collisionState.right = true;
				}
				else
				{
					deltaMovement.x += skinWidth;
					collisionState.left = true;
				}

				if( onControllerCollidedEvent != null )
					onControllerCollidedEvent( _raycastHit );

				// we add a small fudge factor for the float operations here. if our rayDistance is smaller
				// than the width + fudge bail out because we have a direct impact
				if( rayDistance < skinWidth + 0.001f )
					break;
			}
		}
	}


	private bool handleHorizontalSlope( ref Vector3 deltaMovement, float angle, bool isGoingRight )
	{
		// disregard 90 degree angles (walls)
		if( angle == 90f )
			return false;

		// if we can walk on slopes and our angle is small enough we need to move up
		if( angle < slopeLimit )
		{
			// we only need to adjust the y movement if we are not jumping
			// TODO: this uses a magic number which isn't ideal!
			if( deltaMovement.y < 0.07f )
			{
				// apply the slopeModifier. perhaps we should divide by deltaTime then modify the speed and multiply
				// by deltaTime again to get the distance...
				var slopeModifier = slopeSpeedMultiplier.Evaluate( angle );
				deltaMovement.x *= slopeModifier;

				if( isGoingRight )
				{
					deltaMovement.x -= skinWidth;
					collisionState.right = true;
				}
				else
				{
					deltaMovement.x += skinWidth;
					collisionState.left = true;
				}

				// smooth y movement when we climb
				deltaMovement.y = Mathf.Abs( Mathf.Tan( angle * Mathf.Deg2Rad ) * deltaMovement.x );

				collisionState.below = true;
			}
		}
		else // too steep. get out of here
		{
			//Debug.LogWarning( "too steep yo " + angle );
			deltaMovement.x = 0;
		}

		return true;
	}


	private void moveVertically( ref Vector3 deltaMovement )
	{
		var isGoingUp = deltaMovement.y > 0;
		var rayDistance = Mathf.Abs( deltaMovement.y ) + skinWidth;
		var rayDirection = isGoingUp ? Vector2.up : -Vector2.up;
		var initialRayOrigin = isGoingUp ? _raycastOrigins.topLeft : _raycastOrigins.bottomLeft;

		// if we are moving up, we should ignore the layers in oneWayPlatformMask
		var mask = platformMask;
		if( isGoingUp )
			mask &= ~oneWayPlatformMask;
		
		for( var i = 0; i < totalVerticalRays; i++ )
		{
			var ray = new Vector2( initialRayOrigin.x + i * _horizontalDistanceBetweenRays, initialRayOrigin.y );
			
			DrawRay( ray, rayDirection, Color.red );
			_raycastHit = Physics2D.Raycast( ray, rayDirection, rayDistance, mask );
			if( _raycastHit )
			{
				// set our new deltaMovement and recalculate the rayDistance taking it into account
				deltaMovement.y = _raycastHit.point.y - ray.y;
				rayDistance = Mathf.Abs( deltaMovement.y );

				// remember to remove the skinWidth from our deltaMovement
				if( isGoingUp )
				{
					deltaMovement.y -= skinWidth;
					collisionState.above = true;
				}
				else
				{
					deltaMovement.y += skinWidth;
					collisionState.below = true;
				}

				if( onControllerCollidedEvent != null )
					onControllerCollidedEvent( _raycastHit );

				// we add a small fudge factor for the float operations here. if our rayDistance is smaller
				// than the width + fudge bail out because we have a direct impact
				if( rayDistance < skinWidth + 0.001f )
					return;
			}
		}
	}
	
	#endregion


	public void move( Vector3 deltaMovement )
	{
		// save off our current grounded state
		var wasGroundedBeforeMoving = collisionState.below;

		// clear our state
		collisionState.reset();

		var desiredPosition = transform.position + deltaMovement;
		primeRaycastOrigins( desiredPosition, deltaMovement );

		// first we check movement in the horizontal dir
		if( deltaMovement.x != 0 )
			moveHorizontally( ref deltaMovement );

		// next, check movement in the vertical dir
		if( deltaMovement.y != 0 )
			moveVertically( ref deltaMovement );

		// move then update our state
		if( usePhysicsForMovement )
		{
			rigidbody2D.velocity = deltaMovement / Time.fixedDeltaTime;
			velocity = rigidbody2D.velocity;
		}
		else
		{
			transform.Translate( deltaMovement );
			velocity = deltaMovement / Time.deltaTime;
		}

		// set our becameGrounded state based on the previous and current collision state
		if( !wasGroundedBeforeMoving && collisionState.below )
			collisionState.becameGroundedThisFrame = true;
	}

}
