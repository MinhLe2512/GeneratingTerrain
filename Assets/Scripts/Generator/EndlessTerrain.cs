using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public const float viewDst = 450f;
    public Transform viewer;

    public Material mapMaterial;

    public static Vector2 viewerPosition;
    private int chunkSize;
    private int numberOfVisibleChunks;

    Dictionary<Vector2, TerrainChunk> terrainDict;
    List<TerrainChunk> terrainChunkVisibleLastUpdate;

    static WorldGenerator worldGenerator;

    // Start is called before the first frame update
    void Awake()
    {
        worldGenerator = FindObjectOfType<WorldGenerator>();

        chunkSize = WorldGenerator.maxChunkSize - 1;
        numberOfVisibleChunks = Mathf.RoundToInt(viewDst / chunkSize);

        terrainChunkVisibleLastUpdate = new List<TerrainChunk>();

        terrainDict = new Dictionary<Vector2, TerrainChunk>();
    }

    //Day 6: threading
    //Step 1: Request map data
    //Step 2: OnMapDataRecieved
    void OnMapDataReceived() { 
    
    }

    public void UpdateVisibleChunks() {
        for (int i =0; i < terrainChunkVisibleLastUpdate.Count; i++) {
            terrainChunkVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunkVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int offSetY = -numberOfVisibleChunks; offSetY <= numberOfVisibleChunks; offSetY++) { 
            for (int offSetX = -numberOfVisibleChunks; offSetX <= numberOfVisibleChunks; offSetX++) {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + offSetX, currentChunkCoordY + offSetY);

                if (terrainDict.ContainsKey(viewedChunkCoord)) {
                    terrainDict[viewedChunkCoord].UpdateTerrainChunk();
                    terrainChunkVisibleLastUpdate.Add(terrainDict[viewedChunkCoord]);
                }
                else {
                    terrainDict.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, mapMaterial));
                }
            }
        }
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }

    public class TerrainChunk {
        private GameObject meshObject;
        private Vector2 position;
        private Bounds bounds;

        MapData mapData;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        string chunkName;
        public TerrainChunk(Vector2 coord, int size, Transform parent, Material material) {
            position = coord * size;

            chunkName = "Chunk(" + position.x + ", " + position.y + ")";

            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject(chunkName);
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;

            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;
            SetVisible(false);

            worldGenerator.RequestMapData(OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData) {
            worldGenerator.RequestMeshData(mapData, OnMeshDataReceived);
        }

        void OnMeshDataReceived(MeshData meshData) {
            meshFilter.mesh = meshData.CreateMesh();
        }

        public void UpdateTerrainChunk() {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDstFromNearestEdge <= viewDst;
            SetVisible(visible);
        }

        public void SetVisible(bool visible) {
            meshObject.SetActive(visible);
        }

        public bool IsVisble() {
            return meshObject.activeSelf;
        }
    }
}
