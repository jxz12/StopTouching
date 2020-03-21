using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] Rigidbody body;
    public int Quadrant { get; private set; }
    public static float speed;
    public void Init(int quadrant)
    {
        this.Quadrant = quadrant;
        if (quadrant == 0)
        {
            transform.position = new Vector3(0,5,0);
            transform.rotation = Quaternion.Euler(0,0,180);
            body.AddForce(new Vector3(0,-speed,0));
            body.AddTorque(new Vector3(0,Random.Range(10,100),0));
        }
        else if (quadrant == 1)
        {
            transform.position = new Vector3(-5,0,0);
            transform.rotation = Quaternion.Euler(0,0,270);
            body.AddForce(new Vector3(speed,0,0));
            body.AddTorque(new Vector3(Random.Range(10,100),0,0));
        }
        else if (quadrant == 2)
        {
            transform.position = new Vector3(0,-5,0);
            transform.rotation = Quaternion.Euler(0,0,0);
            body.AddForce(new Vector3(0,speed,0));
            body.AddTorque(new Vector3(0,Random.Range(10,100),0));
        }
        else if (quadrant == 3)
        {
            transform.position = new Vector3(5,0,0);
            transform.rotation = Quaternion.Euler(0,0,90);
            body.AddForce(new Vector3(-speed,0,0));
            body.AddTorque(new Vector3(Random.Range(10,100),0,0));
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
                meshRenderer.materials[0].color = Color.green;
            }
        }
    }
}