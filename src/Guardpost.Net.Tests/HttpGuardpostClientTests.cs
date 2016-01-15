namespace Guardpost.Net.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    [TestClass]
    public class HttpGuardpostClientTests
    {
        /// <summary>
        /// API Key in the My Account tab of your Mailgun account (the one with the “pub-key” prefix).
        /// </summary>
        public static string MailgunPublicApiKey = "pubkey-89ou0nuo2u5b3q7tn8bsz0vitmekk721";

        [TestClass]
        public class ConstructorTests : HttpGuardpostClientTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void WhenApiKeyIsNullOrEmpty_ShouldThrow()
            {
                new HttpGuardpostClient(null);
            }

            [TestMethod]
            public void WhenApiKeyIsNotNullOrEmpty_ShouldReturn()
            {
                new HttpGuardpostClient(MailgunPublicApiKey);
            }
        }

        [TestClass]
        public class ValidateAsyncTests : HttpGuardpostClientTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task WhenAddressIsNullOrEmpty_ShouldThrow()
            {
                using (var client = new HttpGuardpostClient(MailgunPublicApiKey))
                {
                    await client.ValidateAsync(null).ConfigureAwait(false);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task WhenAddressIsGreaterThan512Characters_ShouldThrow()
            {
                using (var client = new HttpGuardpostClient(MailgunPublicApiKey))
                {
                    var address = new string('a', 513);
                    await client.ValidateAsync(address).ConfigureAwait(false);
                }
            }

            [TestMethod]
            public async Task WhenAddressIsInvalid_ShouldReturn()
            {
                using (var client = new HttpGuardpostClient(MailgunPublicApiKey))
                {
                    //john@gmail.com: Does not meet Gmail minimum local-part length of 6 characters.
                    var address = "john@gmail.com";
                    var validateResponse = await client.ValidateAsync(address).ConfigureAwait(false);

                    Assert.IsNotNull(validateResponse);
                    Assert.IsFalse(validateResponse.IsValid);
                    Assert.AreEqual(address, validateResponse.Address);
                    Assert.IsNotNull(validateResponse.Parts);
                    Assert.IsNull(validateResponse.Parts.Domain);
                    Assert.IsNull(validateResponse.Parts.LocalPart);
                    Assert.IsNull(validateResponse.DidYouMean);
                }
            }

            [TestMethod]
            public async Task WhenAddressIsValid_ShouldReturn()
            {
                using (var client = new HttpGuardpostClient(MailgunPublicApiKey))
                {
                    //johnsmith@gmail.com: Meets Gmail 6 character minimum and all other requirements.
                    var address = "johnsmith@gmail.com";
                    var validateResponse = await client.ValidateAsync(address).ConfigureAwait(false);

                    Assert.IsNotNull(validateResponse);
                    Assert.IsTrue(validateResponse.IsValid);
                    Assert.AreEqual(address, validateResponse.Address);
                    Assert.IsNotNull(validateResponse.Parts);
                    Assert.AreEqual("gmail.com", validateResponse.Parts.Domain);
                    Assert.AreEqual("johnsmith", validateResponse.Parts.LocalPart);
                    Assert.IsNull(validateResponse.DidYouMean);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task WhenApiKeyIsInvalid_ShouldThrow()
            {
                using (var client = new HttpGuardpostClient("invalidApiKey"))
                {
                    await client.ValidateAsync("john.smith@gmail.com").ConfigureAwait(false);
                }
            }
        }

        [TestClass]
        public class ParseAsyncTests : HttpGuardpostClientTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task WhenAddressesIsNull_ShouldThrow()
            {
                using (var client = new HttpGuardpostClient(MailgunPublicApiKey))
                {
                    await client.ParseAsync(null, true).ConfigureAwait(false);
                }
            }

            [TestMethod]
            public async Task WhenAddressesIsEmpty_ShouldReturn()
            {
                using (var client = new HttpGuardpostClient(MailgunPublicApiKey))
                {
                    var parseResponse = await client.ParseAsync(Enumerable.Empty<string>(), true).ConfigureAwait(false);

                    Assert.IsNotNull(parseResponse);
                    Assert.IsNotNull(parseResponse.Parsed);
                    Assert.IsNotNull(parseResponse.Unparseable);
                    Assert.IsFalse(parseResponse.Parsed.Any());
                    Assert.IsFalse(parseResponse.Unparseable.Any());
                }
            }

            [TestMethod]
            public async Task WhenSyntaxOnlyIsTrue()
            {
                using (var client = new HttpGuardpostClient(MailgunPublicApiKey))
                {
                    var parseResponse = await client.ParseAsync(new[] { "johnsmith@gmail.com", "john@gmail.com", "gmail.com" }, true).ConfigureAwait(false);

                    Assert.IsNotNull(parseResponse);
                    Assert.IsNotNull(parseResponse.Parsed);
                    Assert.IsNotNull(parseResponse.Unparseable);
                    Assert.AreEqual(2, parseResponse.Parsed.Length);
                    Assert.IsTrue(parseResponse.Parsed.Contains("johnsmith@gmail.com"));
                    Assert.IsTrue(parseResponse.Parsed.Contains("john@gmail.com"));
                    Assert.AreEqual(1, parseResponse.Unparseable.Length);
                    Assert.IsTrue(parseResponse.Unparseable.Contains("gmail.com"));
                }
            }

            [TestMethod]
            public async Task WhenSyntaxOnlyIsFalse()
            {
                using (var client = new HttpGuardpostClient(MailgunPublicApiKey))
                {
                    var parseResponse = await client.ParseAsync(new[] { "johnsmith@gmail.com", "john@gmail.com", "gmail.com" }, false).ConfigureAwait(false);

                    Assert.IsNotNull(parseResponse);
                    Assert.IsNotNull(parseResponse.Parsed);
                    Assert.IsNotNull(parseResponse.Unparseable);
                    Assert.AreEqual(1, parseResponse.Parsed.Length);
                    Assert.IsTrue(parseResponse.Parsed.Contains("johnsmith@gmail.com"));
                    Assert.AreEqual(2, parseResponse.Unparseable.Length);
                    Assert.IsTrue(parseResponse.Unparseable.Contains("john@gmail.com"));
                    Assert.IsTrue(parseResponse.Unparseable.Contains("gmail.com"));
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task WhenApiKeyIsInvalid_ShouldThrow()
            {
                using (var client = new HttpGuardpostClient("invalidApiKey"))
                {
                    await client.ParseAsync(new[] { "johnsmith@gmail.com" }, false).ConfigureAwait(false);
                }
            }
        }
    }
}
