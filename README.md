# NedoGram
TCP messenger

Hi there ;)

This is my implementation of the chating app.

It has "server and clients" architecture. 

As it is in the implementation phase at the moment,
Running the app is now available only with using of the Visual Studio and on the local machine.

If you want to use my app, you should:
  1. Open .sln file in the Visual Studio
  2. Run the server project
  3. Run as many client projects as you need
 
For now, only CLI client is available.
 
I used:
- .Net Core 3.0, .Net Standard 2.1
- TCP protocol for a communication between a client and the server
- AES and RSA for an encryption
- JSON API from the .Net Core for a message transformation to the JSON format
