package sample;

import org.apache.http.HttpException;
import org.apache.http.HttpResponse;
import org.apache.http.client.HttpClient;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.client.methods.HttpPut;
import org.apache.http.entity.FileEntity;
import org.apache.http.entity.StringEntity;
import org.apache.http.impl.client.HttpClientBuilder;
import org.apache.http.util.EntityUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestParam;

import javax.json.Json;
import javax.json.JsonObject;
import javax.json.JsonReader;
import java.io.File;
import java.io.IOException;
import java.io.StringReader;
import java.nio.file.Paths;

@Controller
public class IndexController {
    private static final String DOCUMENTS_DIRECTORY = Paths.get(System.getProperty("user.dir"), "Documents").toString();
    private static final String DEFAULT_DOCUMENT = "secret-message.pdf";

    private static final Logger log = LoggerFactory.getLogger(IndexController.class);

    @Value("${prizmdoc.cloud.apiKey:#{null}}")
    private String cloudApiKey;

    @Value("${prizmdoc.pas.baseUrl:#{null}}")
    private String pasBaseUrl;

    @Value("${prizmdoc.pas.secretKey:#{null}}")
    private String pasSecretKey;

    @GetMapping("/")
    public String renderPage(@RequestParam(name="filename", required=false) String filename, Model model) throws IOException, HttpException {
        HttpClient httpClient = HttpClientBuilder.create().build();

        if (filename == null) 
            filename = DEFAULT_DOCUMENT;

        // 1. Create a new viewing session
        HttpPost postRequest = new HttpPost(pasBaseUrl + "ViewingSession");
        JsonObject body = Json.createObjectBuilder()
            .add("source", Json.createObjectBuilder()
                .add("type", "upload")
                .add("displayName", filename)
            ).build();

        if (cloudApiKey != null) {
            postRequest.addHeader("Acs-Api-Key", cloudApiKey);
        }

        postRequest.addHeader("Content-Type", "application/json");
        log.info(body.toString());
        postRequest.setEntity(new StringEntity(body.toString()));

        log.info("Creating viewing session");

        HttpResponse response = httpClient.execute(postRequest);
        if (response.getStatusLine().getStatusCode() != 200) {
            throw new HttpException("POST /ViewingSession HTTP request returned an error: " + response.getStatusLine() + " " + EntityUtils.toString(response.getEntity()));
        }

        String responseJson = EntityUtils.toString(response.getEntity());
        log.debug("Received JSON: {}", responseJson);

        // 2. Send the viewingSessionId and viewer assets to the browser right away so
        // the viewer UI can start loading.
        String viewingSessionId;
        try (JsonReader reader = Json.createReader(new StringReader(responseJson))) {
            viewingSessionId = reader.readObject().getString("viewingSessionId");
            log.info("Received viewingSessionId {}", viewingSessionId);
            model.addAttribute("viewingSessionId", viewingSessionId);
        }

        // 3. Upload the source document to PrizmDoc so that it can start being
        // converted to SVG.
        // The viewer will request this content and receive it automatically once it is
        // ready.
        // We do this part on a background thread so that we don't block the HTML from
        // being
        // sent to the browser.

        class MyRunnable implements Runnable {

            // Allow passing in a filename
            String _filename;
            public MyRunnable(String filename) {
                _filename = filename;
            }
         
            public void run() {
                File document = new File(Paths.get(DOCUMENTS_DIRECTORY, _filename).toString());
                HttpPut putRequest = new HttpPut(pasBaseUrl + "ViewingSession/u" + viewingSessionId + "/SourceFile");
    
                if (cloudApiKey != null) {
                    putRequest.addHeader("Acs-Api-Key", cloudApiKey);
                }
    
                if (pasSecretKey != null) {
                    putRequest.addHeader("Accusoft-Secret", pasSecretKey);
                }
    
                putRequest.addHeader("Content-Type", "application/octet-stream");
                putRequest.setEntity(new FileEntity(document));
    
                try {
                    log.info("Uploading source document");
                    HttpResponse putResponse = httpClient.execute(putRequest);
                    if (putResponse.getStatusLine().getStatusCode() != 200) {
                        throw new HttpException("PUT /SourceFile HTTP request returned an error: " + response.getStatusLine() + " " + EntityUtils.toString(response.getEntity()));
                    }
                } catch (Exception e) {
                    log.error(e.toString());
                }
            }
        }        

        new Thread(new MyRunnable(filename)).start();

        // Render the "index" view, sending the HTML to the browser
        return "index";
    }
}
