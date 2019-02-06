﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MTK_PlanetSegmentJoint : MTK_JointType
{
    public bool m_editMode = false;

    private IcoPlanet m_icoPlanet;
    private IcoSegment m_icoSegment;
    MTK_JointType_Fixed m_parentJoint;

    private float m_baseDistance = 0f;
    private Quaternion m_baserotation;
    int m_oldHeight;

    private Vector3 m_initVec;

    // Planet rotation
    private bool m_grabbing = false;
    // private ConfigurableJoint m_confJoint;

    MTK_InputManager m_currentController;

    protected void Awake()
    {
        rigidbody = transform.parent.GetComponent<Rigidbody>();
        m_icoPlanet = transform.parent.GetComponent<IcoPlanet>();
        m_icoSegment = GetComponent<IcoSegment>();
        m_parentJoint = GetComponentInParent<MTK_JointType_Fixed>();

        m_oldHeight = m_icoSegment.heightLevel;
    }

    public override bool Used()
    {
        return m_connectedGameobject;
    }

    public override bool RemoveJoint()
    {
        m_grabbing = false;
        // Destroy(m_confJoint);

        return base.RemoveJoint();
    }

    protected override bool JointWithOverride(GameObject other)
    {
        if( !Used())
        {
            m_baseDistance = Vector3.Distance(transform.position, other.transform.position);
            m_baserotation = m_parentJoint.connectedGameobject.transform.rotation;

            m_initVec = other.transform.position - transform.parent.position;

            m_currentController = other.GetComponentInParent<MTK_InputManager>();

            // joint
            // m_anchor = transform.InverseTransformPoint(other.transform.position);            

            // m_confJoint = other.AddComponent<ConfigurableJoint>();
            // m_confJoint.connectedBody = transform.parent.GetComponent<Rigidbody>();

            // m_confJoint.autoConfigureConnectedAnchor = false;
            // m_confJoint.xMotion = ConfigurableJointMotion.Locked;
            // m_confJoint.yMotion = ConfigurableJointMotion.Locked;
            // m_confJoint.zMotion = ConfigurableJointMotion.Locked;

            // m_confJoint.enableCollision = true;

            return true;
        }
        else
        {
            return false;
        }
    }

    // Vector3 m_anchor;
    // private void FixedUpdate()
    // {
    //     if(m_confJoint)
    //     {
    //         // Vector3 localJointPos = transform.InverseTransformPoint(m_confJoint.transform.TransformPoint(m_confJoint.anchor));
    //         // Vector3 targetAnchor = localJointPos.magnitude * m_anchor.normalized;
    //         // m_confJoint.connectedAnchor = targetAnchor;
    //     }
    // }

    private void Update()
    { 
        if (Used())
        {
            // Set height segment
            float delta = Vector3.Distance(transform.position, m_connectedGameobject.transform.position) - m_baseDistance;
            int heightSteps = (int)(delta / (m_icoPlanet.heightDelta * m_icoPlanet.transform.localScale.x));

            if (heightSteps != 0)
            {
                m_icoSegment.heightLevel += heightSteps;
                m_baseDistance += heightSteps * (m_icoPlanet.heightDelta * m_icoPlanet.transform.localScale.x);
                m_icoSegment.UpdateSegment();
                m_icoSegment.UpdateNeighbours();

                m_currentController.Haptic(1);

                if(m_oldHeight != m_icoSegment.heightLevel)
                {
                    if(m_icoSegment.heightLevel  <= 0)
                        AkSoundEngine.PostEvent("Water_Play", gameObject);
                    else if (heightSteps  > 0)
                        AkSoundEngine.PostEvent("Stone_Up_Play", gameObject);
                    else if (heightSteps < 0)
                        AkSoundEngine.PostEvent("Stone_Down_Play", gameObject);
                }
                
                m_oldHeight = m_icoSegment.heightLevel;
            }

            m_parentJoint.connectedGameobject.transform.rotation = Quaternion.FromToRotation(m_initVec, m_connectedGameobject.transform.position - transform.parent.position) * m_baserotation;
        }
    }
}