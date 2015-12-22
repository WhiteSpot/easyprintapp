using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhiteSpot.EasyPrintWeb.Messages
{
	public abstract class ResponseBase
	{
		public ResponseBase()
		{
			ErrorMessages = new List<string>();
		}

		public List<string> ErrorMessages { get; private set; }

		public string GetErrorMessage(string separator)
		{
			return ErrorMessages.ToFormattedString(separator);
		}

		public bool ContainsErrors
		{
			get { return ErrorMessages.Count > 0; }
		}

		public void AddErrorMessage(string message, params object[] args)
		{
			ErrorMessages.Add(string.Format(message, args));
		}
	}
}