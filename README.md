CharacterController2D
=====================

CharacterController2D is similar to the built-in Unity CharacterController compoent. It has a similar API (mainly a move method that takes a delta movement) and is a really good base to make a super solid controller using Unity's new 2D goodies.

A simple demo scene is included along with a few Spelunky sprites (which obviously should not be used in your game!).



Demo Scene
=====================

The included demo scene is a minimal example to keep things simple. It has very basic input detection (using the arrow keys) and a simple character sprite/Animator setup.



Basic Setup
=====================

Setup is pretty simple: drag the CharacterController2D onto your player/enemy GameObject and set the *platformMask* in the inspector to contain any layers that you want the player to collide with. If you will be using one way platforms, you can set the *oneWayPlatformMask* as well. Note that one way platforms should be EdgeCollider2Ds.

From there you can tweak the *totalHorizontalRays* and *totalVerticalRays* to your liking. When in the editor rays will be drawn with *Debug.DrawRay* so that you can make sure the ray resolution is appropriate for your smallest platform size. To turn off ray debugging just comment out the *#define DEBUG* line at the top of the CharacterController2D.cs file.

To move the player around just call *move* and provide it a delta movement. You can subscribe to the *onControllerCollidedEvent* to be notified of all collisions if you need them.



License
-----
[Attribution-NonCommercial-ShareAlike 3.0 Unported](http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode) with [simple explanation](http://creativecommons.org/licenses/by-nc-sa/3.0/deed.en_US). You are free to use the CharacterController2D in any and all games that you make. You cannot sell the CharacterController2D directly or as part of a larger game asset.
