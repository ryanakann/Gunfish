using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

[Serializable]
public class TransformTuple {
    public Vector2 position;
    public float rotation; // z rotation
}


[CreateAssetMenu(menuName = "Gunfish/Gun Data", fileName = "New Gun Data")]
public class GunData : ScriptableObject {
    [Header("Gun")]
    public List<TransformTuple> gunBarrels = new List<TransformTuple>();
    public float kickback;
    public float range;
    public float damage;
    public float knockback;
    public int maxAmmo;

    public GameObject gunBarrelPrefab;
    public float fireCooldown;
    public float reload;
    public float reloadWait;
}