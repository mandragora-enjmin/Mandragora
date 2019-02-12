﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SolarSystem : MonoBehaviour
{
    [SerializeField] private float m_maxSpeed = float.PositiveInfinity;
    [SerializeField] private float m_accelerationForce = 1;
    [SerializeField] private float impactForce = 3f;

    [SerializeField] public List<AK.Wwise.State> m_states = new List<AK.Wwise.State>();
    List<PlanetEffect> m_planetEffectsList = new List<PlanetEffect>();

    private Rigidbody m_rb;

    [SerializeField] GameObject m_explosionEffect;
    [SerializeField] MTK_TPZone m_planetTPZone;

    bool m_canTPPlanet = false;

    public IcoPlanet lastPlanet { get { return m_planetList.Count > 0 ? m_planetList[m_planetList.Count - 1] : null; } }

    List<IcoPlanet> m_planetList = new List<IcoPlanet>();



    private void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.attachedRigidbody)
        {            
            MTK_Interactable interactable = other.attachedRigidbody.GetComponent<MTK_Interactable>();
            if (interactable && interactable.isGrabbable && !interactable.isDistanceGrabbed)
            {
                UpdateTPZone(interactable.GetComponent<IcoPlanet>(), true);
                
                if( !other.attachedRigidbody.gameObject.GetComponent< PlanetEffect>())
                {
                    PlanetEffect eff = other.attachedRigidbody.gameObject.AddComponent<PlanetEffect>();
                    if (eff)
                    {
                        eff.maxSpeed = m_maxSpeed;
                        eff.accelerationForce = m_accelerationForce;
                        eff.impactForce = impactForce;
                        eff.sunRigidbody = m_rb;
                        m_planetEffectsList.Add(eff);

                        eff.explosionEffect = m_explosionEffect;

                        UpdateState(m_planetEffectsList.Count);
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.attachedRigidbody)
        {
            PlanetEffect effect = other.attachedRigidbody.GetComponent<PlanetEffect>();

            if (effect)
            {
                UpdateTPZone(other.GetComponent<IcoPlanet>(), false);
                m_planetEffectsList.Remove(effect);
                Destroy(effect);

                UpdateState(m_planetEffectsList.Count);
            }
        }
    }

    void UpdateTPZone(IcoPlanet planet, bool enter)
    {
        if(m_canTPPlanet)
        {
            if(planet)
            {
                foreach (IcoPlanet pl in m_planetList)
                {
                    ParticleSystem.EmissionModule emission = planet.GetComponentInChildren<ParticleSystem>().emission;
                    emission.rateOverTime = new ParticleSystem.MinMaxCurve(0);
                }

                if(enter)
                {
                    ParticleSystem.EmissionModule emission = planet.GetComponentInChildren<ParticleSystem>().emission;
                    emission.rateOverTime = new ParticleSystem.MinMaxCurve(20);

                    m_planetList.Add(planet);
                }
                else
                    m_planetList.Remove(planet);
            }

            m_planetTPZone.gameObject.SetActive(m_planetList.Count > 0);
        }
    }

    public void EnablePlanetTP()
    {
        m_canTPPlanet = true;
    }

    void UpdateState(int count)
    {
        if(count >= 0 && count < m_states.Count)
        {
            AkSoundEngine.SetState(m_states[count].GroupId, m_states[count].Id);
        }
    }
}