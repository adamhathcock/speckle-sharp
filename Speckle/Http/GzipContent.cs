﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Speckle.Http
{
  /// <summary>
  /// Derived HttpContent class that gzip's up the content, adds the required encoding headers, and saves you transaction space yo.
  /// </summary>
  internal sealed class GzipContent : HttpContent
  {
    private readonly HttpContent content;

    public GzipContent(HttpContent content)
    {
      if (content == null)
      {
        return;
      }

      this.content = content;

      // Keep the original content's headers.
      if (content != null)
        foreach (KeyValuePair<string, IEnumerable<string>> header in content.Headers)
        {
          Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

      // ... and let the server know we've Gzip-compressed the body of this request.
      Headers.ContentEncoding.Add("gzip");
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
    {
      // Open a GZipStream that writes to the specified output stream.
      using (GZipStream gzip = new GZipStream(stream, CompressionMode.Compress, true))
      {
        // Copy all the input content to the GZip stream.
        if (content != null)
          await content.CopyToAsync(gzip);
        else
          await (new System.Net.Http.StringContent(string.Empty)).CopyToAsync(gzip);
      }
    }

    protected override bool TryComputeLength(out long length)
    {
      length = -1;
      return false;
    }
  }
}
