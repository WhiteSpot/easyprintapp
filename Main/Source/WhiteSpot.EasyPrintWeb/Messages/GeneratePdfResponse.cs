using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhiteSpot.EasyPrintWeb.Messages
{
	public class GeneratePdfResponse : ResponseBase
	{
		public byte[] GeneratedPdfDocument { get; set; }
	}
}