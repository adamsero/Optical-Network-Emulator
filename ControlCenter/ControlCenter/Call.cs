using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
    class Call {

        private int connectionID;
        private bool interDomainConnection;
        private bool throughSubnetwork;
        private int startAsID;
        private HostConnection startHostConnection;
        private HostConnection targetHostConnection;
        public Path path;

        public Call(int connectionID, bool interDomainConnection, int startAsID, bool throughSubnetwork , HostConnection startHostConnection, HostConnection targetHostConnection, Path path) {
            this.connectionID = connectionID;
            this.interDomainConnection = interDomainConnection;
            this.startAsID = startAsID;
            this.startHostConnection = startHostConnection;
            this.targetHostConnection = targetHostConnection;
            this.throughSubnetwork = throughSubnetwork;
            this.path = path;
        }

        public int GetConnectionID() {
            return connectionID;
        }

        public bool GetInterDomainConnectionFlag() {
            return interDomainConnection;
        }

        public int GetStartAsID() {
            return startAsID;
        }

        public HostConnection GetStartHostConnection() {
            return startHostConnection;
        }

        public HostConnection GetTargetHostConnection() {
            return targetHostConnection;
        }

        public bool GetThroughSubnetwork() {
            return throughSubnetwork;
        }

        public Path GetPath() {
            return path;        
        }
    }
}
