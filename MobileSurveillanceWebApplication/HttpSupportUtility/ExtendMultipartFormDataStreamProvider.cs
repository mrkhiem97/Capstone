using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace MobileSurveillanceWebApplication.HttpSupportUtility
{
    public class ExtendMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
    {
        private const String PARAMETER_NAME = "name";

        private String filename;
        public ExtendMultipartFormDataStreamProvider(String root)
            : base(root)
        { }

        public override string GetLocalFileName(System.Net.Http.Headers.HttpContentHeaders headers)
        {
            String retVal = String.Empty;
            try
            {
                retVal = filename = headers.ContentDisposition.Name.Replace("\"", String.Empty);
            }
            catch (Exception ex)
            {
                retVal = base.GetLocalFileName(headers);
            }
            return retVal;
        }

        public string GetFilename()
        {
            return filename;
        }
    }
}