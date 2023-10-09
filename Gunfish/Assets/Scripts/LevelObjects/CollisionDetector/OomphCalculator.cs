using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OomphCalculator : MonoBehaviour
{
    public Rigidbody2D rb;

    [HideInInspector]
    public Vector2 lastVelocity;

    static float massScale = 1f, velocityScale = 1f;
    public float Momentum { get { return (rb.mass * massScale) * (lastVelocity.magnitude * velocityScale); } }

    float impulseThreshold = 4f;
    float collisionAngleThreshold = 30f;

    // Start is called before the first frame update
    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }


    private void FixedUpdate() {
        lastVelocity = rb.velocity;
    }


    public bool OnTarget(Vector2 target) {
        return lastVelocity.magnitude > 0 && Vector3.Angle(lastVelocity, target - (Vector2)transform.position) <= collisionAngleThreshold;
    }

    public float Oomph(Collision2D collision) {
        print($"I am {gameObject.name}");

        var other = collision.gameObject.GetComponent<OomphCalculator>();

        if (other == null) {
            return 0;
        }

        float impulse = collision.contacts[0].normalImpulse;

        if (impulse < impulseThreshold || !OnTarget(collision.contacts[0].point)) {
            return 0;
        }

        float oomph = Momentum;
        if (other.OnTarget(collision.contacts[0].point) && other.lastVelocity.magnitude > 0) {
            oomph = Momentum * Mathf.Min(1f, Momentum / other.Momentum);
        }
        print($"OOMPH: {oomph}");
        return oomph;
    }
}