using UnityEngine;
using System.Collections;


public static class AnimatorExtensions
{
	/// <summary>
	/// Gos to state only if it is not already in the state
	/// </summary>
	public static void goToStateIfNotAlreadyThere( this Animator self, int stateHash )
	{
		if( self.GetCurrentAnimatorStateInfo( 0 ).nameHash != stateHash )
			self.Play( stateHash );
	}
}
