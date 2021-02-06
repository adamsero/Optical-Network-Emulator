using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
    class Call {

        private int connectionID;
        private bool interDomainConnection;
        private int startAsID;
        private HostConnection startHostConnection;
        private HostConnection targetHostConnection;

        public Call(int connectionID, bool interDomainConnection, int startAsID, HostConnection startHostConnection, HostConnection targetHostConnection) {
            this.connectionID = connectionID;
            this.interDomainConnection = interDomainConnection;
            this.startAsID = startAsID;
            this.startHostConnection = startHostConnection;

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
    }
}
