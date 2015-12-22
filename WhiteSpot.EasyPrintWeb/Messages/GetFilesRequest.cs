using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.SharePoint.Client;

namespace WhiteSpot.EasyPrintWeb.Messages
{
	public class GetFilesRequest : RequestBase
	{
		public string ListTitle { get; private set; }
		public List<Uri> Urls { get; private set; }
		public int MaxNumberOfItems { get; private set; }

		public GetFilesRequest(string accessToken, Uri sharePointUrl, string listTitle, int maxNumberOfItems) 
			: base(accessToken, sharePointUrl)
		{
			ListTitle = listTitle;
			Urls = new List<Uri>();
			MaxNumberOfItems = maxNumberOfItems;
		}

		public GetFilesRequest(string accessToken, Uri sharePointUrl, List<Uri> urls)
			: base(accessToken, sharePointUrl)
		{
			Urls = urls;
		}
	}
}