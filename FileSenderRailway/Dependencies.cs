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
		void Send(Document document);
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

		public string Name { get; }
		public DateTime Created { get;  }
		public string Format { get; }
		public byte[] Content { get; }
	}


    public static class DocumentExtensions
    {
        public static Document Sign(this Document doc,
            ICryptographer cryptographer,
            X509Certificate certificate)
        {
            return new Document(doc.Name, cryptographer.Sign(doc.Content, certificate), doc.Created, doc.Format);
        }
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

	    public Result<Document> PrepareToSend(X509Certificate certificate, Func<FileContent,Document> recognise, ICryptographer cryptographer, Func<DateTime> now)
	    {
	        var doc = recognise(this);
	        if (!IsValidFormatVersion(doc))
	            return Result.Fail<Document>("Can't prepare file to send. Invalid format version: " + doc.Format);
	            //throw new FormatException("Invalid format version: " + doc.Format);
	        if (!IsValidTimestamp(doc, now))
	            return Result.Fail<Document>("Can't prepare file to send. Too old document: " + doc.Created);
            //throw new FormatException("Too old document: " + doc.Created);
            return Result.Ok<Document>(doc);
            return doc.Sign(cryptographer, certificate);
        }
	    private bool IsValidFormatVersion(Document doc)
	    {
	        return doc.Format == "4.0" || doc.Format == "3.1";
	    }

	    private bool IsValidTimestamp(Document doc, Func<DateTime> now)
	    {
	        var oneMonthBefore = now().AddMonths(-1);
	        return doc.Created > oneMonthBefore;
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