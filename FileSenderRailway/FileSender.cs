using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using ResultOf;

namespace FileSenderRailway
{
	public class FileSender
	{
		private readonly ICryptographer cryptographer;
		private readonly Func<DateTime> now;
		private readonly IRecognizer recognizer;
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
				var res = file.PrepareFileToSend(recognizer, cryptographer, certificate, now);
				if (res.IsSuccess)
					res = res.Then(d => sender.Send(d)).RefineError("Can't send. ");
				yield return new FileSendResult(file, res.Error);
			}
		}
	}
}