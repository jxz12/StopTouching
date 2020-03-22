using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    [SerializeField] Rigidbody body;
    public int Quadrant { get; private set; }
    public void InitSides(int quadrant, float speed, Camera cam)
    {
        Quadrant = quadrant;
        float z = -cam.transform.position.z;
        if (Quadrant == 0)
        {
            transform.position = cam.ViewportToWorldPoint(new Vector3(.5f,1.1f,z));
        }
        else if (Quadrant == 1)
        {
            transform.position = cam.ViewportToWorldPoint(new Vector3(-.1f,.5f,z));
        }
        else if (Quadrant == 2)
        {
            transform.position = cam.ViewportToWorldPoint(new Vector3(.5f,-.1f,z));
        }
        else if (Quadrant == 3)
        {
            transform.position = cam.ViewportToWorldPoint(new Vector3(1.1f,.5f,z));
        }
        transform.rotation = Quaternion.Euler(0,0,Vector3.SignedAngle(transform.position, Vector3.down, Vector3.back));
        body.velocity = -transform.position.normalized * speed;
        body.AddTorque(Random.Range(0,100) * -transform.position.normalized);
    }
    public void InitCorners(int quadrant, float speed, Camera cam)
    {
        Quadrant = quadrant;
        float z = -cam.transform.position.z;
        if (Quadrant == 0)
        {
            transform.position = cam.ViewportToWorldPoint(new Vector3(1.1f,1.1f,z));
        }
        else if (Quadrant == 1)
        {
            transform.position = cam.ViewportToWorldPoint(new Vector3(-.1f,1.1f,z));
        }
        else if (Quadrant == 2)
        {
            transform.position = cam.ViewportToWorldPoint(new Vector3(-.1f,-.1f,z));
        }
        else if (Quadrant == 3)
        {
            transform.position = cam.ViewportToWorldPoint(new Vector3(1.1f,-.1f,z));
        }
        transform.rotation = Quaternion.Euler(0,0,Vector3.SignedAngle(transform.position, Vector3.down, Vector3.back));
        body.velocity = -transform.position.normalized * speed;
        body.AddTorque(Random.Range(0,100) * -transform.position.normalized);
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
            body.velocity = new Vector3(5,10,0);
        }
        else if (Quadrant == 1)
        {
            body.velocity = new Vector3(-10,5,0);
        }
        else if (Quadrant == 2)
        {
            body.velocity = new Vector3(-5,-10,0);
        }
        else if (Quadrant == 3)
        {
            body.velocity = new Vector3(10,-5,0);
        }

        body.AddTorque(10000 * Random.insideUnitSphere.normalized);

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