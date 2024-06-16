# Mini HTTP Server

Based on [PracticeBareboneHTTPServer](https://github.com/Charles-Zhang-CSharp/PracticeBareboneHTTPServer), this minimal HTTP server intends to build a feature-complete HTTP(s) server with minimal dependencies for the purpose of conceptual demonstration. ASP.Net core and Kestrel are good but they are too bulky for simple stuff, e.g. [StaticWebpagesServer](https://github.com/chaojian-zhang/StaticWebpagesServer). We intend to make everything compilation to ATO.

As it turns out, getting started is very easy, and wandering to mild distance is not too hard.

## TODO

- [x] Add publishing script; Done, see [commit](https://github.com/chaojian-zhang/MiniHTTPServer/commit/81485ff5e90e888ab5556c364ce007154d115257).
- [x] Make sure replies can be understood by browser and rendered as HTML; Done, see [commit](https://github.com/chaojian-zhang/MiniHTTPServer/commit/d6ada1d044fa4e4bb08c1ceb11bc627c7ee6b852).
- [ ] Static file hosting
- [ ] Make a YouTube video: How to Write Your Own HTTP Server
- [ ] HTTPs support
- [ ] Preliminary (embedded) scripting (e.g. Lua)

## References and Excerpts

Quick references:

* TcpListener Class: https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcplistener?view=net-8.0
* https://github.com/Charles-Zhang-CSharp/PracticeBareboneHTTPServer
* https://github.com/chaojian-zhang/StaticWebpagesServer

Technical readings:

* Communication Networks/HTTP Protocol: https://en.wikibooks.org/wiki/Communication_Networks/HTTP_Protocol
* The C10K problem: http://www.kegel.com/c10k.html. Talks about performance; Looks long, but is pretty straightforward to read if one knows most of the stuff already and have experience with the many modes of programming mentioned. Very succinct in conclusions, likes how it's very relevant to how OS/hardware works.
* https://softwareengineering.stackexchange.com/questions/200821/how-to-write-a-http-server
* https://beej.us/guide/bgnet/html/