using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitToRDFConverter
{
    public class myOwnClasses
    {
        public int userID { get; set; }
        public string body { get; set; }
        public string title { get; set; }
        public string Building { get; set; }

        public string changedRoomName { get; set; }


    }

    public class Building
    {
        public string name { get; set; }
        public string guid { get; set; }


    }
}
