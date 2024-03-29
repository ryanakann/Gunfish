using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(SwordDamageDealer))]
public class Sword : Gun
{
    SwordDamageDealer damageDealer;
    public float damageMultiplier = 1, dashingDamageMultiplier = 3, shootableDamageMultiplier = 3;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        damageDealer = GetComponent<SwordDamageDealer>();
        damageDealer.sword = this;
        if (damageDealer.collisionDetector == null)
            damageDealer.SetupCollisionDetector(gunfish.RootSegment.GetComponent<CompositeCollisionDetector>());
        damageDealer.gunfish = gunfish;
    }

    protected override void Update() {
        base.Update();
        damageDealer.damageMultiplier = fireCooldown_timer > 0 ? dashingDamageMultiplier: damageMultiplier;
    }

    protected override void _Fire() {
        // don't do anything?
    }
}
