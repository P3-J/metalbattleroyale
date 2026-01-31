using Godot;
public partial class Player
{
	[Export] Sprite2D pupSprite;
	private void CheckHandCollisionAndHoldItem()
	{
		if (!tryingToHoldItem) {
			objInHand = null;
			holdingObj = false;
		}

		if (holdingObj && objInHand != null && tryingToHoldItem)
		{
			Vector3 targetPosition = handMarker.GlobalPosition;
			Vector3 startingPosition = objInHand.GlobalPosition;
			int STR = 5; // crank that soulja 

			objInHand.LinearVelocity = (targetPosition - startingPosition) * STR;

			pupSprite.Visible = false;
			return;
		}
		
		if (!handRay.IsColliding())
		{
			pupSprite.Visible = false;
			return;
		} 

		Node3D collider = (Node3D)handRay.GetCollider();

		if (collider.IsInGroup("moveableObject"))
		{
			pupSprite.Visible = true;
			if (!tryingToHoldItem) return;
			RigidBody3D obj = (RigidBody3D)collider;
			holdingObj = true;
			objInHand = obj;

		} else
		{
			pupSprite.Visible = false;
		}

	}



}
