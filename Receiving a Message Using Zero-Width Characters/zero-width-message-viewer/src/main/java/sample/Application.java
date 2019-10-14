package sample;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.cloud.netflix.zuul.EnableZuulProxy;
import org.springframework.context.annotation.Bean;
import sample.pasProxy.AddApiKeyRequestHeaderFilter;

// We use Netflix Zuul to setup an embedded reverse proxy to PAS.
// This allows the viewer to make requests to PAS through this web application.
//
// The reverse proxy route which the viewer will use is configured in application.properties.
@EnableZuulProxy
@SpringBootApplication
public class Application {
    public static void main(String[] args) {
        SpringApplication.run(Application.class, args);
    }

    // Setup a Zuul pre-filter which ensures that, if a PrizmDoc Cloud API key is set,
    // all requests proxied to PAS have the API key injected as a request header.
    // See AddApiKeyRequestHeaderFilter.java.
    @Bean
    public AddApiKeyRequestHeaderFilter addApiKeyRequestHeaderFilter() {
        return new AddApiKeyRequestHeaderFilter();
    }
}
