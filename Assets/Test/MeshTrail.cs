using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MeshTrail : MonoBehaviour
{

    class Drop
    {
        public List<int> triList;
        public List<Vector3> vertList;
        public Vector3[] meshVertList;
        public Vector3 headPos;
        public Vector3 rootPos;
        public List<Vector3> controlPoints;

        public int ringSlice;
        public int vSlice;

        public float lifeTime = 10.0f;

        private float currLife = 0.0f;

      

        public Drop(Vector3 emmitPos, int rSlice = 12, int vSlice = 12, float radius = 3.0f, float lifeTime = 10.0f)
        {
            headPos = rootPos = emmitPos;
            controlPoints = new List<Vector3>(vSlice);
            ringSlice = rSlice;
            this.vSlice = vSlice;
            this.lifeTime = lifeTime;
            //Gen Control Point
            for (int i = 0; i < vSlice; i++)
            {
                controlPoints.Add(rootPos);
            }
            
            //Gen Mesh Data
            vertList = new List<Vector3>((ringSlice + 2) * vSlice);
            triList = new List<int>(vertList.Capacity * 3);
            for (int i = 0; i < vSlice + 2; i++)
            {
                for (int j = 0; j < ringSlice; j++)
                {
                    float theta = j * 1.0f / ringSlice * Mathf.PI * 2.0f;
                    float x = radius * Mathf.Cos(theta);
                    float z = radius * Mathf.Sin(theta);
                    Vector3 pos = new Vector3(x, 0, z) + rootPos;
                    vertList.Add(pos);
                    if (i < vSlice+1)
                    {
                        if (j != ringSlice - 1)
                        {
                            triList.Add(i * ringSlice + j);
                            triList.Add((i + 1) * ringSlice + j);
                            triList.Add((i + 1) * ringSlice + j + 1);
                           
                            
                            triList.Add(i * ringSlice + j);
                            triList.Add((i + 1) * ringSlice + j + 1);
                            triList.Add(i * ringSlice + j + 1);
                         
                        }
                        else
                        {
                            triList.Add(i * ringSlice + j);
                            triList.Add((i + 1) * ringSlice + j);
                            triList.Add((i + 1) * ringSlice);
                          
                            triList.Add(i * ringSlice + j);
                            triList.Add((i + 1) * ringSlice);
                            triList.Add(i * ringSlice);
                        
                        }
                       
                    }
                  
                }
            }

            meshVertList = new Vector3[vertList.Count];
            for (int i = 0; i < meshVertList.Length; i++)
            {
                meshVertList[i] = vertList[i];
            }
        }

        public void MoveHeadPoint(Vector3 dir, float dt)
        {
            float trueDt = 0.0f;
            if (currLife >= lifeTime)
            {
                return;
            }

            trueDt = Mathf.Min(dt, lifeTime - currLife);
            headPos += dir * trueDt;
            currLife += trueDt;
            UpdateCtrlPoint(trueDt);
            UpdateMeshData();
        }
        
        private void UpdateCtrlPoint(float dt)
        {
            int endIdx = controlPoints.Count - 1;
            Vector3 targetP;
            Vector3 currP;
            for (int i = endIdx; i >= 0; i--)
            {
                currP = controlPoints[i];
                if (i == endIdx)
                {
                    targetP = headPos;
                }
                else
                {
                    targetP = controlPoints[i + 1];
                }

                float distance = Vector3.Distance(currP, targetP);
                Vector3 dir = targetP - currP;
                if (distance <= 0.001f)
                {
                    dir = Vector3.zero;
                }
                else
                {
                    dir = dir.normalized;
                }

                controlPoints[i] =currP + distance * 0.5f * dir * dt;

            }
        }

        private void UpdateMeshData()
        {
            
            for (int i = controlPoints.Count; i >= 0 ; i--)
            {
                Vector3 centerPos;
                if (i == controlPoints.Count)
                {
                    for (int j = vertList.Count - 1; j >= vertList.Count - ringSlice ; j--)
                    {
                        centerPos = headPos;
                        meshVertList[j] =  centerPos + vertList[j];
                    }
                    
                }
                else
                {
                    centerPos = controlPoints[i];
                    for (int j = (i + 1) * ringSlice; j < (i + 2) * ringSlice; j++)
                    {
                        meshVertList[j] =  centerPos + vertList[j];
                    }
                }
            }
        }
        
       
        private Vector3 RotateRing(Vector3 oPos, Vector3 tVec)
        {
         
            
            return oPos;
        }
        //Utilt Func
        private Matrix4x4 QuatToMatrix(Quaternion quat)
        {
            Matrix4x4 mat = Matrix4x4.identity;
            float sqrt2 = Mathf.Sqrt(2.0f);
            float q0, q1, q2, q3;
            q0 = sqrt2 * quat.w;
            q1 = sqrt2 * quat.x;
            q2 = sqrt2 * quat.y;
            q3 = sqrt2 * quat.z;
            return mat;
        }
        private Quaternion AxisAngleToQuat(Vector3 axis, float angle)
        {
            Quaternion quat = Quaternion.identity;
            float sin = Mathf.Sin(angle / 2.0f);
            float cos = Mathf.Cos(angle / 2.0f);
            quat.x = axis.x * sin;
            quat.y = axis.y * sin;
            quat.z = axis.z * sin;
            quat.w = cos;
            return quat;
        }
        
        
        private Quaternion VecToQuat(Vector3 tVec)
        {
            Quaternion quat = quaternion.identity;
            Vector3 norVec = new Vector3(tVec.z, 0, -tVec.x);
            if (tVec.x + tVec.x < 0.0001)
            {
                norVec = Vector3.up;
            }

            norVec = norVec.normalized;
            float cos = tVec.y;
            float angle = Mathf.Acos(cos);
            quat = AxisAngleToQuat(norVec, angle);
            
            return quat;
        }
        
       
    }
    
    
    //
    private Mesh _mesh;

    private MeshFilter mf;
    
    private Drop drop;
    // Start is called before the first frame update
    void Start()
    {
        mf = GetComponent<MeshFilter>();
        if (!mf) return;
        drop = new Drop(Vector3.zero);
        _mesh = new Mesh
        {
            vertices = drop.meshVertList,
            triangles = drop.triList.ToArray()
        };
        mf.mesh = _mesh;
    }

    private void OnDestroy()
    {
        if (_mesh)
        {
            _mesh.Clear();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (drop != null)
        {
            drop?.MoveHeadPoint(Vector3.up + Vector3.right, 0.01f);
            List<Vector3> vl = new List<Vector3>();
            foreach (var pos in drop.meshVertList)
            {
                vl.Add(pos);
            }
            _mesh.SetVertices(vl);
        }
        
       
     



    }
}
