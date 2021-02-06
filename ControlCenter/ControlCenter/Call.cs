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

        public Call(int connectionID, bool interDomainConnection, int startAsID) {
            this.connectionID = connectionID;
            this.interDomainConnection = interDomainConnection;
            this.startAsID = startAsID;
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
    }
}
