//#define CULLMODEON

using UnityEngine;
using System.Collections.Generic;

//Basado en el codigo de https://github.com/tutsplus/unity-2d-water-effect
//y en las fisicas de Michael Hoffman http://gamedevelopment.tutsplus.com/tutorials/make-a-splash-with-2d-water-effects--gamedev-236

[SelectionBase]
public class Water : MonoBehaviour
{
	public bool HabilitaOlas;

    public float Width;
    public float Depth;

    //The material we're using for the top of the water
    public Material mat;
    public bool AddNoiseToShader;

    //Our renderer that'll make the top of the water visible
    //LineRenderer LineRender_;

    //Our physics arrays
    float[] xpositions;
    float[] ypositions;
    float[] velocities;

    //Our meshes and colliders
    GameObject[] colliders;
    Mesh meshBase;

    //All our constants
    const float springconstant = 0.02f;
    const float damping = 0.04f;
    const float spread = 0.05f;
    float z;

    //The properties of our water
    float TopPos;
    float leftPos;
    //Time ForWave
    float WaveTimer;

    //optimize
#if CULLMODEON
    Bounds renderbounds;//Para el frustum requiere ser Bounds
#endif

    Vector3[] verticesPos;

    void Start()
    {
        leftPos = transform.position.x - Width / 2f;
        TopPos = transform.position.y;
        float BottomPos_ = transform.position.y - Depth;
        SpawnWater(leftPos, Width, TopPos, BottomPos_);

        WaveTimer = Random.Range(2f, 4f);
        z = transform.position.z;
#if CULLMODEON
        renderbounds = new Bounds(new Vector3(0f, -Depth / 2f), new Vector3(Width, TopPos - BottomPos_));
        if (cam == null)
            cam = Camera.main;
#endif
    }


    public void Splash(float xpos, float velocity)
    {
		if (!HabilitaOlas)
			return;
        //If the position is within the bounds of the water:
        if (xpos >= xpositions[0] && xpos <= xpositions[xpositions.Length - 1])
        {
            //Offset the x position to be the distance from the left side
            xpos -= xpositions[0];

            //Find which spring we're touching
            int index = Mathf.RoundToInt((xpositions.Length - 1) * (xpos / (xpositions[xpositions.Length - 1] - xpositions[0])));

            //Add the velocity of the falling object to the spring
            velocities[index] += velocity;
        }
    }

    public void SpawnWater(float _Left, float _Width, float _Top, float _Bottom)
    {
        //Trigger collider, for float effect
        /*gameObject.AddComponent<BoxCollider2D>();
        gameObject.GetComponent<BoxCollider2D>().center = new Vector2(0f, -Depth/2f);
        gameObject.GetComponent<BoxCollider2D>().size = new Vector2(_Width, _Top - _Bottom);
        gameObject.GetComponent<BoxCollider2D>().isTrigger = true;*/

        //Calculating the number of edges and nodes we have
        int edgecount = Mathf.RoundToInt(_Width) * 5;
        int nodecount = edgecount + 1;

        //Add our line renderer and set it up:
        /*LineRender_ = gameObject.AddComponent<LineRenderer>();
        LineRender_.material = mat;
        LineRender_.material.renderQueue = 1000;
        LineRender_.SetVertexCount(nodecount);
        LineRender_.SetWidth(0.1f, 0.1f);*/

        //Declare our physics arrays
        xpositions = new float[nodecount];
        ypositions = new float[nodecount];
        velocities = new float[nodecount];

        //Declare our mesh arrays
        Mesh[] meshes; meshes = new Mesh[edgecount];
        colliders = new GameObject[edgecount];

        //For each node, set the line renderer and our physics arrays
        for (int i = 0; i < nodecount; i++)
        {
            ypositions[i] = _Top;
            xpositions[i] = _Left + _Width * i / edgecount;
            //LineRender_.SetPosition(i, new Vector3(xpositions[i], _Top, z));
            velocities[i] = 0f;
        }

        //Cache-----------------------------------------------------
        //Set the UVs of the texture
        float U = 0f;
        float factorU = 1.0f / edgecount;
        Vector2[] UVs = new Vector2[4];

        //Set where the triangles should be.
        int[] tris = new int[6] { 0, 1, 3, 3, 2, 0 };

        //Setting the meshes now:
        for (int i = 0; i < edgecount; i++)
        {
            //Make the mesh
            meshes[i] = new Mesh();

            //Create the corners of the mesh
            Vector3[] Vertices = new Vector3[4];
            Vertices[0] = new Vector3(xpositions[i], ypositions[i], z);
            Vertices[1] = new Vector3(xpositions[i + 1], ypositions[i + 1], z);
            Vertices[2] = new Vector3(xpositions[i], _Bottom, z);
            Vertices[3] = new Vector3(xpositions[i + 1], _Bottom, z);

            //Add all this data to the mesh.
            meshes[i].vertices = Vertices;
            UVs[0] = new Vector2(U, 1);
            UVs[1] = new Vector2(U + factorU, 1);
            UVs[2] = new Vector2(U, 0);
            UVs[3] = new Vector2(U + factorU, 0);
            meshes[i].uv = UVs;
            U += factorU;
            meshes[i].triangles = tris;

            //Create our colliders, set them be our child
            colliders[i] = new GameObject();
            colliders[i].name = "Trigger";
            colliders[i].AddComponent<BoxCollider2D>();
            colliders[i].transform.parent = transform;

            //Set the position and scale to the correct dimensions
            colliders[i].transform.position = new Vector3(_Left + _Width * (i + 0.5f) / edgecount, _Top - 0.5f, 0);
            colliders[i].transform.localScale = new Vector3(_Width / edgecount, 1, 1);

            //Add a WaterDetector and make sure they're triggers
            colliders[i].GetComponent<BoxCollider2D>().isTrigger = true;
            colliders[i].AddComponent<WaterDetector>();
        }

        CombineInstance[] combine = new CombineInstance[meshes.Length];
        int k = 0;
        Matrix4x4 matrixLocalPos = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
        while (k < meshes.Length)
        {
            combine[k].mesh = meshes[k];
            combine[k].transform = matrixLocalPos;//meshobjects[k].transform.localToWorldMatrix;
            //meshobjects[k].gameObject.SetActive(false);
            k++;
        }

        meshBase = new Mesh();
        GameObject goCombineMesh = new GameObject("CombinedMesh");
        //goCombineMesh.transform.position = transform.position; //Set in wrong position.
        goCombineMesh.AddComponent<MeshFilter>();
        goCombineMesh.AddComponent<MeshRenderer>();
        meshBase.CombineMeshes(combine); //Combine all Meshes
        MeshWeld(meshBase, 0.1f); //Weld closes vertices
        verticesPos = meshBase.vertices; //Get Vertices
        goCombineMesh.GetComponent<MeshFilter>().mesh = meshBase; //Set Mesh
        goCombineMesh.GetComponent<Renderer>().material = mat;
        goCombineMesh.transform.parent = transform;
        if(AddNoiseToShader)
            goCombineMesh.AddComponent<shader_water2D>();
    }

    //Same as the code from in the meshes before, set the new mesh positions
    void UpdateMeshes()
    {
		if (!HabilitaOlas)
			return;
        for (int i = 0; i < ypositions.Length; i++) //meshes
        {
            verticesPos[YIndexToVerticesIndex(i)].y = ypositions[i];
        }
        meshBase.vertices = verticesPos;
    }

    static Camera cam;

    void LateUpdate()
    {
		if (HabilitaOlas)
		{
			//Creamos olas de manera aleatoria para que no este congelado el agua
			WaveTimer -= Time.deltaTime;
			if (WaveTimer < 0f)
			{
				//WaveTimer = Random.Range(0.5f, 1.5f);
				Splash(Random.Range(leftPos, leftPos + Width), -Random.Range(0.1f, 0.5f));
				WaveTimer = Random.Range(0.3f, 1f);
				Splash(leftPos, -Random.Range(0.05f, 0.5f));
			}
		}
        

#if CULLMODEON
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(sanchoCam);
        gameObject.SetActive(GeometryUtility.TestPlanesAABB(planes, renderbounds));
#endif
    }

    //Called regularly by Unity
    void FixedUpdate()
    {
        //Here we use the Euler method to handle all the physics of our springs:
        for (int i = 0; i < xpositions.Length; i++)
        {
            float force = springconstant * (ypositions[i] - TopPos) + velocities[i] * damping;//formula de resortes
            ypositions[i] += velocities[i];
            velocities[i] += -force;//accelerations[i];
            //LineRender_.SetPosition(i, new Vector3(xpositions[i], ypositions[i], z));
        }

        //Now we store the difference in heights:
        float[] leftDeltas = new float[xpositions.Length];
        float[] rightDeltas = new float[xpositions.Length];

        //We make 8 small passes for fluidity:
        for (int j = 0; j < 8; j++)
        {
            for (int i = 0; i < xpositions.Length; i++)
            {
                //We check the heights of the nearby nodes, adjust velocities accordingly, record the height differences
                if (i > 0)
                {
                    leftDeltas[i] = spread * (ypositions[i] - ypositions[i - 1]);
                    velocities[i - 1] += leftDeltas[i];
                }
                if (i < xpositions.Length - 1)
                {
                    rightDeltas[i] = spread * (ypositions[i] - ypositions[i + 1]);
                    velocities[i + 1] += rightDeltas[i];
                }
            }

            //Now we apply a difference in position
            for (int i = 0; i < xpositions.Length; i++)
            {
                if (i > 0)
                    ypositions[i - 1] += leftDeltas[i];
                if (i < xpositions.Length - 1)
                    ypositions[i + 1] += rightDeltas[i];
            }
        }
        //Finally we update the meshes to reflect this
        UpdateMeshes();
    }

    /*void OnTriggerStay2D(Collider2D Hit)
    {
    }*/


    //Convierte el Index de ypositions a Index del vertice
    int YIndexToVerticesIndex(int _index)
    {
        if (_index > 1)
            return _index * 2;
        return _index;
    }

    void MeshWeld(Mesh _mesh, float _threshold)
    {
        Vector3[] verts = _mesh.vertices;

        // Build new vertex buffer and remove "duplicate" verticies
        // that are within the given threshold.
        List<Vector3> newVerts = new List<Vector3>();
        List<Vector2> newUVs = new List<Vector2>();

        for (int i = 0; i < verts.Length; i++)
        {
            bool skipToNext = false;
            // Has vertex already been added to newVerts list?
            for (int k = 0; k < newVerts.Count; k++)
            {
                if (Vector3.Distance(newVerts[k], verts[i]) <= _threshold)
                {
                    skipToNext = true;
                    break;
                }
            }
            if (skipToNext == false)
            {
                newVerts.Add(verts[i]);
                newUVs.Add(_mesh.uv[i]);
            }
        }

        // Rebuild triangles using new verticies
        int[] tris = _mesh.triangles;
        for (int i = 0; i < tris.Length; ++i)
        {
            // Find new vertex point from buffer
            for (int j = 0; j < newVerts.Count; ++j)
            {
                if (Vector3.Distance(newVerts[j], verts[tris[i]]) <= _threshold)
                {
                    tris[i] = j;
                    break;
                }
            }
        }

        // Update mesh!
        _mesh.Clear();
        _mesh.vertices = newVerts.ToArray();
        _mesh.triangles = tris;
        _mesh.uv = newUVs.ToArray();
        _mesh.MarkDynamic();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        float left_ = transform.position.x - Width / 2f;
        float right_ = transform.position.x + Width / 2f;
        float bottom_ = transform.position.y - Depth;
        Vector3 TopLeft = new Vector3(left_, transform.position.y, transform.position.z);
        Vector3 TopRight = new Vector3(right_, transform.position.y, transform.position.z);
        Vector3 BotLeft = new Vector3(left_, bottom_, transform.position.z);
        Vector3 BotRight = new Vector3(right_, bottom_, transform.position.z);

        Gizmos.DrawLine(TopLeft, TopRight);
        Gizmos.DrawLine(TopRight, BotRight);
        Gizmos.DrawLine(BotRight, BotLeft);
        Gizmos.DrawLine(BotLeft, TopLeft);
    }
}
