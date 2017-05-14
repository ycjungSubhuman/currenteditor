using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet1Script : MonoBehaviour {

    float startTime;
	// Use this for initialization
	void Start () {
        startTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
        gameObject.GetComponent<Transform>().Translate(new Vector3(0f, 0.5f, 0f));
        if (Time.time - startTime > 2)
            Destroy(gameObject);
	}
}
