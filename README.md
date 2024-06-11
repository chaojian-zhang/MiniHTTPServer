# Mini HTTP Server

Based on [PracticeBareboneHTTPServer](https://github.com/Charles-Zhang-CSharp/PracticeBareboneHTTPServer), this minimal HTTP server intends to build a feature-complete HTTP(s) server with minimal dependencies for the purpose of conceptual demonstration. ASP.Net core and Kestrel are good but they are too bulky for simple stuff, e.g. [StaticWebpagesServer](https://github.com/chaojian-zhang/StaticWebpagesServer). We intend to make everything compilation to ATO.

As it turns out, getting started is very easy, and wandering to mild distance is not too hard.

## TODO

- [ ] Proper browser handleable replies
- [ ] Static file hosting
- [ ] HTTPs support
- [ ] Preliminary (embedded) scripting (e.g. Lua)

## References

* https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcplistener?view=net-8.0
* https://github.com/Charles-Zhang-CSharp/PracticeBareboneHTTPServer
* https://github.com/chaojian-zhang/StaticWebpagesServer