Guardpost.Net
=============

A .NET implementation of Mailgun's [Guardpost API](https://api.mailgun.net/v2/address).

[![Build status](https://ci.appveyor.com/api/projects/status/72m4u80mt587llc2)](https://ci.appveyor.com/project/rbwestmoreland/guardpost-net)

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
  var validateResponse = await guardpost.ValidateAsync("john.smith@gmail.com").ConfigureAwait(false);
}

//parse (syntax only)
using (var guardpost = new HttpGuardpostClient(mailgunPublicApiKey))
{
  var addresses = new [] { "john.smith@gmail.com", "john@gmail.com", "gmail.com" };
  var parseResponse = await guardpost.ParseAsync(addresses, true).ConfigureAwait(false);
}

//parse (syntax + DNS and ESP specific validation as well)
using (var guardpost = new HttpGuardpostClient(mailgunPublicApiKey))
{
  var addresses = new [] { "john.smith@gmail.com", "john@gmail.com", "gmail.com" };
  var parseResponse = await guardpost.ParseAsync(addresses, false).ConfigureAwait(false);
}
``` 

License
---
The MIT License (MIT)

Copyright (c) 2013 Bates Westmoreland 

[Full License](LICENSE)
