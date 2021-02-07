using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter {
    class Util {

        public static Dictionary<string, string> DecodeRequest(string message) {
            string[] pieces = message.Split(';');
            Dictionary<string, string> data = new Dictionary<string, string>();
            foreach (string piece in pieces) {
                string[] keyAndValue = piece.Split(':');
                data.Add(keyAndValue[0], keyAndValue[1]);
            }
            return data;
        }

    }
}
