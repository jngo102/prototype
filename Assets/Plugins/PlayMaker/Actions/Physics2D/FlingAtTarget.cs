using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Physics2D)]
	[Tooltip("Fling a RigidBody2D at a target in a kinematic fashion.")]
	public class FlingAtTarget : ComponentAction<Rigidbody2D>
	{
		[RequiredField]
		[CheckForComponent(typeof(Rigidbody2D))]
		[Tooltip("The GameObject with the Rigidbody2D attached.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("The GameObject to fling this Rigidbody2D at.")]
		public FsmGameObject target;

		[Tooltip("The amount of time that the projectile will be in the air for.")]
		public float airTime;

        public override void Reset()
        {
			gameObject = null;
			target = null;
			airTime = 0;
        }

        public override void OnEnter()
		{
			var go = Fsm.GetOwnerDefaultTarget(gameObject);
			if (!UpdateCacheAndTransform(go))
			{
				return;
			}

			if (target.IsNone || target == null)
            {
				Debug.Log("No target selected, choosing Player as target by default.");
				target.Value = GameObject.FindGameObjectWithTag("Player");
			}

			Vector2 selfPos = gameObject.GameObject.Value.transform.position;
			Vector2 targetPos = target.Value.transform.position;
			Vector2 delta = targetPos - selfPos;
			Vector2 throwVelocity = new Vector2(delta.x/airTime, (delta.y/airTime) -0.5f*Physics2D.gravity.y*airTime);
			rigidbody2d.velocity = throwVelocity;
		}
	}

}
