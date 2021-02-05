using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManagementCenter
{
    class RouterConnection
    {
        private readonly Router router;
        private readonly TcpClient client;
        private readonly int id;

        public RouterConnection(Router router, TcpClient client, int id)
        {
            this.router = router;
            this.client = client;
            this.id = id;
        }

        public int GetID() {
            return id;
        }

        //public void SendMPLSTableAgain(ConfigLoader ConfigLoader) {
        //    NetworkStream stream = client.GetStream();

        //    foreach (Router r in ConfigLoader.GetRouters()) {

        //        LinkedList<string[]> rows = new LinkedList<string[]>();
        //        foreach (var tuple in ConfigLoader.routerMPLSTables) {
        //            if (tuple.Item1 == id) {
        //                var vals = tuple.Item2;
        //                string[] row = { vals.Item1.ToString(), vals.Item2.ToString(), vals.Item3.ToString(), vals.Item4,
        //                                             vals.Item5.ToString(), vals.Item6.ToString(), vals.Item7.ToString(), vals.Rest.Item1.ToString() };
        //                rows.AddLast(row);
        //            }
        //        }
        //        byte[] bytes = Server.SerializeObject(rows);
        //        stream.Write(bytes, 0, bytes.Length);
        //        GUIWindow.PrintLog("Sending MPLS table to Router #" + id);

        //        break;
        //    }
        //}
    }
}
