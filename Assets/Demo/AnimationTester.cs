using UnityEngine;
using System.Collections;



/// <summary>
/// This is a very basic class that skips all the Mecanim features and just plays animations based on input
/// </summary>
public class AnimationTester : MonoBehaviour
{
	Animator _animator;
	CharacterController2D _controller;


	void Awake()
	{
		_animator = GetComponent<Animator>();
		_controller = GetComponent<CharacterController2D>();
	}


	void Update()
	{
		if( _controller.isGrounded && ( Input.GetKey( KeyCode.LeftArrow ) || Input.GetKey( KeyCode.RightArrow ) ) )
			_animator.goToStateIfNotAlreadyThere( Animator.StringToHash( "Base Layer.Run" ) );
		else if( _controller.isGrounded )
			_animator.goToStateIfNotAlreadyThere( Animator.StringToHash( "Base Layer.Idle" ) );


		if( Input.GetKeyDown( KeyCode.UpArrow ) )
			_animator.Play( Animator.StringToHash( "Base Layer.Jump" ) );
	}

}
