using System;
using System.Security.Cryptography.X509Certificates;
using ResultOf;

namespace FileSenderRailway
{
	public interface ICryptographer
	{
		byte[] Sign(byte[] content, X509Certificate certificate);
	}

	public interface IRecognizer
	{
		/// <exception cref="FormatException">Not recognized</exception>
		Document Recognize(FileContent file);
	}

	public interface ISender
	{
		/// <exception cref="InvalidOperationException">Can't send</exception>
		Result<Document> Send(Document document);
	}

	public class Document
	{
		public Document(string name, byte[] content, DateTime created, string format)
		{
			Name = name;
			Created = created;
			Format = format;
			Content = content;
		}

		public string Name { get;  }
		public DateTime Created { get; }
		public string Format { get;  }
		public byte[] Content { get;  }

		
	}
	

	public class FileContent
	{
		public FileContent(string name, byte[] content)
		{
			Name = name;
			Content = content;
		}

		public string Name { get; }
		public byte[] Content { get; }

		public Result<Document> PrepareFileToSend(IRecognizer recognizer, ICryptographer cryptographer, 
			X509Certificate certificate, Func<DateTime> now)
		{
			return Result.Of<Document>(() => recognizer.Recognize(this))
				.Then(IsValidFormatVersion)
				.Then(d => IsValidTimestamp(d, now))
				.Then(d => SignDocument(d, cryptographer, certificate))
				.RefineError("Can't prepare file to send");
//			if (!IsValidFormatVersion(doc))
//				throw new FormatException($"Invalid format version: {doc.Format}");
//			if (!IsValidTimestamp(doc))
//				throw new FormatException($"Too old document: {doc.Created}");
//			doc.Content = cryptographer.Sign(doc.Content, certificate);
		}

		private Result<Document> SignDocument(Document doc, ICryptographer cryptographer,
			X509Certificate certificate)
		{

			return Result.Of<Document>(() =>
				new Document(doc.Name, cryptographer.Sign(doc.Content, certificate), doc.Created, doc.Format));
		}
		private Result<Document> IsValidFormatVersion(Document doc)
		{
			if (doc.Format == "4.0" || doc.Format == "3.1")
				return Result.Ok(doc);
			return Result.Fail<Document>($"Invalid format version: {doc.Format}");
		}

		private Result<Document> IsValidTimestamp(Document doc, Func<DateTime> now)
		{
			var oneMonthBefore = now().AddMonths(-1);
			if (doc.Created > oneMonthBefore)
				return Result.Ok(doc);
			return Result.Fail<Document>($"Too old document: {doc.Created}");
		}
	}

	public class FileSendResult
	{
		public FileSendResult(FileContent file, string error = null)
		{
			File = file;
			Error = error;
		}

		public FileContent File { get; }
		public string Error { get; }
		public bool IsSuccess => Error == null;
	}
}