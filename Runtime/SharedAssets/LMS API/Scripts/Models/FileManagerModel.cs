namespace LMS.Models
{

	public class FileUploadResponse
	{
		
	}

	public class FileDownloadResponse
	{
		public string base64string;
		public string contentType;
		public string filename;
	}

	public class FileStreamResponse
	{
		public byte[] fileBytes;
		
		public FileStreamResponse(byte[] fileBytes)
		{
			this.fileBytes = fileBytes;
		}
	}
}
