# Zero Width Character Message Viewer

Programming contest materials for "Receiving Zero Width Character Message" task.
Based on [hello-prizmdoc-viewer-with-java-and-html]: a minimal Java Spring web application which integrates PrizmDoc Viewer.

## Adding your code

This web application allows you to view a preset document, using PrizmDoc viewer. 

It contains placeholder code that you can fill in to obtain the text from the document and extract the secret message from it:

```
/* --- start logic --- */        

// ...

/* --- end logic --- */
```

## Pre-Requisites

- [JDK 8]
- [Maven] 3.0+

## Running the Sample

To start the application from the command line:

```
mvn spring-boot:run
```

This will launch the message viewer on `http://localhost:8080`, which will show the default `zero-width-message.pdf` document.

Passing a filename as a request parameter allows to load other documents located in Documents folder: `http://localhost:8080?example.pdf`

## How the sample works

This web application creates a "viewing session" on PrizmDoc server, and immediately returns HTML with the viewer
and the viewing session ID. Then it uploads the document to PrizmDoc server, so it can start processing the document.

As soon as the viewer had finished loading in the browser, it starts repeatedly asking PrizmDoc server for the 
document content corresponding to the viewing session ID, and once it is available, displays it.

The sample HTML page also requests additional content from the web application: decoded document text. This
is done by calling http://localhost:8080/decode endpoint, which retrieves document text from PrizmDoc server,
for the same viewing session ID, extracts the secret message from it and returns to the browser.

[JDK 8]: https://www.oracle.com/technetwork/java/javase/downloads/jdk8-downloads-2133151.html
[Maven]: https://maven.apache.org/index.html
[hello-prizmdoc-viewer-with-java-and-html]: https://github.com/Accusoft/hello-prizmdoc-viewer-with-java-and-html