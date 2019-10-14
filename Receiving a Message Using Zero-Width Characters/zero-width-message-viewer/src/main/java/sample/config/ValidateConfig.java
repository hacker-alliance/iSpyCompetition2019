package sample.config;

import org.apache.http.HttpResponse;
import org.apache.http.client.HttpClient;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.client.methods.HttpPut;
import org.apache.http.entity.StringEntity;
import org.apache.http.impl.client.HttpClientBuilder;
import org.apache.http.util.EntityUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.event.ContextRefreshedEvent;
import org.springframework.context.event.EventListener;
import org.springframework.stereotype.Component;

import javax.json.Json;
import javax.json.JsonObject;
import javax.json.JsonReader;
import java.io.IOException;
import java.io.StringReader;
import java.io.UnsupportedEncodingException;
import java.util.Objects;

/**
 * Validates the configuration to PAS is correct during application startup.
 */
@Component
public class ValidateConfig {
    private static final Logger log = LoggerFactory.getLogger(ValidateConfig.class);

    private static final String DEFAULT_CLOUD_API_KEY = "YOUR_API_KEY";
    private static final String DEFAULT_PAS_BASE_URL = "https://api.accusoft.com/prizmdoc/";
    private static final String DEFAULT_PAS_SECRET_KEY = null;

    @Value("${prizmdoc.cloud.apiKey:#{null}}")
    private String cloudApiKey;

    @Value("${prizmdoc.pas.baseUrl:#{null}}")
    private String pasBaseUrl;

    @Value("${prizmdoc.pas.secretKey:#{null}}")
    private String pasSecretKey;

    @EventListener
    public void onApplicationEvent(ContextRefreshedEvent event) throws UnsupportedEncodingException {
        ensurePasBaseUrlIsNotNull();
        ensurePasBaseUrlEndsWithSlash();
        ensureConfigurationIsNotDefault();

        HttpClient httpClient = HttpClientBuilder.create().build();
        String viewingSessionId = ensureCanPostViewingSession(httpClient);
        ensureCanPutSourceDocument(httpClient, viewingSessionId);

        System.out.println("================================================================================");
        System.out.println("APPLICATION STARTING AT:");
        System.out.println();
        System.out.println("http://localhost:" + 8080);
        System.out.println();
        System.out.println("================================================================================");
    }

    private void ensurePasBaseUrlIsNotNull() {
        if (pasBaseUrl == null) {
            exitWithMessage("Missing required application.properties configuration setting \"prizmdoc.pas.baseUrl\". See the README.md for more information.");
        }
    }

    // The PAS base URL MUST end with a trailing slash in order for the embedded Zuul reverse proxy to work correctly.
    private void ensurePasBaseUrlEndsWithSlash() {
        if (!pasBaseUrl.endsWith("/")) {
            exitWithMessage("The PAS base URL (\"prizmdoc.pas.baseUrl\" in application.properties) must end with a trailing slash.");
        }
    }

    private void ensureConfigurationIsNotDefault() {
        boolean configurationHasNotYetBeenSet =
                Objects.equals(DEFAULT_PAS_BASE_URL, pasBaseUrl) &&
                        Objects.equals(DEFAULT_CLOUD_API_KEY, cloudApiKey) &&
                        Objects.equals(DEFAULT_PAS_SECRET_KEY, pasSecretKey);

        if (configurationHasNotYetBeenSet) {
            exitWithMessage("It looks like you have not yet configured your connection to PAS (PrizmDoc Application Services). See the README.md for more information.");
        }
    }

    private String ensureCanPostViewingSession(HttpClient httpClient) throws UnsupportedEncodingException {
        HttpResponse response;

        // Test POST /ViewingSession to see whether the configuration will work or not
        String postUrl = pasBaseUrl + "ViewingSession";
        HttpPost postRequest = new HttpPost(postUrl);
        JsonObject body = Json.createObjectBuilder()
                .add("source", Json.createObjectBuilder()
                        .add("type", "upload")
                        .add("displayName", "test")
                ).build();
        if (cloudApiKey != null) {
            postRequest.addHeader("Acs-Api-Key", cloudApiKey);
        }
        postRequest.addHeader("Content-Type", "application/json");
        postRequest.setEntity(new StringEntity(body.toString()));
        int statusCode = 0;
        String responseJson = "";
        try {
            response = httpClient.execute(postRequest);
            statusCode = response.getStatusLine().getStatusCode();
            responseJson = EntityUtils.toString(response.getEntity());
        } catch (IOException ignored) {
        }

        String errorCode = "";
        if (statusCode != 200) {
            try (JsonReader reader = Json.createReader(new StringReader(responseJson))) {
                errorCode = reader.readObject().getString("errorCode");
            } catch (Exception e) {
                // Ignore JSON parsing errors. errorCode will remain empty string.
            }
        }

        if (statusCode == 401 /* Unauthorized */ && errorCode.equals("Unauthorized")) {
            if (cloudApiKey != null) {
                exitWithMessage("Invalid API key. Make sure your \"prizmdoc.cloud.apiKey\" value in application.properties is correct. " +
                        "See the README.md for more information.");
            } else {
                exitWithMessage("Missing API key. When using PrizmDoc Cloud, make sure you have provided a \"prizmdoc.cloud.apiKey\" value in application.properties. " +
                        "Visit https://cloud.accusoft.com to sign up for an account and get an API key at no cost. " +
                        "See the README.md for more information.");
            }
        }

        if (statusCode != 200) {
            exitWithMessage("Unexpected response when testing the connection to PAS. Are you sure that \"prizmdoc.pas.baseUrl\" in application.properties is correct? " +
                    "See the README.md for more information.");
        }

        String viewingSessionId;
        try (JsonReader reader = Json.createReader(new StringReader(responseJson))) {
            viewingSessionId = reader.readObject().getString("viewingSessionId");
        }

        return viewingSessionId;
    }

    private void ensureCanPutSourceDocument(HttpClient httpClient, String viewingSessionId) throws UnsupportedEncodingException {
        HttpResponse response;

        // Test PUT /SourceFile, for the self-hosted case, to make sure the secret key will work
        String url = pasBaseUrl + "ViewingSession/u" + viewingSessionId + "/SourceFile?FileExtension=txt";
        HttpPut request = new HttpPut(url);

        if (cloudApiKey != null) {
            request.addHeader("Acs-Api-Key", cloudApiKey);
        }

        if (pasSecretKey != null) {
            request.addHeader("Accusoft-Secret", pasSecretKey);
        }

        request.addHeader("Content-Type", "application/octet-stream");
        request.setEntity(new StringEntity("test"));

        int statusCode = 0;
        String errorCode = "";
        try {
            response = httpClient.execute(request);
            statusCode = response.getStatusLine().getStatusCode();

            if (statusCode != 200) {
                String responseJson = EntityUtils.toString(response.getEntity());
                try (JsonReader reader = Json.createReader(new StringReader(responseJson))) {
                    errorCode = reader.readObject().getString("errorCode");
                } catch (Exception e) {
                    // Ignore JSON parsing errors. errorCode will remain empty string.
                }
            }
        } catch (IOException ignored) {
        }

        if (statusCode == 403 /* Forbidden */ && errorCode.equals("InvalidSecret")) {
            if (pasSecretKey != null) {
                exitWithMessage("Invalid PAS secret. Make sure your \"prizmdoc.pas.secretKey\" value in application.properties matches the \"secretKey\" value in your PAS config file. " +
                        "See the README.md for more information.");
            } else {
                exitWithMessage("You appear to be using a self-hosted PAS instance, but you have not provided a \"prizmdoc.pas.secretKey\" value in application.properties. " +
                        "When self-hosting PAS, certain requests require an \"Accusoft-Secret\" header in order to be accepted. " +
                        "To ensure this header is correctly sent, make sure to provide a \"prizmdoc.pas.secretKey\" configuration value in your application.properties file, " +
                        "and make sure the value matches the \"secretKey\" value in your PAS config file. " +
                        "See the README.md for more information.");
            }
        }

        if (statusCode != 200) {
            exitWithMessage("Unexpected response when testing the connection to PAS. " +
                    "Have you configured the connection to PAS (PrizmDoc Application Services) correctly? " +
                    "See the README.md for more information.");
        }
    }

    private void exitWithMessage(String message) {
        // Log the error
        log.error(message);

        // Wrap the message to lines no longer than 80 characters for easier reading.
        String[] tokens = message.split("\\s+");
        StringBuilder line = new StringBuilder();
        StringBuilder wrappedMessage = new StringBuilder();

        for (String token : tokens) {
            if (line.length() + token.length() > 80) {
                wrappedMessage.append(line.toString());
                wrappedMessage.append(String.format("%n"));
                line = new StringBuilder();
            }
            line.append(token);
            line.append(" ");
        }

        if (line.length() > 0) {
            wrappedMessage.append(line.toString());
        }

        // Print a human readable final message before exiting.
        System.err.println();
        System.err.println("================================================================================");
        System.err.println("CONFIGURATION ERROR:");
        System.err.println();
        System.err.println(wrappedMessage);
        System.err.println("================================================================================");
        System.err.println();

        // Exit with error
        System.exit(1);
    }
}
