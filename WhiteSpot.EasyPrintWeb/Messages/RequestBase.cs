using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhiteSpot.EasyPrintWeb.Messages
{
	public abstract class RequestBase
	{
		public string AccessToken { get; protected set; }
		public Uri SharePointUrl { get; protected set; }

		public RequestBase(string accessToken, Uri sharePointUrl)
		{
			AccessToken = accessToken;
			SharePointUrl = sharePointUrl;
		}

	}
}