using System;
using System.Collections.Generic;
using System.Linq;

namespace Kubernetes
{
    public class Controller : IController
    {
        public Dictionary<string, Pod> pods = new Dictionary<string, Pod>();
        public SortedDictionary<(int Port, string Namespace), Pod> sortedPods = new SortedDictionary<(int Port, string Namespace), Pod>();
        private Dictionary<int, List<Pod>> podsByPort = new Dictionary<int, List<Pod>>();
        private Dictionary<string, List<Pod>> podsByNamespace = new Dictionary<string, List<Pod>>();

        public bool Contains(string podId)
        {
            return pods.ContainsKey(podId);
        }

        public void Deploy(Pod pod)
        {
            pods.Add(pod.Id, pod);
            sortedPods.Add((pod.Port, pod.Namespace), pod);

            if (!podsByPort.ContainsKey(pod.Port))
            {
                podsByPort[pod.Port] = new List<Pod>();
            }
            podsByPort[pod.Port].Add(pod);

            if (!podsByNamespace.ContainsKey(pod.Namespace))
            {
                podsByNamespace[pod.Namespace] = new List<Pod>();
            }
            podsByNamespace[pod.Namespace].Add(pod);
        }

        public IEnumerable<Pod> GetPodsBetweenPort(int lowerBound, int upperBound)
        {
            return podsByPort.Where(kvp => kvp.Key >= lowerBound && kvp.Key <= upperBound).SelectMany(kvp => kvp.Value);
        }
        public IEnumerable<Pod> GetPodsInNamespace(string @namespace)
        {
            return podsByNamespace.ContainsKey(@namespace) ? podsByNamespace[@namespace] : Enumerable.Empty<Pod>();
        }

        public Pod GetPod(string podId)
        {
            if (!pods.ContainsKey(podId))
            {
                throw new ArgumentException();
            }
            return pods[podId];
        }

        public IEnumerable<Pod> GetPodsOrderedByPortThenByName()
        {
            return sortedPods.Values.OrderByDescending(p => p.Port).ThenBy(p => p.Namespace);
        }

        public int Size()
        {
            return pods.Count;
        }

        public void Uninstall(string podId)
        {
            if (pods.ContainsKey(podId))
            {
                pods.Remove(podId);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public void Upgrade(Pod pod)
        {
            if (pods.ContainsKey(pod.Id))
            {
                pods[pod.Id] = pod;
            }
            else
            {
                pods.Add(pod.Id, pod);
            }
        }
    }
}