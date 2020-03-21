using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    [SerializeField] Rigidbody body;
    public int Quadrant { get; private set; }
    public static float speed;
    public void Init(int quadrant)
    {
        Quadrant = quadrant;
        if (Quadrant == 0)
        {
            transform.position = new Vector3(0,4,0);
            transform.rotation = Quaternion.Euler(0,180,180);
            body.velocity = new Vector3(0,-speed,0);
            body.AddTorque(new Vector3(0,Random.Range(0,100),0));
        }
        else if (Quadrant == 1)
        {
            transform.position = new Vector3(-4,0,0);
            transform.rotation = Quaternion.Euler(0,180,90);
            body.velocity = new Vector3(speed,0,0);
            body.AddTorque(new Vector3(Random.Range(0,100),0,0));
        }
        else if (Quadrant == 2)
        {
            transform.position = new Vector3(0,-4,0);
            transform.rotation = Quaternion.Euler(0,180,0);
            body.velocity = new Vector3(0,speed,0);
            body.AddTorque(new Vector3(0,Random.Range(0,100),0));
        }
        else if (Quadrant == 3)
        {
            transform.position = new Vector3(4,0,0);
            transform.rotation = Quaternion.Euler(0,180,270);
            body.velocity = new Vector3(-speed,0,0);
            body.AddTorque(new Vector3(Random.Range(0,100),0,0));
        }
    }
    [SerializeField] MeshRenderer meshRenderer;
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Warning")
        {
            if (tag == "Hand") {
                meshRenderer.materials[0].color = Color.red;
            } else {
                meshRenderer.materials[0].color = Color.blue;
            }
        }
    }
    [SerializeField] Effect smackPrefab, touchPrefab;
    public void Smack()
    {
        if (Quadrant == 0)
        {
            body.AddForce(new Vector3(5*speed,10*speed,0));
        }
        else if (Quadrant == 1)
        {
            body.AddForce(new Vector3(-10*speed,5*speed,0));
        }
        else if (Quadrant == 2)
        {
            body.AddForce(new Vector3(-5*speed,-10*speed,0));
        }
        else if (Quadrant == 3)
        {
            body.AddForce(new Vector3(10*speed,-5*speed,0));
        }
        body.AddTorque(1000 * Random.insideUnitSphere);

        var effect = Instantiate(smackPrefab);
        effect.transform.position = transform.position + 2*Vector3.back;
        GetComponent<Collider>().enabled = false;

        IEnumerator WaitThenDestroy()
        {
            yield return new WaitForSeconds(1);
            Destroy(gameObject);
        }
        StartCoroutine(WaitThenDestroy());
    }
    public void Touch()
    {
        var effect = Instantiate(touchPrefab);
        effect.transform.position = transform.position + 2*Vector3.back;
        Destroy(gameObject);
    }
}