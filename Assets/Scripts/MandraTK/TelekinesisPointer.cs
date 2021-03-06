using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelekinesisPointer : MonoBehaviour
{
	FXManager m_fxManager;
	[SerializeField] MTK_InteractHand m_interactHand;
	[SerializeField] Animator m_handAnimator;
	[SerializeField] Transform m_repere;

	[Header("Settings")]
	[SerializeField, Range(0,0.1f)]
	float m_minMagnitudeToAttract = .2f;
	[SerializeField, Range(0,1)]
	float m_maxForce = 1;
	[SerializeField, Range(0,10)]
	float m_forceScale = 4;
	[SerializeField, Range(0,10)]
	float m_distanceSpeedScale = 4;

	[Header("Sound")]
	[SerializeField] AK.Wwise.Event m_wObjectGrabbed;
	[SerializeField] AK.Wwise.Event m_wObjectPlay;
	[SerializeField] AK.Wwise.Event m_wObjectStop; 
	[SerializeField] AK.Wwise.Event m_wHandPlay;
	[SerializeField] AK.Wwise.Event m_wHandStop;

	MTK_InputManager m_inputManager;
	MTK_InteractHand m_hand;
	MTK_InteractiblesManager m_interactiblesManager;

	MTK_Interactable m_currentInteractable;

    public bool isAttracting { get { return m_connectedBody || Target; } }
    Rigidbody m_connectedBody;
	[HideInInspector] public bool Active = true;
    
    bool m_attract;
	float m_initDistanceToTarget;
	Vector3 m_lastForceApplied;
	Vector3 m_lastPos;

	MTK_Interactable Target
	{
		get
		{
			return m_currentInteractable;
		}
		set
		{
			if(value == null)
			{
				if(m_currentInteractable)
				{
					m_wObjectStop.Post(m_currentInteractable.gameObject);
					m_currentInteractable.Outline = false;
					m_currentInteractable = null;
				}
			}
			else
			{
				if(m_currentInteractable != value)
				{
					if(m_currentInteractable)
					{
						m_currentInteractable.Outline = false;
						m_wObjectStop.Post(m_currentInteractable.gameObject);
					}

					m_currentInteractable = value;
					m_currentInteractable.Outline = true;

					m_inputManager.Haptic(1);
				}
			}			
		}
	}

	void Awake()
	{
		m_inputManager = GetComponentInParent<MTK_InputManager>();
		m_inputManager.m_onTrigger.AddListener(TriggerAttract);

		m_lastPos = transform.position;

		m_hand = GetComponentInParent<MTK_InteractHand>();
		m_hand.m_onTouchInteractable.AddListener(GrabIfTarget);

		m_interactiblesManager = MTK_InteractiblesManager.Instance;

		m_fxManager = GetComponentInChildren<FXManager>();
	}

	void Update()
	{
		if(Active)
		{
			if(!m_attract)
			{
				if(m_interactHand.ObjectInTrigger)
				{
					Target = null;
				}
				else if(!m_connectedBody)
				{
					Target = m_interactiblesManager.GetClosestToView(transform.position, m_repere.position - transform.position, 15);
				}
			}
			
			if(Target)
			{
				float distanceScale = Mathf.Min(GetDistanceToTarget() / m_initDistanceToTarget, 1); // 1 -> 0

				// Detect movement to trigger attraction
				if(m_attract)
				{
					Vector3 force = (m_repere.position - m_lastPos) * 10;

					if(force.sqrMagnitude > m_maxForce)
						force = force.normalized * m_maxForce;
					
					if(force.sqrMagnitude > Mathf.Max(m_minMagnitudeToAttract, m_lastForceApplied.sqrMagnitude))
						Attract(force);

					// Levitation
					if(!m_connectedBody && IsLevitating(Target))
					{
						Rigidbody rgbd = Target.GetComponent<Rigidbody>();
						rgbd.velocity = Vector3.ClampMagnitude(rgbd.velocity, .1f);
					}
				}

				// Apply various forces
				if(m_connectedBody)
				{
					Vector3 targetVel = (transform.position - Target.transform.position).normalized * m_forceScale;
					targetVel += distanceScale * m_lastForceApplied * GetDistanceToTarget();

					m_connectedBody.rotation = Quaternion.RotateTowards(m_connectedBody.rotation, transform.rotation * Quaternion.Euler(Target.m_upwardRotation), Time.deltaTime * (1 - distanceScale) * 500);
					m_connectedBody.velocity = Vector3.MoveTowards(m_connectedBody.velocity, targetVel * (Mathf.Sqrt(GetDistanceToTarget()) * m_distanceSpeedScale), Time.deltaTime * (20 + (1-distanceScale) * 10));

					m_inputManager.Haptic((1- distanceScale) / 10);

					AkSoundEngine.SetRTPCValue("Grab_Distance", GetDistanceToTarget());
				}
			}

			// Update force
			m_lastPos = m_repere.position;
		}
	}

    void SetLevitation(MTK_Interactable interactable, bool value)
    {
        Rigidbody rb = interactable.GetComponent<Rigidbody>();
        if(rb)
        {
            if (value)
            {
				interactable.GetComponent<Rigidbody>().isKinematic = false;
				interactable.GetComponent<Rigidbody>().drag = 0;

                interactable.UseEffects = false;
                rb.velocity = Vector3.up;
                rb.angularVelocity = Random.onUnitSphere;

				m_lastForceApplied = Vector3.zero;
            }
            rb.useGravity = !value; // !gravity = levitate
        }         
    }

    bool IsLevitating(MTK_Interactable interactable)
    {
        Rigidbody rb = interactable.GetComponent<Rigidbody>();
        if (rb)
        {
            return !rb.useGravity;
        }
        return false;
    }

    void TriggerAttract(bool input)
	{
		//m_handAnimator.SetBool("Attract", input);

		if(input)
		{
			if(Target)
			{
				m_wHandPlay.Post(gameObject);

				m_attract = true;
				Target.UseEffects = false;
                Target.isDistanceGrabbed = true;

                SetLevitation(Target, true);
				m_fxManager.Activate("Grab", Target.transform);
				m_fxManager.Activate("Grab_In", Target.transform);
			}
		}
		else
		{
			m_wHandStop.Post(gameObject);

			m_attract = false;
			m_fxManager.DeActivate("Grab");

			if(Target)
			{
				if(IsLevitating(Target))
					m_fxManager.Activate("Grab_Out", Target.transform);

                SetLevitation(Target, false);

				Target.UseEffects = true;
                Target.isDistanceGrabbed = false;
            }

			if(m_connectedBody)
				UnAttract();
		}
	}

	void Attract(Vector3 force)
	{
		//m_handAnimator.SetBool("Attract", false);
		m_handAnimator.SetBool("Attract", true);

        SetLevitation(Target, false);
        m_fxManager.DeActivate("Grab");

		if(!m_connectedBody) // = first call
		{
			m_wObjectPlay.Post(Target.gameObject);
			m_initDistanceToTarget = GetDistanceToTarget();
		}

		m_connectedBody = Target.GetComponent<Rigidbody>();

		if(m_connectedBody)
		{
			m_connectedBody.AddForce((m_lastForceApplied - force) * Mathf.Sqrt(GetDistanceToTarget() * 1000));
			m_connectedBody.AddTorque(force.z, force.x, force.y);
			m_connectedBody.useGravity = false;
			
			m_lastForceApplied = force;
		}
	}

	void UnAttract()
	{
		//m_handAnimator.SetBool("Attract", false);
		m_handAnimator.SetBool("Attract", true);

		m_wObjectStop.Post(Target.gameObject);

		if(m_connectedBody)
		{
			m_connectedBody.useGravity = true;
			m_connectedBody = null;
		}

        Target.isDistanceGrabbed = false;
        Target = null;
	}

	void GrabIfTarget(MTK_Interactable input)
	{
		if(m_attract && Target && input)
		{
			if(Target.gameObject == input.gameObject)
			{
				m_inputManager.Haptic(1);

				m_wObjectGrabbed.Post(Target.gameObject);
				m_wObjectStop.Post(Target.gameObject);
				m_wHandStop.Post(gameObject);

				input.transform.position = transform.position;
				m_hand.Grab(Target);

				SetLevitation(input, false);
				UnAttract();
			}
		}
	}

	float GetDistanceToTarget()
	{
		return Vector3.Distance(transform.position, Target.transform.position);
	}
}