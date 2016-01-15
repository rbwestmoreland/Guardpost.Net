namespace Guardpost.Net
{
    #region About
    // Guardpost
    // ===  
    // Guardpost is an address validation service. Given an arbitrary address
    // guardpost validates address based off syntax checks (RFC defined grammar),
    // DNS validation, spell checks, and if available, Email Service Provider
    // (ESP) specific local-part grammar.
    //
    // No addresses submitted to the guardpost service are ever stored on any
    // Rackspace servers. Nothing is persisted after the request is complete.
    //
    // Documentation: <https://api.mailgun.net/v2/address>
    #endregion About

    #region License, Terms, and Author(s)
    // The MIT License (MIT)
    //
    // Copyright (c) 2013 Bates Westmoreland
    //
    // Permission is hereby granted, free of charge, to any person obtaining a copy
    // of this software and associated documentation files (the "Software"), to deal
    // in the Software without restriction, including without limitation the rights
    // to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    // copies of the Software, and to permit persons to whom the Software is
    // furnished to do so, subject to the following conditions:
    //
    // The above copyright notice and this permission notice shall be included in
    // all copies or substantial portions of the Software.
    //
    // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    // FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    // THE SOFTWARE.
    #endregion License, Terms, and Author(s)

    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;

    public interface IGuardpostClient : IDisposable
    {
        /// <summary>
        /// Given an arbitrary address, validates address based off syntax checks, DNS
        /// validation, spell check, and if available, Email Service Provider (ESP)
        /// specific local-part grammar.
        /// </summary>
        /// <param name="address">An email address to validate. (Maximum: 512 characters)</param>
        /// <returns>A ValidationResponse.</returns>
        Task<ValidateResponse> ValidateAsync(string address);

        /// <summary>
        /// Parses an enumeration of email addresses into two lists: parsed
        /// addresses and unparsable portions. The parsed addresses are a list of
        /// addresses that are syntactically valid (and optionally have DNS and ESP
        /// specific grammar checks) the unparsable list is a list of characters
        /// sequences that the parser was not able to understand. These often align with
        /// invalid email addresses, but not always.
        /// </summary>
        /// <param name="addresses">An enumeration of addresses. (Maximum: 524288 characters)</param>
        /// <param name="syntaxOnly">Perform only syntax checks or DNS and ESP specific validation as well.</param>
        /// <returns>A ParseResponse.</returns>
        Task<ParseResponse> ParseAsync(IEnumerable<string> addresses, bool syntaxOnly);
    }

    public class ParseResponse
    {
        [JsonProperty("parsed")]
        public string[] Parsed { get; set; }

        [JsonProperty("unparseable")]
        public string[] Unparseable { get; set; }
    }

    public class ValidateResponse
    {
        [JsonProperty("is_valid")]
        public bool IsValid { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("parts")]
        public EmailParts Parts { get; set; }

        [JsonProperty("did_you_mean")]
        public string DidYouMean { get; set; }

        public class EmailParts
        {
            [JsonProperty("local_part")]
            public string LocalPart { get; set; }

            [JsonProperty("domain")]
            public string Domain { get; set; }
        }
    }

    public class HttpGuardpostClient : IGuardpostClient
    {
        private HttpClient _httpClient;

        /// <summary>
        /// Creates an instance of HttpGuardpostClient.
        /// </summary>
        /// <param name="apiKey">API Key in the My Account tab of your Mailgun account (the one with the “pub-key” prefix).</param>
        public HttpGuardpostClient(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentNullException("apiKey");

            var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "api", apiKey)));
            var baseAddress = new Uri("https://api.mailgun.net/v2/address/", UriKind.Absolute);
            _httpClient = new HttpClient { BaseAddress = baseAddress };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Guardpost.Net", "2.0.0"));
        }

        /// <summary>
        /// Given an arbitrary address, validates address based off syntax checks, DNS
        /// validation, spell check, and if available, Email Service Provider (ESP)
        /// specific local-part grammar.
        /// </summary>
        /// <param name="address">An email address to validate. (Maximum: 512 characters)</param>
        /// <returns>A ValidationResponse.</returns>
        public async Task<ValidateResponse> ValidateAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException("address");

            if (address.Length > 512)
                throw new ArgumentException("maximum of 512 characters", "address");

            var requestUri = string.Format("validate?address={0}", address);
            var httpResult = await _httpClient.GetAsync(requestUri).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (httpResult.StatusCode == HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<ValidateResponse>(content);
            else if (httpResult.StatusCode == HttpStatusCode.Unauthorized)
                throw new  InvalidOperationException("Invalid apiKey.");
            else
                throw new Exception("An unknown error occured.");
        }

        /// <summary>
        /// Parses an enumeration of email addresses into two lists: parsed
        /// addresses and unparsable portions. The parsed addresses are a list of
        /// addresses that are syntactically valid (and optionally have DNS and ESP
        /// specific grammar checks) the unparsable list is a list of characters
        /// sequences that the parser was not able to understand. These often align with
        /// invalid email addresses, but not always.
        /// </summary>
        /// <param name="addresses">An enumeration of addresses. (Maximum: 524288 characters)</param>
        /// <param name="syntaxOnly">Perform only syntax checks or DNS and ESP specific validation as well.</param>
        /// <returns>A ParseResponse.</returns>
        public async Task<ParseResponse> ParseAsync(IEnumerable<string> addresses, bool syntaxOnly)
        {
            if (addresses == null)
                throw new ArgumentNullException("addresses");

            if (!addresses.Any())
                return new ParseResponse { Parsed = new string[] { }, Unparseable = new string[] { } };

            var addressesJoined = string.Join(";", addresses);
            if (addressesJoined.Length > 524288)
                throw new ArgumentException("maximum of 524288 characters", "addresses");

            var requestUri = string.Format("parse?syntax_only={0}&addresses={1}", syntaxOnly, addressesJoined);
            var httpResult = await _httpClient.GetAsync(requestUri).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (httpResult.StatusCode == HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<ParseResponse>(content);
            else if (httpResult.StatusCode == HttpStatusCode.Unauthorized)
                throw new InvalidOperationException("Invalid apiKey.");
            else
                throw new Exception("An unknown error occured.");
        }

        #region IDisposable
        bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~HttpGuardpostClient()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _httpClient.Dispose();

            _disposed = true;
        }
        #endregion IDisposable
    }
}
