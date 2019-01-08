﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MTK_InertJoint : MTK_JointType
{
    public override bool Used()
    {
        return m_connectedGameobject;
    }

    protected override bool JointWithOverride(GameObject other)
    {
        return !Used();
    }

    private void Update()
    {
        if (m_connectedGameobject)
            Debug.DrawLine(transform.position, m_connectedGameobject.transform.position, Color.red);
    }
}
