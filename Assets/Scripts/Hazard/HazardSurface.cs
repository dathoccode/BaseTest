using UnityEngine;

public class HazardSurface : MonoBehaviour
{
    private void Awake()
    {
        // Đặc thù 3D: Hai tấm sàn bẫy Cube_8.053 và Cube_8.055 trong 2.fbx là mặt phẳng 2D (độ dày Y = 0)
        // bị ngược вектор pháp tuyến (Normal = (0, -1, 0) úp xuống đất).
        // MeshCollider 1 mặt sẽ cho nhân vật rơi xuyên qua mặt lưng của nó xuống sàn phòng Cube_006 (Y = 0.85) bên dưới.
        // Do đó ta tự động bọc 1 BoxCollider có độ dày 0.2m để bắt va chạm từ phía trên chuẩn 100%.
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
