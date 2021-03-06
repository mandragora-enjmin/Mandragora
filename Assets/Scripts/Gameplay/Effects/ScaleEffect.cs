﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleEffect : Effect
{
    public Vector3 originalScale;// { get; private set; }

    private void Awake()
    {
        affectsRigidbody = false;
    }

    public override bool ApplyEffect()
    {
        originalScale = transform.localScale;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;

        return true;
    }

    public override void RemoveEffect()
    {
        // transform.localScale = originalScale;

        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
    }
}
