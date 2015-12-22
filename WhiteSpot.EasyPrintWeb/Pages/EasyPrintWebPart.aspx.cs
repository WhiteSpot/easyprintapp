using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microsoft.SharePoint.Client;
using WhiteSpot.EasyPrintWeb.Messages;

namespace WhiteSpot.EasyPrintWeb.Pages
{
	public partial class EasyPrintWebPart : Page
	{
		#region Page and Control Events

		protected void Page_Load(object sender, EventArgs e)
		{
			try
			{
				PrintAllButton.Attributes.Add("onclick", "printAllInProgress();" + ClientScript.GetPostBackEventReference(PrintAllButton, "").ToString());

				Uri sharePointUrl = GetSharePointUrl();
				if (sharePointUrl == null)
				{
					DisplayErrorMessage("This app can only be accessed from an authenticated host.");
					return;
				}

				SetStyleSheet(sharePointUrl);
				SetButtonText();

				if (InEditMode)
					return;

				string accessToken = GetAccessToken(sharePointUrl);

				if (string.IsNullOrEmpty(accessToken))
				{
					DisplayErrorMessage("This app can only be accessed from an authenticated host.");
					return;
				}

				var listResponse = SharePointHelper.GetList(new GetListRequest(accessToken, sharePointUrl, Request.QueryString));
				if (listResponse.ContainsErrors)
				{
					// we could not retrieve the links list based on the information provided, 
					// so we'll display the error message and stop processing
					DisplayErrorMessage(listResponse.GetErrorMessage("<br />"));
					return;
				}

				var filesResponse = SharePointHelper.GetFileUrls(new GetFilesRequest(accessToken, sharePointUrl, listResponse.List.Title, GetMaxNumberOfItems()));
				if (filesResponse.ContainsErrors)
				{
					StringBuilder errorMessage = new StringBuilder();
					errorMessage.AppendFormat("The following {0} occurred while trying to retrieve the file information from the list:", filesResponse.ErrorMessages.Count > 1 ? "errors" : "error");
					errorMessage.Append(filesResponse.ErrorMessages.GetErrorsAsUnorderedList());

					DisplayErrorMessage(errorMessage.ToString());
					return;
				}

				// since there aren't any errors, we render out the document links
				RenderLinks(filesResponse.Urls);

				PrintAllButton.CommandArgument = accessToken;
				PrintAllButton.Enabled = true;
			}
			catch (Exception ex)
			{
				DisplayErrorMessage(string.Format("The following unexpected error occurred:<br />{0}", ex.Message));
			}
		}

		private const string HrefRegexPattern = @"<(?<Tag_Name>(a)|img)\b[^>]*?\b(?<URL_Type>(?(1)href|src))\s*=\s*(?:""(?<URL>(?:\\""|[^""])*)""|'(?<URL>(?:\\'|[^'])*)')";

		protected void PrintAllButton_Click(object sender, EventArgs e)
		{
			if (InEditMode)
				return;

			RemoveOldDocuments();

			string accessToken = ((Button)sender).CommandArgument;
			Uri sharePointUrl = GetSharePointUrl();

			List<Uri> urls = GetCheckedItems();
			if (urls.Count > 0)
			{
				var response = SharePointHelper.GetFiles(new GetFilesRequest(accessToken, GetSharePointUrl(), urls));
				if (response.ContainsErrors)
				{
					StringBuilder errorMessage = new StringBuilder();
					errorMessage.AppendFormat("<span>The following {0} occurred while trying to retrieve the selected documents:</span>", response.ErrorMessages.Count > 1 ? "errors" : "error");
					errorMessage.Append(response.ErrorMessages.GetErrorsAsUnorderedList());

					DisplayErrorMessage(errorMessage.ToString());
				}
				else
				{
					var generatePdfResponse = DocumentHelper.GeneratePdf(response.Files);

					if (generatePdfResponse.ContainsErrors)
					{
						StringBuilder errorMessage = new StringBuilder();
						errorMessage.AppendFormat("<span>The following {0} occurred while trying to generate the PDF document:</span>", generatePdfResponse.ErrorMessages.Count > 1 ? "errors" : "error");
						errorMessage.Append(generatePdfResponse.ErrorMessages.GetErrorsAsUnorderedList());

						DisplayErrorMessage(errorMessage.ToString());
					}
					else
					{
						byte[] document = generatePdfResponse.GeneratedPdfDocument;
						string pdfFileName = GetPdfFileName();
						System.IO.File.WriteAllBytes(System.IO.Path.Combine(Server.MapPath("/Docs"), pdfFileName), document);

						string message = string.Format("The PDF document has been created from the selected files. Please <a target='_blank' href='/Docs/{0}'>click here</a> to download it.", pdfFileName);
						DisplayMessage(message);
					}
				}
			}
			else
			{
				DisplayErrorMessage("None of the documents were checked.");
			}
		}

		private void RemoveOldDocuments()
		{
			if (!System.IO.Directory.Exists(Server.MapPath("/Docs")))
				System.IO.Directory.CreateDirectory(Server.MapPath("/Docs"));

			System.Diagnostics.Trace.TraceInformation("RemoveOldDocuments: We're about to retrieve all of the files in the /Docs folder");
			string[] fileNames = System.IO.Directory.GetFiles(Server.MapPath("/Docs"));
			System.Diagnostics.Trace.TraceInformation("RemoveOldDocuments: There are [{0}] files in the /Docs folder", fileNames.Count());
			foreach (string fileName in fileNames)
			{
				System.IO.FileInfo fi = new System.IO.FileInfo(fileName);

				if ((DateTime.UtcNow - fi.CreationTimeUtc).Days > 2)
				{
					System.Diagnostics.Trace.TraceInformation("RemoveOldDocuments: Deleting the file {0} ...", fi.Name);
					fi.Delete();
					System.Diagnostics.Trace.TraceInformation("RemoveOldDocuments: The file {0} was deleted", fi.Name);
				}
				else
				{
					System.Diagnostics.Trace.TraceInformation("RemoveOldDocuments: The file {0} was created {1} so it is not being deleted.", fi.Name, fi.CreationTimeUtc);
				}
			}
		}

		private string GetPdfFileName()
		{
			return string.Format("GeneratedDocument-{0}.pdf", Guid.NewGuid().ToString("N"));
		}

		#endregion

		private string GetAccessToken(Uri sharePointUrl)
		{
			if (IsPostBack && !string.IsNullOrEmpty(PrintAllButton.CommandArgument))
			{
				return PrintAllButton.CommandArgument;
			}

			string contextTokenString = TokenHelper.GetContextTokenFromRequest(Page.Request);
			if (!string.IsNullOrEmpty(contextTokenString))
			{
				SharePointContextToken contextToken = TokenHelper.ReadAndValidateContextToken(contextTokenString, Request.Url.Authority);
				return TokenHelper.GetAccessToken(contextToken, sharePointUrl.Authority).AccessToken;
			}

			return null;
		}

		private Uri GetSharePointUrl()
		{
			if (!Request.QueryString.ContainsKey("SPHostUrl"))
				return null;

			return new Uri(Page.Request["SPHostUrl"]);
		}

		#region CheckBox Methods

		private const string CheckBoxIdPrefix = "DocumentToPrint";

		private void RenderLinks(List<FieldUrlValue> urls)
		{
			if (urls.Count == 0)
				return;

			AddCheckBox(CheckBoxes, "Select all documents", "select-all", true, true);

			int i = 0;
			foreach (var url in urls)
			{
				string text = string.Format("<a target='_blank' href='{0}'>{1}</a>", url.Url, url.Description);
				AddCheckBox(CheckBoxes, text, CheckBoxIdPrefix + i++, true);
			}
		}

		private void AddCheckBox(Control container, string text, string id, bool isChecked)
		{
			AddCheckBox(container, text, id, isChecked, false);
		}

		private void AddCheckBox(Control container, string text, string id, bool isChecked, bool isSelectAll)
		{
			HtmlGenericControl div = new HtmlGenericControl("div");
			var checkBox = new CheckBox { Text = text, ID = id, Checked = isChecked };
			
			if (isSelectAll)
			{
				checkBox.Attributes.Add("onClick", "checkAll();");
			}
			div.Controls.Add(checkBox);
			div.Attributes.Add("class", "hcf-form-checkbox");
			container.Controls.Add(div);
		}

		private List<Uri> GetCheckedItems()
		{
			List<Uri> urls = new List<Uri>();

			int i = 0;
			CheckBox checkbox = null;
			do
			{
				checkbox = FindControl(CheckBoxIdPrefix + i++) as CheckBox;
				if (checkbox != null)
				{
					if (!checkbox.Checked)
						continue;

					var match = Regex.Match(checkbox.Text, HrefRegexPattern, RegexOptions.IgnoreCase);
					urls.Add(new Uri(match.Groups["URL"].Value));
				}
				else
				{
					break;
				}
			}
			while (true);

			return urls;
		}

		#endregion

		#region Display Methods

		private const string ButtonTextProperty = "ButtonText";

		private void SetButtonText()
		{
			PrintAllButton.Text = string.IsNullOrEmpty(Request[ButtonTextProperty]) ? "Print all" : Request[ButtonTextProperty];
		}

		private void SetStyleSheet(Uri sharePointUrl)
		{
			string stylesheetUrl = string.Format("<link rel='stylesheet' href='{0}/_layouts/15/defaultcss.ashx' />", sharePointUrl.AbsoluteUri);
			Page.Header.Controls.Add(new LiteralControl(stylesheetUrl));
			Page.Header.Controls.Add(new LiteralControl("<link rel='stylesheet' href='/styles/style.css' />"));
		}

		private void DisplayErrorMessage(string message)
		{
			// TODO: decide how error messages should be displayed
			this.MessageLiteral.Text = string.Format("<div class=\"hcf-error\">{0}</div>", message);
			this.MessageLiteral.Visible = true;
		}

		private void DisplayMessage(string message, params object[] args)
		{
			this.MessageLiteral.Text = string.Format("<div class=\"hcf-success\">" + message + "</div>", args);
			this.MessageLiteral.Visible = true;
		}

		#endregion

		private const string MaxNumberOfItemsProperty = "MaxNumberOfItems";
		private const int DefaultMaxNumberOfItems = 100;

		private int GetMaxNumberOfItems()
		{
			int maxNumberOfItems;
			if (int.TryParse(Request[MaxNumberOfItemsProperty], out maxNumberOfItems) && maxNumberOfItems > 0 && maxNumberOfItems < 200)
			{
				return maxNumberOfItems;
			}

			return DefaultMaxNumberOfItems;
		}

		private const string EditModeProperty = "editmode";

		public bool InEditMode
		{
			get
			{
				if (Request.QueryString.ContainsKey(EditModeProperty))
				{
					int flag;

					if (int.TryParse(Request.QueryString[EditModeProperty], out flag))
					{
						return flag == 1;
					}
				}

				return false;
			}
		}
	}
}