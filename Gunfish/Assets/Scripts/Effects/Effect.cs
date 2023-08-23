using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.UI;

public enum EffectType { FlopModify, NoMove };


[Serializable]
public class Effect
{
    public Gunfish gunfish;
    public EffectType effectType;

    public Effect(Gunfish gunfish)
    {
        this.gunfish = gunfish;
    }

    public virtual void Check(Effect effect) 
    {
        // possibly respond to the addition of another effect
    }

    public virtual void OnAdd()
    {
        // do something to that gunfish
    }

    public virtual void Update()
    {
        // keep doing something to that gunfish 
    }

    public virtual void Merge(Effect effect)
    {
        // eat that friggin effect
    }

    public virtual void OnRemove()
    {
        // stop doing what you were doing to that poor gunfish
    }
}

[Serializable]
public class FlopModify_Effect : Effect
{
    public float flopMultiplier;

    public FlopModify_Effect(Gunfish gunfish, float flopMultiplier) : base(gunfish)
    {
        this.flopMultiplier = flopMultiplier;
        effectType = EffectType.FlopModify;
    }

    public override void OnAdd()
    {
        base.OnAdd();
        gunfish.statusData.flopForce *= flopMultiplier;
    }

    public override void Merge(Effect effect)
    {
        FlopModify_Effect t_effect = (FlopModify_Effect) effect;
        flopMultiplier += t_effect.flopMultiplier;
    }

    public override void Update() {
        base.Update();
        if (Mathf.Approximately(flopMultiplier, 0)) {
            gunfish.RemoveEffect(effectType);
        }
    }

    public override void OnRemove()
    {
        base.OnRemove();
        gunfish.statusData.flopForce = gunfish.data.flopForce;
    }
}

[Serializable]
public class NoMove_Effect : Effect {
    public int counter;

    public NoMove_Effect(Gunfish gunfish, int counter=1) : base(gunfish) {
        this.counter = counter;
        effectType = EffectType.NoMove;
    }

    public override void OnAdd() {
        base.OnAdd();
        gunfish.statusData.CanMove = false;
    }

    public override void Merge(Effect effect) 
    {
        // eat that friggin effect
        NoMove_Effect t_effect = (NoMove_Effect) effect;
        counter += t_effect.counter;
    }

    public override void Update() {
        base.Update();
        if (counter <= 0)
            gunfish.RemoveEffect(effectType);
    }

    public override void OnRemove() {
        base.OnRemove();
        gunfish.statusData.CanMove = true;
    }
}