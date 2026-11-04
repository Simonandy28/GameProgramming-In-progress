using UnityEditor;
using UnityEngine;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result){var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();result.Log("State swim="+p.animator.GetBool("IsSwimming")+" walk="+p.animator.GetBool("IsWalking")+" jump="+p.animator.GetBool("IsJumping")+" grounded="+p.GetComponent<CharacterController>().isGrounded);}
}
