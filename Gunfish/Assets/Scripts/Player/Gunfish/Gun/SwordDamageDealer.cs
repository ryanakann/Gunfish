using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordDamageDealer : CollisionDamageDealer
{

    public Gunfish gunfish;

    [HideInInspector]
    public Sword sword;

    protected override void Start() {
        base.Start();
    }

    protected override void HandleCollisionEnter(GameObject src, Collision2D collision) {
        // check if src is sword and target is not self

        if (src != collisionDetector.gameObject)
            return;

        HitCounter hitCounter = collision.collider.GetComponentInParent<HitCounter>();
        if (hitCounter != null) {
            hitCounter.TakeHit(gunfish);
        }
        if (collision.collider.GetComponent<GunfishSegment>()?.gunfish == gunfish) {
            return;
        }
        if (collision.rigidbody != null) {
            var shootable = collision.rigidbody.GetComponent<Shootable>();
            if (shootable != null) {
                damageMultiplier = sword.shootableDamageMultiplier;
            }
        }
        if (trace)
            print($"{src}, {collision.rigidbody}");
        //trace = true;
        base.HandleCollisionEnter(src, collision);
    }
}
