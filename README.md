Guardpost.Net
=============

A .NET implementation of Mailgun's [Guardpost API](https://api.mailgun.net/v2/address).

Installation
---  
Just download and add [Guardpost.Net.cs](src/Guardpost.Net/Guardpost.Net.cs) to your project.

* Requires .NET 4.0 or later
* Requires Json.Net 4.0.1 or later

Usage
---  

```csharp
//API Key in the My Account tab of your Mailgun account (the one with the “pubkey” prefix).
var mailgunPublicApiKey = "pubkey-1234567890abcdefghijklmnopqrstuvwxyz";

//validate
using (var guardpost = new HttpGuardpostClient(mailgunPublicApiKey))
{
  var validateResponse = guardpost.Validate("john.smith@gmail.com");
}

//parse (syntax only)
using (var guardpost = new HttpGuardpostClient(mailgunPublicApiKey))
{
  var addresses = new [] { "john.smith@gmail.com", "john@gmail.com", "gmail.com" };
  var parseResponse = guardpost.Parse(addresses, true);
}

//parse (syntax + DNS and ESP specific validation as well)
using (var guardpost = new HttpGuardpostClient(mailgunPublicApiKey))
{
  var addresses = new [] { "john.smith@gmail.com", "john@gmail.com", "gmail.com" };
  var parseResponse = guardpost.Parse(addresses, false);
}
``` 

License
---
The MIT License (MIT)

Copyright (c) 2013 Bates Westmoreland 

[Full License](LICENSE)
