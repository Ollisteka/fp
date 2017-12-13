using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using ResultOf;

namespace FileSenderRailway
{
	public class FileSender
	{
		private readonly ICryptographer cryptographer;
		private readonly IRecognizer recognizer;
		private readonly Func<DateTime> now;
		private readonly ISender sender;

		public FileSender(
			ICryptographer cryptographer,
			ISender sender,
			IRecognizer recognizer,
			Func<DateTime> now)
		{
			this.cryptographer = cryptographer;
			this.sender = sender;
			this.recognizer = recognizer;
			this.now = now;
		}

		public IEnumerable<FileSendResult> SendFiles(FileContent[] files, X509Certificate certificate)
		{
            
			foreach (var file in files)
			{
                //string errorMessage = null;
			    Result<Document> result;
                try
                {

                     result = file.PrepareToSend(certificate, f => recognizer.Recognize(f), cryptographer, now);

				    if (result.IsSuccess)
				       sender.Send(result.Value);

            	}
            //catch (FormatException e)
            //{
            //	errorMessage = "Can't prepare file to send. " + e.Message;
            //}
                catch (InvalidOperationException e)
                {
                   result = Result.Fail<Document>("Can't send. " + e.Message);
                }
                yield return new FileSendResult(file, result.Error);
			}
		}

		
	}
}