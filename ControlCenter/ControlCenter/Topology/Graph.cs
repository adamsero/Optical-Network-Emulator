using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
    class Graph {

        // No. of vertices in graph
        private int v;

        // adjacency list 
        private List<int>[] adjList;

        public List<List<int>> allPaths = new List<List<int>>();

        // Constructor 
        public Graph(int vertices) {

            // initialise vertex count
            this.v = vertices;

            // initialise adjacency list
            initAdjList();
        }

        // utility method to initialise
        // adjacency list
        private void initAdjList() {
            adjList = new List<int>[v];

            for (int i = 0; i < v; i++) {
                adjList[i] = new List<int>();
            }
        }

        // add edge from u to v 
        public void addEdge(int u, int v) {
            // Add v to u's list. 
            adjList[u].Add(v);
        }

        // Prints all paths from 
        // 's' to 'd' 
        public void printAllPaths(int s, int d) {
            allPaths.Clear();
            bool[] isVisited = new bool[v];
            List<int> pathList = new List<int>();

            // add source to path[] 
            pathList.Add(s);

            // Call recursive utility 
            printAllPathsUtil(s, d, isVisited, pathList);
        }

        // A recursive function to print 
        // all paths from 'u' to 'd'. 
        // isVisited[] keeps track of 
        // vertices in current path. 
        // localPathList<> stores actual 
        // vertices in the current path 
        private void printAllPathsUtil(int u, int d,
                                       bool[] isVisited,
                                       List<int> localPathList) {

            if (u.Equals(d)) {
                //GUIWindow.PrintLog(string.Join(" ", localPathList));
                allPaths.Add(new List<int>(localPathList));
                // if match found then no need 
                // to traverse more till depth 
                return;
            }

            // Mark the current node 
            isVisited[u] = true;

            // Recur for all the vertices 
            // adjacent to current vertex 
            foreach (int i in adjList[u]) {
                if (!isVisited[i]) {
                    // store current node 
                    // in path[] 
                    localPathList.Add(i);
                    printAllPathsUtil(i, d, isVisited,
                                      localPathList);

                    // remove current node 
                    // in path[] 
                    localPathList.Remove(i);
                }
            }

            // Mark the current node 
            isVisited[u] = false;
        }
    }
}
