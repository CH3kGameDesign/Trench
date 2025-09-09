using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Asteroid : MonoBehaviour
{
    private Rigidbody rb;

    public int I_maxHealth = 10;
    private int i_health;

    public List<Asteroid> G_splitAsteroids = new List<Asteroid>();
    public Vector2Int V2_splitBounds = new Vector2Int(2, 4);
    public float F_splitLinForce = 100;
    public float F_splitRotForce = 50;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!rb)
        {
            rb = GetComponent<Rigidbody>();
            i_health = I_maxHealth;
        }
    }

    public void OnCreate(Vector3 _linForce, Vector3 _rotForce)
    {
        rb = GetComponent<Rigidbody>();
        i_health = I_maxHealth;

        rb.AddForce(_linForce, ForceMode.Impulse);
        rb.AddTorque(_rotForce, ForceMode.Impulse);
    }

    public void OnHit(GunManager.bulletClass _bullet, DamageSource _source)
    {
        i_health -= Mathf.RoundToInt(_bullet.F_damage);
        if (i_health <= 0)
        {
            Split();
            Destroy(gameObject);
        }
    }
    void Split()
    {
        if (G_splitAsteroids.Count == 0) return;
        int _amt = Random.Range(V2_splitBounds.x, V2_splitBounds.y);
        for (int i = 0; i < _amt; i++)
        {
            int _ran = Random.Range(0, G_splitAsteroids.Count);
            Vector3 _dir = Random.insideUnitSphere;
            Quaternion _rot = Random.rotation;
            Asteroid _A = Instantiate(G_splitAsteroids[_ran], transform.position + _dir, _rot);
            _A.transform.parent = transform.parent;
            _A.OnCreate(_dir * F_splitLinForce, _rot.eulerAngles * F_splitRotForce);
        }
    }
}
