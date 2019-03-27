﻿using System.IO;
using System.Threading.Tasks;
using MediaBrowser.Model.Net;

namespace EmbyStat.Common.Net
{
    public interface IAsyncHttpClient
    {
	    Task<Stream> SendAsync(HttpRequest options);
	    Task<HttpResponse> GetResponse(HttpRequest options, bool sendFailureResponse = false);
	}
}
