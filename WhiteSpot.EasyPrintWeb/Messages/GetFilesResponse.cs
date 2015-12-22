using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.SharePoint.Client;

namespace WhiteSpot.EasyPrintWeb.Messages
{
	public class GetFilesResponse : ResponseBase
	{
		public Dictionary<File, byte[]> Files { get; private set; }
		public List<FieldUrlValue> Urls { get; private set; }

		public GetFilesResponse()
		{
			Files = new Dictionary<File, byte[]>();
			Urls = new List<FieldUrlValue>();
		}

		public void AddFile(File file, byte[] content)
		{
			Files.Add(file, content);
		}

		public void AddUrl(FieldUrlValue url)
		{
			Urls.Add(url);
		}
	}
}