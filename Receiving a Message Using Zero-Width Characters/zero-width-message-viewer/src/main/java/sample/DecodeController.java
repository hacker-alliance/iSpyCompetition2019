package sample;

import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.beans.factory.annotation.Value;

import org.apache.http.HttpException;
import org.apache.http.HttpResponse;
import org.apache.http.client.HttpClient;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.impl.client.HttpClientBuilder;
import org.apache.http.util.EntityUtils;

import java.io.IOException;
import java.io.StringReader;
import java.io.UnsupportedEncodingException;
import java.util.ArrayList;

import javax.json.Json;
import javax.json.JsonArray;
import javax.json.JsonObject;
import javax.json.JsonString;
import javax.json.JsonReader;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

@Controller
public class DecodeController {

    private static final Logger log = LoggerFactory.getLogger(DecodeController.class);

    @Value("${prizmdoc.cloud.apiKey:#{null}}")
    private String cloudApiKey;

    @Value("${prizmdoc.pas.baseUrl:#{null}}")
    private String pasBaseUrl;

    @Value("${prizmdoc.pas.secretKey:#{null}}")
    private String pasSecretKey;


    public static String decodeSecretMessage(String prizmDocResponse) throws UnsupportedEncodingException {

        final char zeroWidthNonJoiner = '\u200C';
        final char zeroWidthJoiner = '\u200D';

        String decodedText;

        /* --- start logic --- */        
        /* --- Hint: https://help.accusoft.com/PrizmDoc/v13.8/HTML/webframe.html#html5-viewing.html --- */
        /* --- Hint: https://docs.oracle.com/javaee/7/api/javax/json/JsonReader.html --- */
        StringBuilder sb = new  StringBuilder();
        for (char c : prizmDocResponse.toCharArray()) {
            if (c == zeroWidthNonJoiner || c == zeroWidthJoiner)
            {
                sb.append(c);
            }
        }
        String encoded = sb.toString();
        StringBuilder sb2 = new StringBuilder();
        byte b = 0;
        for (int i = 0; i < encoded.length(); i += 8) {
            for (int j = 0; j < 8; ++j)
            {
                if (encoded.charAt(i + j) == zeroWidthJoiner)
                {
                    b |= 1 << 7 - j;
                }
                else {
                    b &= ~(1 << 7 - j);
                }
                    
            }
            sb2.append((char) b);
        } 

        decodedText = " -------------------------- Your implementation here --------------------------";
        /* --- end logic --- */
        decodedText = sb2.toString();
        return decodedText;
    }

    @GetMapping("/decode")
    public String decode(@RequestParam(name="viewingSessionId", required=true) String viewingSessionId, Model model) throws IOException, HttpException{

        HttpClient httpClient = HttpClientBuilder.create().build();

        // Get document text for the first page 
        HttpGet getRequest = new HttpGet(pasBaseUrl + "/Document/q/0-0/Text?DocumentID=u" + viewingSessionId);
        getRequest.addHeader("Accept-Encoding", "gzip");
        if (cloudApiKey != null) {
            getRequest.addHeader("Acs-Api-Key", cloudApiKey);
        }

        if (pasSecretKey != null) {
            getRequest.addHeader("Accusoft-Secret", pasSecretKey);
        }

        String decodedText = "";
        String decodeStatus;

        try {
            log.info("Getting document text");
            HttpResponse response = httpClient.execute(getRequest);
            if (response.getStatusLine().getStatusCode() != 200) {
                throw new HttpException("GET /Document/q/.../Text HTTP request returned an error: " + response.getStatusLine() + " " + EntityUtils.toString(response.getEntity()));
            }
    
            String responseJson = EntityUtils.toString(response.getEntity());
            log.debug("Received JSON: {}", responseJson);
    
            decodedText = decodeSecretMessage(responseJson);
            decodeStatus = decodedText.length() > 0 ? "Success" : "No secret message found";
        }
        catch (Exception e) {
            decodedText = "";
            decodeStatus = "Could not decode message: " + e.getMessage();
        }

        model.addAttribute("decodeStatus", decodeStatus);
        model.addAttribute("decodedText", decodedText);
        return "decode";
    }

}