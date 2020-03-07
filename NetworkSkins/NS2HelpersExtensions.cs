using ColossalFramework;
using ICities;
using UnityEngine;
using System;

namespace NetworkSkins
{
    internal static class NS2HelpersExtensions
    {
        internal static ref NetNode ToNode(this ushort nodeID) => ref NetManager.instance.m_nodes.m_buffer[nodeID];
        internal static ref NetSegment ToSegment(this ushort nodeID) => ref NetManager.instance.m_segments.m_buffer[nodeID];
        
        internal static ushort GetFirstSegment(this NetNode node)
        {
            ushort ret = 0;
            for(int i = 0; i < 8; ++i)
            {
                ret = node.GetSegment(i);
                if (ret > 0)
                    break;
            }
            return ret;
        }

        internal static void Assert(bool con, string m)
        {
            if (!con)
            {
                Debug.LogAssertion("Assertion failed: " + m + "\n" + Environment.StackTrace);
            }
        }

        internal static void AssertNotNull(object obj, string m)
        {
            if (obj == null)
            {
                Debug.LogAssertion($"Assertion failed: expected {m} != null" + m + "\n" + Environment.StackTrace);
            }
        }

        internal static bool InSimulationThread() => 
            System.Threading.Thread.CurrentThread == SimulationManager.instance.m_simulationThread;
    }
}
