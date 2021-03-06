using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class MTK_Interactable : MonoBehaviour
{
    [SerializeField] AK.Wwise.Event m_wOnCollision;

    [SerializeField] public Vector3 m_upwardRotation;

    [SerializeField] public bool isDistanceGrabbable = true;
    [SerializeField] public bool isDroppable = true;

    [SerializeField] private bool m_isGrabbable = true;
    public bool isGrabbable
    {
        get { return m_isGrabbable; }
        set
        {
            if (value != m_isGrabbable)
            {
                m_isGrabbable = value;
                onIsGrabbableChange.Invoke(this);
            }
        }
    }
    public UnityEventMTK_Interactable onIsGrabbableChange = new UnityEventMTK_Interactable();

    Outline m_outline;

    public MTK_JointType jointType { get { return m_joints[m_indexJointUsed]; } }
    private MTK_JointType[] m_joints;
    private int m_indexJointUsed = 0;
    public int IndexJointUsed { get { return m_indexJointUsed; } set { m_indexJointUsed = Mathf.Clamp(value, 0, m_joints.Length - 1); } }

    [SerializeField] AK.Wwise.Event m_wOnUseStart;
    [SerializeField] UnityEvent m_onUseStart;
    [SerializeField] AK.Wwise.Event m_wOnUseStop;
    [SerializeField] UnityEvent m_onUseStop;
    [SerializeField] AK.Wwise.Event m_wOnGrabStart;
    [SerializeField] UnityEvent m_onGrabStart;
    [SerializeField] AK.Wwise.Event m_wOnGrabStop;
    [SerializeField] UnityEvent m_onGrabSop;

    [HideInInspector] public bool isDistanceGrabbed = false;

    public bool UseEffects
    {
        set
        {
            foreach (var effect in GetComponents<Effect>())
            {
                if( effect.affectsRigidbody)
                {
                    effect.enabled = value;
                }
            }
        }
    }

    public bool Outline
    {
        set
        {
            m_outline.enabled = value;
        }
    }

    protected virtual void OnEnable()
    {
        if(MTK_InteractiblesManager.Instance)
            MTK_InteractiblesManager.Instance.Subscribe(this);
    }

    // Use this for initialization
    void Awake ()
    {
        Assert.IsTrue(GetComponents<MTK_Interactable>().Length <= 1, gameObject.name);


        m_outline = GetComponent<Outline>();
        if(m_outline == null)
            m_outline = gameObject.AddComponent<Outline>();

        Outline = false;

        m_onUseStart = new UnityEvent();
        m_onUseStop = new UnityEvent();
        m_onGrabStart = new UnityEvent();
        m_onGrabSop = new UnityEvent();
        //m_wOnUseStart = new AK.Wwise.Event();
        //m_wOnUseStop = new AK.Wwise.Event();
        //m_wOnGrabStart = new AK.Wwise.Event();
        //m_wOnGrabStop = new AK.Wwise.Event();

        m_joints = GetComponents<MTK_JointType>();
        if (m_joints.Length == 0)
            m_joints = new MTK_JointType[]{ gameObject.AddComponent<MTK_JointType_Fixed>(), gameObject.AddComponent<MTK_InertJoint>() };
    }

    private void OnDisable()
    {
        if(MTK_InteractiblesManager.Instance)
            MTK_InteractiblesManager.Instance.UnSubscribe(this);
    }

    public virtual void Grab(bool input)
    {
        if (input)
        {
            m_onGrabStart.Invoke();

            if(m_wOnGrabStart !=null)
                m_wOnGrabStart.Post(gameObject);
        }
        else
        {
            m_onGrabSop.Invoke();

            if(m_wOnGrabStop !=null)
                m_wOnGrabStop.Post(gameObject);

            m_onUseStop.Invoke();

            if(m_wOnUseStop != null)
                m_wOnUseStop.Post(gameObject);

            UseEffects = true;
        }
    }

    public void Use(bool input)
    {
        if(input)
        {
            m_onUseStart.Invoke();

            if(m_wOnUseStart != null)
                m_wOnUseStart.Post(gameObject);
        }
        else
        {
            m_onUseStop.Invoke();

            if(m_wOnUseStop != null)
                m_wOnUseStop.Post(gameObject);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if(m_wOnCollision != null)
            m_wOnCollision.Post(gameObject);
    }
}
