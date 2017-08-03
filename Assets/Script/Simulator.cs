using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulator : MonoBehaviour {
    public Vector3 direction;
    public int id;
    public int sort;
    bool moving;
    float spantime;
    float speed = 10;
    float dietime = 0.5f;
    // Use this for initialization
    void Start () {
		
	}

    public void startmove() {
        spantime = 0;
        moving = true;
    }


    private void FixedUpdate()
    {
        if (moving) {
            gameObject.transform.position += direction * speed;
            spantime += Time.deltaTime;
        }
        if (spantime > dietime) {
            GameObject.Destroy(gameObject, 0.1f);
            moving = false;
        }
    }
    // Update is called once per frame
    void Update () {
		
	}
}
