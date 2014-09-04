namespace Guardpost.Net.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq;

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
        public class ValidateTests : HttpGuardpostClientTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void WhenAddressIsNullOrEmpty_ShouldThrow()
            {
                using (var client = new HttpGuardpostClient(MailgunPublicApiKey))
                {
                    client.Validate(null);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void WhenAddressIsGreaterThan512Characters_ShouldThrow()
            {
                using (var client = new HttpGuardpostClient(MailgunPublicApiKey))
                {
                    var address = new string('a', 513);
                    client.Validate(address);
                }
            }

            [TestMethod]
            public void WhenAddressIsInvalid_ShouldReturn()
            {
                using (var client = new HttpGuardpostClient(MailgunPublicApiKey))
                {
                    //john@gmail.com: Does not meet Gmail minimum local-part length of 6 characters.
                    var address = "john@gmail.com";
                    var validateResponse = client.Validate(address);

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
            public void WhenAddressIsValid_ShouldReturn()
            {
                using (var client = new HttpGuardpostClient(MailgunPublicApiKey))
                {
                    //john.smith@gmail.com: Meets Gmail 6 character minimum and all other requirements.
                    var address = "john.smith@gmail.com";
                    var validateResponse = client.Validate(address);

                    Assert.IsNotNull(validateResponse);
                    Assert.IsTrue(validateResponse.IsValid);
                    Assert.AreEqual(address, validateResponse.Address);
                    Assert.IsNotNull(validateResponse.Parts);
                    Assert.AreEqual("gmail.com", validateResponse.Parts.Domain);
                    Assert.AreEqual("john.smith", validateResponse.Parts.LocalPart);
                    Assert.IsNull(validateResponse.DidYouMean);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void WhenApiKeyIsInvalid_ShouldThrow()
            {
                using (var client = new HttpGuardpostClient("invalidApiKey"))
                {
                    client.Validate("john.smith@gmail.com");
                }
            }
        }

        [TestClass]
        public class ParseTests : HttpGuardpostClientTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void WhenAddressesIsNull_ShouldThrow()
            {
                using (var client = new HttpGuardpostClient(MailgunPublicApiKey))
                {
                    client.Parse(null, true);
                }
            }

            [TestMethod]
            public void WhenAddressesIsEmpty_ShouldReturn()
            {
                using (var client = new HttpGuardpostClient(MailgunPublicApiKey))
                {
                    var parseResponse = client.Parse(Enumerable.Empty<string>(), true);

                    Assert.IsNotNull(parseResponse);
                    Assert.IsNotNull(parseResponse.Parsed);
                    Assert.IsNotNull(parseResponse.Unparseable);
                    Assert.IsFalse(parseResponse.Parsed.Any());
                    Assert.IsFalse(parseResponse.Unparseable.Any());
                }
            }

            [TestMethod]
            public void WhenSyntaxOnlyIsTrue()
            {
                using (var client = new HttpGuardpostClient(MailgunPublicApiKey))
                {
                    var parseResponse = client.Parse(new[] { "johnsmith@gmail.com", "john@gmail.com", "gmail.com" }, true);

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
            public void WhenSyntaxOnlyIsFalse()
            {
                using (var client = new HttpGuardpostClient(MailgunPublicApiKey))
                {
                    var parseResponse = client.Parse(new[] { "johnsmith@gmail.com", "john@gmail.com", "gmail.com" }, false);

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
            public void WhenApiKeyIsInvalid_ShouldThrow()
            {
                using (var client = new HttpGuardpostClient("invalidApiKey"))
                {
                    client.Parse(new [] { "john.smith@gmail.com" }, false);
                }
            }
        }
    }
}
