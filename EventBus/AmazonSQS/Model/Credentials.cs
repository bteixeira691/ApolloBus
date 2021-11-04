using System;
using System.Collections.Generic;
using System.Text;

namespace ApolloBus.AmazonSQS.Model
{
    public class Credentials
    {

        //
        // Summary:
        //     Gets the AccessKey property for the current credentials.
        public string AccessKey { get; set; }
        //
        // Summary:
        //     Gets the SecretKey property for the current credentials.
        public string SecretKey { get; set; }
        //
        // Summary:
        //     Gets the Token property for the current credentials.
        public string Token { get; set; }
        //
        // Summary:
        //     Gets the UseToken property for the current credentials. Specifies if Token property
        //     is non-empty.
        public bool UseToken { get; set; }
    }
}
