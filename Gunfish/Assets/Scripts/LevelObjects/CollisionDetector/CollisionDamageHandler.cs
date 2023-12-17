using Cinemachine;
using SolidUtilities.UnityEngineInternals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionData {
    public float timestamp;
    public float oomph;
    public Collision2D collision;
    public ContactPoint2D[] contacts;
    public int numCollisions=0;
}

public class CollisionTracker {
    public Dictionary<GameObject, CollisionData> tracker = new Dictionary<GameObject, CollisionData>();

    void AddNewTarget(GameObject target) {
        if (tracker.ContainsKey(target) == false) {
            tracker[target] = new CollisionData();
            tracker[target].timestamp = Time.time;
        }
    }

    void AddCollisionData(GameObject target, Collision2D collision, float oomph) {
        tracker[target].collision = collision.ShallowCopy();
        tracker[target].contacts = collision.contacts;
        tracker[target].oomph = oomph;
        tracker[target].timestamp = Time.time;
        tracker[target].numCollisions++;
    }

    public void AddTarget(GameObject target, Collision2D collision, float oomph = 0) {
        AddNewTarget(target);
        AddCollisionData(target, collision, oomph);
    }

    public bool AddTarget(GameObject target, Collision2D collision, float oomph, float collisionTimeThreshold, float oomphThreshold) {
        AddNewTarget(target);
        if (Time.time - tracker[target].timestamp < collisionTimeThreshold) {
            if (oomph > tracker[target].oomph) {
                AddCollisionData(target, collision, oomph);
                return true;
            }
        }
        return false;
    }
}

public class CollisionDamageHandler : MonoBehaviour {

    public CollisionTracker collisionTracker = new CollisionTracker();
    int checkCollisions;
    float oomphThreshold;

    public bool dealDamage = true;

    static float collisionTimeThreshold = 0.1f;

    public CompositeCollisionDetector collisionDetector;

    private void Start() {
        if (collisionDetector == null) {
            collisionDetector = GetComponent<CompositeCollisionDetector>();
        }
        collisionDetector.OnComponentCollideEnter += HandleCollisionEnter;
    }


    private void Update() {
        // iterate over oomphs
        // if sufficient time has passed, pass to the relevant 
        if (checkCollisions <= 0) {
            checkCollisions = 0;
            return;
        }

        List<GameObject> removeList = new List<GameObject>();
        foreach (var target in collisionTracker.tracker) {
            if ((Time.time - target.Value.timestamp) > collisionTimeThreshold || target.Key == null) {
                removeList.Add(target.Key);
                checkCollisions--;
                // apply damage to the target
                if (target.Key != null)
                    target.Key.GetComponent<CollisionDamageReceiver>().Damage(new CollisionHitObject(target.Value.collision, target.Value.contacts, gameObject, target.Value.oomph));
            }
        }

        foreach (var target in removeList) {
            collisionTracker.tracker.Remove(target);
        }
    }

    void HandleCollisionEnter(GameObject src, Collision2D collision) {
        var target = collision.collider.gameObject;
        var subDetector = collision.collider.GetComponent<SubCollisionDetector>();
        if (subDetector != null) {
            target = subDetector.parentDetector.gameObject;
        }

        float oomph = src.GetComponent<OomphCalculator>().Oomph(collision);

        // if not enough oomph, just return
        if (oomph <= oomphThreshold)
            return;

        if (collisionTracker.AddTarget(target, collision, oomph, collisionTimeThreshold, oomphThreshold))
            checkCollisions++;
    }
}