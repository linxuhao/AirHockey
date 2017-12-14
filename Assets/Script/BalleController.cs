using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BalleController : MonoBehaviour {

    public bool pushTheBallOnStart;
    public float speed;
    public Vector3 direction;
    private Rigidbody rb;
    private Component halo;
    private bool initialized = false;
    public AudioClip chocSound;
    public AudioClip refresh;
    private AudioSource audioPlayer;

    // Use this for initialization
    void Start (){
        init();
    }

    private void init(){
        rb = GetComponent<Rigidbody>();
        rb.angularDrag = 0;
        rb.drag = 0;
        halo = GetComponent("Halo");
        audioPlayer = GetComponent<AudioSource>();
        playClip(refresh);
        setHaloActiveState(false);
        direction = new Vector3(0f, 0, 0f);

        initialized = true;
    }
    void Update(){
        if (!initialized) {
            init();
        }
    }

    private void playClip(AudioClip clip){
        if (audioPlayer.isActiveAndEnabled && !audioPlayer.isPlaying) {
            audioPlayer.clip = clip;
            audioPlayer.Play();
        }
    }

    private void setHaloActiveState(bool state){
        if (halo) {
            halo.GetType().GetProperty("enabled").SetValue(halo, state, null);
        }
    }

    void OnCollisionEnter(Collision collision){
        if (!collision.collider.CompareTag("tableFond")) { 
            float tempSpeed = 2 * speed;
            Collider collider = collision.collider;
            //hit wall is different from hitting objects
            if (collider.CompareTag("murSud") || collider.CompareTag("murNord")){
                choc(tempSpeed, direction.x, -direction.z);
            }
            else if (collider.CompareTag("murEst") || collider.CompareTag("murOuest")){
                choc(tempSpeed, -direction.x, direction.z);
            }
            else{
                //the center of contact points
                Vector3 contactPoint = new Vector3();
                float x = 0;
                float y = 0;
                float z = 0;
                for (int i = 0; i < collision.contacts.Length; i++){
                    ContactPoint contact = collision.contacts[i];
                    x += contact.point.x;
                    y += contact.point.y;
                    z += contact.point.z;
                }
                x /= collision.contacts.Length;
                y /= collision.contacts.Length;
                z /= collision.contacts.Length;
                contactPoint.x = x;
                contactPoint.y = y;
                contactPoint.z = z;

                Vector3 contactVector = transform.position - contactPoint;
                choc(tempSpeed, contactVector.x, contactVector.z);
            }
        }
    }

    //x and y on 2D plan, is x and z on 3D plan
    public void choc(float speed, float xDirection, float yDirection) {
        playClip(chocSound);
        StartCoroutine(shineForSeconds(0.1f));
        direction = new Vector3(xDirection, 0, yDirection);
        Vector3 speedVector = new Vector3(direction.normalized.x * speed, 0, direction.normalized.z * speed);
        rb.velocity = speedVector;
    }

    public IEnumerator shineForSeconds(float seconds) {
        setHaloActiveState(true);
        yield return new WaitForSeconds(seconds);
        setHaloActiveState(false);
    }
}
