using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace WhiteSpot.EasyPrintWeb
{
	public static class Extensions
	{
		public static bool ContainsKey(this NameValueCollection queryString, string key)
		{
			return (queryString != null && queryString.AllKeys.Contains(key));
		}

		public static byte[] ToArray(this Stream stream)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				return ms.ToArray();
			}
		}

		public static string ToFormattedString<T>(this IEnumerable<T> source, string separator)
		{
			if (source == null) throw new ArgumentException("source can not be null.");

			if (separator == null) throw new ArgumentException("separator can not be null.");

			// A LINQ query to call ToString on each elements and constructs a string array.
			string[] array = (from s in source
							  where s != null
							  select s.ToString()).ToArray();

			// utilise builtin string.Join to concate elements with customizable separator.
			return string.Join(separator, array);
		}

		/// <summary>
		/// Returns true if the current string ends with at least one of the values in the array
		/// </summary>
		/// <param name="source"></param>
		/// <param name="values"></param>
		/// <param name="stringComparison"></param>
		/// <returns></returns>
		public static bool EndsWith(this string source, string[] values, StringComparison stringComparison)
		{
			if (values == null)
				throw new ArgumentNullException("values");

			foreach (string value in values)
			{
				if (source.EndsWith(value, stringComparison))
					return true;
			}

			return false;
		}

		public static string GetErrorsAsUnorderedList(this List<string> messages)
		{
			StringBuilder unorderedList = new StringBuilder();

			unorderedList.Append("<ul>");
			foreach (var message in messages)
			{
				unorderedList.AppendFormat("<li>{0}</li>", message);
			}
			unorderedList.Append("</ul>");
			
			return unorderedList.ToString();
		}
	}
}