using UnityEngine;

public class HazardSurface : MonoBehaviour
{
    private void Awake()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            Bounds b = mf.sharedMesh.bounds;
            if (b.size.y < 0.05f)
            {
                BoxCollider box = GetComponent<BoxCollider>();
                if (box == null)
                {
                    box = gameObject.AddComponent<BoxCollider>();
                }
                box.center = b.center;
                box.size = new Vector3(b.size.x, 0.2f, b.size.z);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.Die();
        }
    }
}
