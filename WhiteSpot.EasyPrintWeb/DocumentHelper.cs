using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Aspose.Pdf.Facades;
using Aspose.Words;
using WhiteSpot.EasyPrintWeb.Messages;

namespace WhiteSpot.EasyPrintWeb
{
	public class DocumentHelper
	{
		private const string PdfLicenseName = "Aspose.Pdf.lic";
		private const string WordLicenseName = "Aspose.Words.lic";

		internal static GeneratePdfResponse GeneratePdf(Dictionary<Microsoft.SharePoint.Client.File, byte[]> filesToMerge)
		{
			GeneratePdfResponse response = new GeneratePdfResponse();

			try
			{
				SetLicenses();

				Stream[] inputStreams = GetInputStreams(filesToMerge, response);

				if (response.ContainsErrors)
					return response;

				MemoryStream outputStream = new MemoryStream();

				PdfFileEditor editor = new PdfFileEditor();
				editor.AllowConcatenateExceptions = false;
				editor.CorruptedFileAction = PdfFileEditor.ConcatenateCorruptedFileAction.ConcatenateIgnoringCorrupted;
				editor.CloseConcatenatedStreams = true;

				bool generated = editor.Concatenate(inputStreams, outputStream);

				if (!generated)
				{
					foreach (var item in editor.CorruptedItems)
					{
						response.AddErrorMessage("The document [{0}] is corrupt.", filesToMerge.Keys.ElementAt(item.Index).Name);
					}

					return response;
				}

				response.GeneratedPdfDocument = outputStream.ToArray();
			}
			catch (Exception ex)
			{
				string message = string.Format("The following unexpected error occurred while generating the PDF document: <em>{0}</em>", ex.Message);
				response.AddErrorMessage(message);
			}

			return response;
		}

		private static Stream[] GetInputStreams(Dictionary<Microsoft.SharePoint.Client.File, byte[]> filesToMerge, GeneratePdfResponse response)
		{
			List<Stream> inputStreams = new List<Stream>();

			// for now, we're only supporting PDF and Word documents ... so if the file isn't a PDF
			// document, we can assume it is a Word document
			foreach (var item in filesToMerge)
			{
				try
				{
					MemoryStream inputStream;

					if (item.Key.Name.EndsWith(".pdf"))
					{
						inputStream = new MemoryStream(item.Value);
					}
					else
					{
						Document document = new Document(new MemoryStream(item.Value));
						inputStream = new MemoryStream();
						document.Save(inputStream, SaveFormat.Pdf);
					}

					inputStreams.Add(inputStream);
				}
				catch (Exception ex)
				{
					string message = string.Format("The following unexpected error occurred while processing the contents of the file {0}<br /><em>{1}</em>", item.Key.Name, ex.Message);
					response.AddErrorMessage(message);
				}
			}

			return inputStreams.ToArray();
		}

		private static void SetLicenses()
		{
			Aspose.Pdf.License pdfLicense = new Aspose.Pdf.License();
			pdfLicense.SetLicense(PdfLicenseName);

			Aspose.Words.License wordLicense = new Aspose.Words.License();
			wordLicense.SetLicense(WordLicenseName);
		}
	}
}