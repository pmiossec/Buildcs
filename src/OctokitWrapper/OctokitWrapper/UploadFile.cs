using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace OctokitWrapper
{
	public class UploadFile
	{
		public string Path { get; set; }
		public string ContentType { get; set; }
	}

//    static class Ext
//    {
//        public static void MaybeSetMetadata(this TaskItem item, string name, string value)
//        {
//            if(value != null)
//                item.SetMetadata(name, value);
//        }
//    }
}
