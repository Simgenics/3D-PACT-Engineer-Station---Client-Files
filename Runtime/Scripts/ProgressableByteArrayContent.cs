using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class ProgressableByteArrayContent : HttpContent
{
    private readonly byte[] _content;
    private readonly Action<long, long> _progress;

    public ProgressableByteArrayContent(byte[] content, Action<long, long> progress)
    {
        _content = content;
        _progress = progress;
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
    {
        long totalBytes = _content.Length;
        long totalSent = 0;
        int bufferSize = 4096; // Buffer size

        using (var memoryStream = new MemoryStream(_content))
        {
            byte[] buffer = new byte[bufferSize];
            int bytesRead;

            while ((bytesRead = await memoryStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await stream.WriteAsync(buffer, 0, bytesRead);
                totalSent += bytesRead;

                // Report progress
                _progress(totalSent, totalBytes);
            }
        }
    }

    protected override bool TryComputeLength(out long length)
    {
        length = _content.Length;
        return true;
    }

    protected override void Dispose(bool disposing)
    {
        // No resources to dispose in this implementation
        base.Dispose(disposing);
    }
}