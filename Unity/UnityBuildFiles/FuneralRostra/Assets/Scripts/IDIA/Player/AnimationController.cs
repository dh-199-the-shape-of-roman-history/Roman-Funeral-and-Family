// ----------------------------------------------------------------------------
// This source code is provided only under the Creative Commons licensing terms stated below.
// HVWC Multiplayer Platform alpha v1 by Institute for Digital Intermedia Arts at Ball State University \is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.
// Based on a work at https://github.com/HVWC/HVWC.
// Work URL: http://idialab.org/mellon-foundation-humanities-virtual-world-consortium/
// Permissions beyond the scope of this license may be available at http://idialab.org/info/.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/4.0/deed.en_US.
// ----------------------------------------------------------------------------
using UnityEngine;

/// <summary>
///  This class handles the activation and deactivation of animations on the player.
/// </summary>
public class AnimationController : MonoBehaviour{
	
	#region Fields
	/// <summary>
	///  An instance of the Animator component on the player.
	/// </summary>
	Animator animator;
	/// <summary>
	///  An array of the names of the animations we have on our player.
	/// </summary>
	string[] animations = new string[]{"idle", "walk", "run", "jump", "sit", "fly"};
	/// <summary>
	///  The currently playing animation.
	/// </summary>
	string currentAnimation;
	#endregion
	
	#region Properties
	/// <summary>
	///  A property to get/set the animations we have on our player.
	/// </summary>
	public string[] Animations {
		get {
			return animations;
		}
		set {
			animations = value;
		}
	}
	/// <summary>
	///  A property to get/set the currently playing animation.
	/// </summary>
	public string CurrentAnimation {
		get {
			return currentAnimation;
		}
		private set {
			currentAnimation = value;
		}
	}
	#endregion
	
	#region Unity Messages
	/// <summary>
	/// A message called when the script is enabled just before Update is called.
	/// </summary>
	void Start(){
		animator = GetComponent<Animator>(); //Get the Animator component
	}
	#endregion
	
	#region Methods
	/// <summary>
	///  A method to set activate an animation on our player.
	/// </summary>
	/// <param name="animationToActivate">
	/// The name of the animation to activate.
	/// </param>
	public void ActivateAnimation(string animationToActivate){
		string target = "";
		foreach(string a in Animations){ //First, deactivate every animation on our player, except for the one we'd like to activate
			if(a==animationToActivate){
				target = a;	
			}else{
				animator.SetBool(a,false);	
			}
		}
		animator.SetBool(target,true); //Then activate our desired animation
		CurrentAnimation = target;
	}
	
	/// <summary>
	///  A method to set activate an animation on our player.
	/// </summary>
	/// <param name="index">
	/// The index of the animation to activate.
	/// </param>
	public void ActivateAnimation(int index){
		for(int i=0;i<Animations.Length;i++){ //First, deactivate every animation on our player, except for the one we'd like to activate
			if(i!=index){
				if(animator != null) {
					animator.SetBool(Animations[i],false);
				}
			}
		}
		if (animator != null) {
			animator.SetBool (Animations [index], true); //Then activate our desired animation
		}
		CurrentAnimation = Animations[index];
	}
	#endregion
	
}