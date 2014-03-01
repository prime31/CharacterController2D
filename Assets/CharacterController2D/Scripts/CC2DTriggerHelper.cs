using UnityEngine;
using System.Collections;


/// <summary>
/// this script just captures the OnTrigger* messages and passes them on to the CharacterController2D
/// </summary>
public class CC2DTriggerHelper : MonoBehaviour
{
	private CharacterController2D _parentCharacterController;


	public void setParentCharacterController( CharacterController2D parentCharacterController )
	{
		_parentCharacterController = parentCharacterController;
	}


	#region MonoBehavior

	void OnTriggerEnter2D( Collider2D col )
	{
		if( col.isTrigger )
			_parentCharacterController.OnTriggerEnter2D( col );
	}


	void OnTriggerStay2D( Collider2D col )
	{
		if( col.isTrigger )
			_parentCharacterController.OnTriggerStay2D( col );
	}


	void OnTriggerExit2D( Collider2D col )
	{
		if( col.isTrigger )
			_parentCharacterController.OnTriggerExit2D( col );
	}

	#endregion

}
