using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Microsoft.SharePoint.Client;
using WhiteSpot.EasyPrintWeb.Messages;

namespace WhiteSpot.EasyPrintWeb
{
	public class SharePointHelper
	{
		private const string UrlFieldStaticName = "URL";
		private const string LinkContentTypeName = "Link";

		internal static GetListResponse GetList(GetListRequest request)
		{
			GetListResponse response = new GetListResponse();

			if (!request.DoesListTitlePropertyExist)
			{
				response.AddErrorMessage("The list title property has not been provided to the web part. Please contact the system administrator.");
				return response;
			}

			string listTitle = request.ListTitle;
			if (string.IsNullOrEmpty(listTitle))
			{
				response.AddErrorMessage("The list title property has not been set in the web part properties.");
				return response;
			}

			using (ClientContext context = TokenHelper.GetClientContextWithAccessToken(request.SharePointUrl.AbsoluteUri, request.AccessToken))
			{
				try
				{
					response.List = context.Web.Lists.GetByTitle(listTitle);
					context.Load(response.List, l => l.Title, l => l.ContentTypes.Include(ct => ct.Name), l => l.Fields.Include(f => f.StaticName));
					context.ExecuteQuery();

					// now we name sure the list is using the Link content type, and the URL field is present, as
					// these are the requirements for the list to be used for this app
					if (!response.List.ContentTypes.Any(ct => ct.Name == LinkContentTypeName))
					{
						response.AddErrorMessage(string.Format("The list <em>{0}</em> isn't a Links List.", listTitle));
						return response;
					}

					if (!response.List.Fields.Any(f => f.StaticName == UrlFieldStaticName))
					{
						response.AddErrorMessage(string.Format("The list <em>{0}</em> is missing the URL field, so it is not a valid Links List.", listTitle));
						return response;
					}

					return response;
				}
				catch (Exception ex)
				{
					response.AddErrorMessage(ex.Message);
					return response;
				}
			}
		}

		internal static GetFilesResponse GetFileUrls(GetFilesRequest request)
		{
			GetFilesResponse response = new GetFilesResponse();

			using (ClientContext context = TokenHelper.GetClientContextWithAccessToken(request.SharePointUrl.AbsoluteUri, request.AccessToken))
			{
				try
				{
					var list = context.Web.Lists.GetByTitle(request.ListTitle);

					var query = new CamlQuery();
					query.ViewXml = string.Format("<View Scope='RecursiveAll'><ViewFields><FieldRef Name='URL' /><FieldRef Name='Order' /></ViewFields><Query><OrderBy><FieldRef Name='Order'/></OrderBy></Query><RowLimit>{0}</RowLimit></View>", request.MaxNumberOfItems);

					var results = list.GetItems(query);
					context.Load(results, items => items.Include(i => i.Id, i => i[UrlFieldStaticName]));
					context.ExecuteQuery();

					foreach (var item in results)
					{
						FieldUrlValue fileUrl = item["URL"] as FieldUrlValue;
						if (fileUrl == null)
						{
							response.AddErrorMessage(string.Format("The record with id {0} in the list <em>{1}</em> has a URL field of type <em>{2}</em> which is incorrect.", item.Id, request.ListTitle, item[UrlFieldStaticName].GetType()));
						}
						else if (string.IsNullOrEmpty(fileUrl.Url))
						{
							response.AddErrorMessage(string.Format("The record with id {0} in the list <em>{1}</em> doesn't have a value in the URL field.", item.Id, request.ListTitle));
						}
						else
						{
							response.AddUrl(fileUrl);
						}
					}

					return response;
				}
				catch (Exception ex)
				{
					response.AddErrorMessage(ex.Message);
					return response;
				}
			}
		}

		private readonly static string[] ValidDocumentExtensions = new string[] { ".pdf", ".docx", ".doc" };

		internal static GetFilesResponse GetFiles(GetFilesRequest request)
		{
			GetFilesResponse response = new GetFilesResponse();

			using (ClientContext context = TokenHelper.GetClientContextWithAccessToken(request.SharePointUrl.AbsoluteUri, request.AccessToken))
			{
				context.Load(context.Site, s => s.Url);
				context.ExecuteQuery();

				List<string> fileUrlsToDownload = new List<string>();
				foreach (var url in request.Urls)
				{
					if (!url.Host.Equals(request.SharePointUrl.Host, StringComparison.CurrentCultureIgnoreCase))
					{
						response.AddErrorMessage("The file <em>{0}</em> doesn't reside in the current SharePoint tenant.", url);
						continue;
					}

					string fileUrl = string.IsNullOrEmpty(url.Query) ? url.AbsolutePath : url.AbsolutePath.Replace(url.Query, "");
					
					if (!fileUrl.EndsWith(ValidDocumentExtensions, StringComparison.CurrentCultureIgnoreCase))
					{
						response.AddErrorMessage(string.Format("The file {0} is not supported. We can only combine the following file types: [{1}]", fileUrl, ValidDocumentExtensions.ToFormattedString(" ")));
						continue;
					}

					if (!url.AbsoluteUri.StartsWith(context.Site.Url, StringComparison.CurrentCultureIgnoreCase))
					{
						response.AddErrorMessage("The file <em>{0}</em> doesn't reside in the current site collection.", url);
						continue;
					}

					fileUrlsToDownload.Add(fileUrl);
				}

				// there's no point in downloading the files if there is at least one error, since we 
				// won't be able to generate the document
				if (!response.ContainsErrors)
				{
					foreach (var fileUrl in fileUrlsToDownload)
					{
						try
						{
							var file = context.Web.GetFileByServerRelativeUrl(fileUrl);
							var contentStream = file.OpenBinaryStream();
							context.Load(file);
							context.ExecuteQuery();

							response.AddFile(file, contentStream.Value.ToArray());
						}
						catch (Exception ex)
						{
							response.AddErrorMessage("The following unexpected error occurred while trying to get the content from the file <em>{0}</em>.<br />{1}", fileUrl, ex.Message);
						}
					}
				}
			}

			return response;
		}
	}
}