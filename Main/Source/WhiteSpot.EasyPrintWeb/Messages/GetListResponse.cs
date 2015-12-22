using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.SharePoint.Client;

namespace WhiteSpot.EasyPrintWeb.Messages
{
	public class GetListResponse : ResponseBase
	{
		public List List { get; set; }
	}
}