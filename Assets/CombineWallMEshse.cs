using UnityEngine;

public class CombineWallMeshes : MonoBehaviour
{
    [Header("设置材质，如果留空则使用第一个Cube的材质")]
    public Material wallMaterial;

    [Header("是否隐藏原Cube")]
    public bool hideOriginalCubes = true;

    [ContextMenu("Combine Walls")] // 在 Inspector 里右键可直接执行
    public void Combine()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        if (meshFilters.Length == 0)
        {
            Debug.LogWarning("没有找到子 MeshFilter");
            return;
        }

        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;

            if (hideOriginalCubes)
                meshFilters[i].gameObject.SetActive(false);
        }

        // 添加 MeshFilter 和 MeshRenderer
        MeshFilter mf = gameObject.GetComponent<MeshFilter>();
        if (mf == null)
            mf = gameObject.AddComponent<MeshFilter>();

        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
        if (mr == null)
            mr = gameObject.AddComponent<MeshRenderer>();

        mf.mesh = new Mesh();
        mf.mesh.CombineMeshes(combine);

        // 材质处理
        if (wallMaterial != null)
            mr.material = wallMaterial;
        else
            mr.material = meshFilters[0].GetComponent<MeshRenderer>().sharedMaterial;

        // 添加 MeshCollider
        MeshCollider mc = gameObject.GetComponent<MeshCollider>();
        if (mc == null)
            mc = gameObject.AddComponent<MeshCollider>();

        mc.sharedMesh = mf.mesh;
        mc.convex = false;

        // 确保墙体可见
        gameObject.SetActive(true);

        Debug.Log("✅ 墙体合并完成！");
    }
}
