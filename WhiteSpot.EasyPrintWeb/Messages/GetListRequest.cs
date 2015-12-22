using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using WhiteSpot.EasyPrintWeb;

namespace WhiteSpot.EasyPrintWeb.Messages
{
	public class GetListRequest : RequestBase
	{
		private const string ListTitleProperty = "ListTitle";

		public NameValueCollection QueryString { get; protected set; }

		public GetListRequest(string accessToken, Uri sharePointUrl, NameValueCollection queryString) : base(accessToken, sharePointUrl)
		{
			QueryString = queryString;
			SharePointUrl = sharePointUrl;
		}

		public bool DoesListTitlePropertyExist
		{
			get { return QueryString.ContainsKey(ListTitleProperty); }
		}

		public string ListTitle
		{
			get
			{
				if (QueryString.ContainsKey(ListTitleProperty))
					return QueryString[ListTitleProperty];

				return null;
			}
		}
	}
}