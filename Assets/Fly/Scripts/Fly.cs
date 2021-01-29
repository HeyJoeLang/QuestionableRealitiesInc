using UnityEngine;
using System.Collections;

public class Fly : MonoBehaviour {
    Animator fly;
    private IEnumerator coroutine;
	// Use this for initialization
	void Start () {
        fly = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.S))
        {
            fly.SetBool("idle", true);
            fly.SetBool("walk", false);
            fly.SetBool("rubbing", false);
            fly.SetBool("turnright", false);
            fly.SetBool("turnleft", false);
        }
        if (Input.GetKey(KeyCode.W))
        {
            fly.SetBool("walk", true);
            fly.SetBool("idle", false);
        }
        if ((Input.GetKey(KeyCode.R))||(Input.GetKey(KeyCode.Space)))
        {
            fly.SetBool("takeoff", true);
            fly.SetBool("walk", false);
            fly.SetBool("idle", false);
            StartCoroutine("flying");
            flying();
        }
        if ((Input.GetKey(KeyCode.F))||(Input.GetKey(KeyCode.S)))
        {
            fly.SetBool("landing", true);
            fly.SetBool("flying", false);
            fly.SetBool("flyleft", false);
            fly.SetBool("flyright", false);
            StartCoroutine("idle");
            idle();
        }
        if (Input.GetKey(KeyCode.E))
        {
            fly.SetBool("rubbing", true);
            fly.SetBool("idle", false);
        }
        if (Input.GetKey(KeyCode.W))
        {
            fly.SetBool("flying", true);
            fly.SetBool("flyleft", false);
            fly.SetBool("flyright", false);
        }
        if (Input.GetKey(KeyCode.A))
        {
            fly.SetBool("flyleft", true);
            fly.SetBool("flying", false);
            fly.SetBool("flyright", false);
        }
        if (Input.GetKey(KeyCode.D))
        {
            fly.SetBool("flyright", true);
            fly.SetBool("flying", false);
            fly.SetBool("flyleft", false);
        }
        if (Input.GetKey(KeyCode.A))
        {
            fly.SetBool("turnleft", true);
            fly.SetBool("turnright", false);
            fly.SetBool("idle", false);
        }
        if (Input.GetKey(KeyCode.D))
        {
            fly.SetBool("turnright", true);
            fly.SetBool("turnleft", false);
            fly.SetBool("idle", false);
        }
        if (Input.GetKey(KeyCode.Keypad0))
        {
            fly.SetBool("die", true);
            fly.SetBool("flying", false);
        }
	}
    IEnumerator flying()
    {
        yield return new WaitForSeconds(0.1f);
        fly.SetBool("takeoff", false);
        fly.SetBool("flying", true);
    }
    IEnumerator idle()
    {
        yield return new WaitForSeconds(0.1f);
        fly.SetBool("idle",true);
        fly.SetBool("flying", false);
        fly.SetBool("landing", false);
    }
}
