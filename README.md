CharacterController2D
=====================

CharacterController2D is similar to the built-in Unity CharacterController component. It has a similar API (mainly a *move* method that takes a delta movement) and provides a firm base with which to make a super solid controller using Unity's 2D system.

A simple demo scene is included along with a few Spelunky sprites (which obviously should not be used in your game!). An overview video showing how to get started is available on the prime31studios YouTube channel [here](http://www.youtube.com/watch?v=KpnImAdiiaQ&feature=youtu.be).



A Word about Physics Based Movement
=====================

It is important to note that with regard to the CharacterController2D when the term "physics based movement" is used it does not mean that forces are used to move the player. CharacterController2D is made for cases where you need precision and full control so forces are never used to move the Transform. When the term "physics based movement" is used here it just means that the RigidBody2D's MovePosition method is used to move the Transform. For the CharacterController2D's non physics based movement Transform.Translate is used. In both cases (physics based and non physics based) all movement distance calculations are still made manually by the CharacterController2D.



Demo Scene
=====================

The included demo scene is minimal to keep things as simple as possible. It has very basic input detection (using the arrow keys to move and jump) and a simple character sprite/Animator setup. This should be simple enough for anyone with a rudimentry knowledge of Unity to read through and understand.



Basic Setup
=====================

Setup is pretty simple: drag the CharacterController2D onto your player/enemy GameObject and set the *platformMask* in the inspector to contain any layers that you want the player to collide with. If you will be using one way platforms, you can set the *oneWayPlatformMask* as well. Note that one way platforms should be EdgeCollider2Ds. *ProTip*: make sure your player is on a separate layer than your platforms so that she doesn't try to collide with herself! You will also want to make sure you BoxCollider2D is centered and has a zero horizontal offset. Flipping requires setting the scale.x to -1 so a horizontal offset other than zero will result in the collider jumping around.

You can then tweak the *totalHorizontalRays* and *totalVerticalRays* to your liking. When in the editor rays will be drawn with *Debug.DrawRay* so that you can make sure the ray resolution is appropriate for your smallest platform size. To turn off ray debugging just comment out the *#define DEBUG_CC2D_RAYS* line at the top of the CharacterController2D.cs file.

To move the player around just call *move* and provide it a delta movement (the physical distance the player should move this frame). You can subscribe to the *onControllerCollidedEvent* to be notified of all collisions if you need them.

In order to be able to receive trigger events (onTriggerEnter/Stay/Exit) the CharacterController2D needs to be told which layers it should interact with. Select the layers that should fire trigger events via the *triggerMask* field in the inspector.



Special Notes for Using Physics-based Movement
=====================

If you set *usePhysicsForMovement* to true some special consideration need to be made:

* the RigidBody2D on the CharacterController2D should still be kinematic
* the RigidBody2D on the CharacterController2D should have Interpolate set to None or Extrapolate
* all the usual Unity rules for physics-based movement will also apply: you should only move the CharacterController2D in FixedUpdate, dont gather input in FixedUpdate, etc
* you may want to experiment with the TimeManager's Fixed Timestep value to get better results



License
-----
[Attribution-NonCommercial-ShareAlike 3.0 Unported](http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode) with [simple explanation](http://creativecommons.org/licenses/by-nc-sa/3.0/deed.en_US). You are free to use the CharacterController2D in any and all games that you make. You cannot sell the CharacterController2D directly or as part of a larger game asset.
