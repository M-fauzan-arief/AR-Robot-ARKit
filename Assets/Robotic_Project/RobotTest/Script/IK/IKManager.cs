/*
Copyright (c) 2022 FZI Forschungszentrum Informatik

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. 
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace BurstIK
{
    public class IKManager : MonoBehaviour
    {
        const float GRAD_EPS = 1e-2f;
        const float GRAD_EPS_SHIFT = 1e-3f;

        public static IKManager instance;

        public float DefaultRotationWeight = 8.0f;
        public float DefaultLocationWeight = 64.0f;
        public float DefaultPenaltyWeight = 128.0f;
        public int IterationsPerFrame = 1;

        private List<IKSystem> ik_systems;

        private NativeArray<RobotJoint.JointData> buffer_joint_data;
        private NativeArray<ComputeForwardIO> data_io;

        private bool bufferDirty;

        void Awake()
        {
            instance = this;
            ik_systems = new List<IKSystem>();
        }

        void Update()
        {
            if (bufferDirty)
            {
                RebuildBuffers();
                bufferDirty = false;
            }

            int k = 0;
            for (int i = 0; i < ik_systems.Count; ++i)
            {
                IKSystem s = ik_systems[i];
                for (int j = 0; j < s.joints.Length; ++j)
                {
                    RobotJoint.JointData jd = buffer_joint_data[k];
                    jd.Target = s.Target.transform.position;
                    jd.TargetRotation = s.Target.transform.rotation;
                    jd.rotGradWeight = s.EnableRotationalTarget ? DefaultRotationWeight : 0;
                    buffer_joint_data[k] = jd;
                    ++k;
                }

                ComputeForwardIO io = data_io[i];
                io.localToWorldMatrix = s.joints[0].transform.parent.localToWorldMatrix;
                data_io[i] = io;
            }

            JobHandle forwardHandle, inverseHandle;
            for (int i = 0; i < IterationsPerFrame; ++i)
            {
                ComputeForwardKinematics forwardJob = new ComputeForwardKinematics { data = buffer_joint_data, data_io = data_io };
                forwardHandle = forwardJob.Schedule(ik_systems.Count, 1);

                ComputeInverseGradientDescent inverseJob = new ComputeInverseGradientDescent
                {
                    data = buffer_joint_data,
                    deltaT = Time.deltaTime,
                    locationWeight = DefaultLocationWeight,
                    penaltyWeight = DefaultPenaltyWeight
                };
                inverseHandle = inverseJob.Schedule(buffer_joint_data.Length, 4, forwardHandle);

                JobHandle.CompleteAll(ref forwardHandle, ref inverseHandle);
            }

            k = 0;
            for (int i = 0; i < ik_systems.Count; ++i)
            {
                IKSystem s = ik_systems[i];
                for (int j = 0; j < s.joints.Length; ++j)
                {
                    RobotJoint.JointData jd = buffer_joint_data[k];
                    s.joints[j].ApplyForwardKinematics(jd.value, jd.localPosition, jd.localRotation);
                    ++k;
                }
            }
        }

        private void RebuildBuffers()
        {
            if (buffer_joint_data.IsCreated)
            {
                buffer_joint_data.Dispose();
                data_io.Dispose();
            }

            List<RobotJoint.JointData> l_joints = new List<RobotJoint.JointData>();
            data_io = new NativeArray<ComputeForwardIO>(ik_systems.Count, Allocator.TempJob);

            for (int i = 0; i < ik_systems.Count; ++i)
            {
                IKSystem s = ik_systems[i];
                ComputeForwardIO io;
                io.offset = new int2(l_joints.Count, s.joints.Length);
                io.position = 0;
                io.rotation = quaternion.identity;
                io.localToWorldMatrix = s.joints[0].transform.parent.localToWorldMatrix;
                data_io[i] = io;

                for (int j = 0; j < s.joints.Length; ++j)
                {
                    RobotJoint joint = s.joints[j];
                    RobotJoint.JointData jd;
                    jd.type = joint.Type;
                    jd.axis = joint.Axis;
                    jd.value = 0f;
                    jd.validValueInterval = joint.ValidValueInterval;
                    jd.maxSpeed = joint.MaxSpeed;
                    jd.localPosition = joint.transform.localPosition;
                    jd.localRotation = joint.transform.localRotation;
                    jd.P = 0;
                    jd.dP = 0;
                    jd.Q = quaternion.identity;
                    jd.dQ = quaternion.identity;
                    jd.Target = 0;
                    jd.TargetRotation = quaternion.identity;
                    jd.rotGradWeight = 0;
                    jd.curGrad = 0;
                    l_joints.Add(jd);
                }
            }

            buffer_joint_data = new NativeArray<RobotJoint.JointData>(l_joints.ToArray(), Allocator.Persistent);
            Debug.Log("[IKManager] Rebuilded Buffer: solvers: " + ik_systems.Count + ", buffer_size=" + l_joints.Count);
        }

        public void RegisterIK(IKSystem solver)
        {
            ik_systems.Add(solver);
            bufferDirty = true;
        }

        public void UnregisterIK(IKSystem solver)
        {
            ik_systems.Remove(solver);
            bufferDirty = true;
        }

        private void OnDestroy()
        {
            if (buffer_joint_data.IsCreated)
            {
                buffer_joint_data.Dispose();
                data_io.Dispose();
            }
        }

        public struct ComputeForwardIO
        {
            public int2 offset;
            public float4x4 localToWorldMatrix;
            public float3 position;
            public quaternion rotation;
        }

        [BurstCompile]
        public struct ComputeForwardKinematics : IJobParallelFor
        {
            [NativeDisableParallelForRestriction]
            public NativeArray<RobotJoint.JointData> data;

            public NativeArray<ComputeForwardIO> data_io;

            public void Execute(int index)
            {
                ComputeForwardIO io = data_io[index];
                int2 offset = io.offset;
                float3 prev = data[offset.x].localPosition;
                quaternion rotation = quaternion.identity;

                NativeArray<float3> prevs = new NativeArray<float3>(offset.y - 1, Allocator.Temp);
                NativeArray<quaternion> rotations = new NativeArray<quaternion>(offset.y - 1, Allocator.Temp);

                prevs[0] = data[offset.x].localPosition;
                rotations[0] = quaternion.identity;

                for (int i = 0; i < offset.y - 1; ++i)
                {
                    RobotJoint.JointData j0 = data[i + offset.x];
                    RobotJoint.JointData j1 = data[i + offset.x + 1];

                    if (j0.type == RobotJoint.JointType.SHIFT)
                    {
                        prev += math.mul(rotation, j0.axis * j0.value);
                        rotation = math.mul(rotation, j0.localRotation);

                        for (int j = 0; j < i; ++j)
                        {
                            prevs[j] += math.mul(rotations[j], j0.axis * j0.value);
                            rotations[j] = math.mul(rotations[j], j0.localRotation);
                        }
                        prevs[i] += math.mul(rotations[i], j0.axis * (j0.value + GRAD_EPS_SHIFT));
                        rotations[i] = math.mul(rotations[i], j0.localRotation);
                    }
                    else if (j0.type == RobotJoint.JointType.HINGE)
                    {
                        rotation = math.mul(rotation, math.mul(j0.localRotation, quaternion.AxisAngle(j0.axis, math.radians(j0.value))));

                        for (int j = 0; j < i; ++j)
                        {
                            rotations[j] = math.mul(rotations[j], math.mul(j0.localRotation, quaternion.AxisAngle(j0.axis, math.radians(j0.value))));
                        }
                        rotations[i] = math.mul(rotations[i], math.mul(j0.localRotation, quaternion.AxisAngle(j0.axis, math.radians(j0.value + GRAD_EPS))));
                    }

                    prev += math.mul(rotation, j1.localPosition);

                    for (int j = 0; j <= i; ++j)
                    {
                        prevs[j] += math.mul(rotations[j], j1.localPosition);
                    }
                    for (int j = i + 1; j < offset.y - 1; ++j)
                    {
                        prevs[j] = prev;
                        rotations[j] = rotation;
                    }
                }

                float4x4 rm = io.localToWorldMatrix;
                float3 c0 = math.normalize(new float3(rm.c0.x, rm.c0.y, rm.c0.z));
                float3 c1 = math.normalize(new float3(rm.c1.x, rm.c1.y, rm.c1.z));
                float3 c2 = math.normalize(new float3(rm.c2.x, rm.c2.y, rm.c2.z));
                float3x3 rm3 = new float3x3(c0, c1, c2);

                io.position = math.mul(io.localToWorldMatrix, new float4(prev, 1)).xyz;
                io.rotation = math.mul(new quaternion(rm3), rotation);
                data_io[index] = io;

                for (int i = 0; i < offset.y - 1; ++i)
                {
                    RobotJoint.JointData j0 = data[i + offset.x];
                    j0.dP = math.mul(io.localToWorldMatrix, new float4(prevs[i], 1)).xyz;
                    j0.dQ = math.mul(new quaternion(rm3), rotations[i]);
                    j0.P = io.position;
                    j0.Q = io.rotation;
                    data[i + offset.x] = j0;
                }

                prevs.Dispose();
                rotations.Dispose();
            }
        }

        [BurstCompile]
        public struct ComputeInverseGradientDescent : IJobParallelFor
        {
            public NativeArray<RobotJoint.JointData> data;
            public float deltaT;
            public float locationWeight;
            public float penaltyWeight;

            public void Execute(int index)
            {
                RobotJoint.JointData jd = data[index];
                bool isEndEffector = (index == data.Length - 1);

                if (isEndEffector)
                {
                    jd.rotGradWeight = 0;
                    jd.Target = float3.zero;
                    jd.TargetRotation = quaternion.identity;
                }

                float3x3 mQ = new float3x3(jd.Q);
                float3x3 mdQ = new float3x3(jd.dQ);
                float3x3 mTQ = new float3x3(jd.TargetRotation);

                if (!isEndEffector)
                {
                    float pen0 = math.exp(jd.validValueInterval.x - jd.value) + math.exp(jd.value - jd.validValueInterval.y);
                    float pen1 = math.exp(jd.validValueInterval.x - jd.value - (jd.type == RobotJoint.JointType.HINGE ? GRAD_EPS : GRAD_EPS_SHIFT)) + math.exp(jd.value + (jd.type == RobotJoint.JointType.HINGE ? GRAD_EPS : GRAD_EPS_SHIFT) - jd.validValueInterval.y);

                    float d0 = locationWeight * math.lengthsq(jd.Target - jd.P) + jd.rotGradWeight * (rotNorm(mQ.c0, mTQ.c0) + rotNorm(mQ.c1, mTQ.c1) + rotNorm(mQ.c2, mTQ.c2)) + pen0 * penaltyWeight;
                    float d1 = locationWeight * math.lengthsq(jd.Target - jd.dP) + jd.rotGradWeight * (rotNorm(mdQ.c0, mTQ.c0) + rotNorm(mdQ.c1, mTQ.c1) + rotNorm(mdQ.c2, mTQ.c2)) + pen1 * penaltyWeight;

                    float dPdv = (d1 - d0) / (jd.type == RobotJoint.JointType.HINGE ? GRAD_EPS : GRAD_EPS_SHIFT);
                    dPdv = math.lerp(jd.curGrad, dPdv, 0.2f);
                    jd.value -= jd.maxSpeed * linstep(0, 0.004f, math.abs(dPdv)) * math.sign(dPdv) * deltaT;
                }

                data[index] = jd;
            }

            float rotNorm(float3 a, float3 b) => 1 - math.dot(a, b);
            float linstep(float a, float b, float x) => math.clamp((x - a) / (b - a), 0, 1);
        }
    }
}
