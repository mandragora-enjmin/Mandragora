﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class Rotate : MonoBehaviour {

	public Vector3 rotation;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void FixedUpdate () {
		this.transform.Rotate(rotation);
	}
}
