package sample.pasProxy;

import javax.servlet.http.HttpServletRequest;
import com.netflix.zuul.context.RequestContext;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;

import com.netflix.zuul.ZuulFilter;

/**
 * Ensures that the PrizmDoc Cloud API key, if defined, is injected
 * as a request header before proxied requests are sent to PAS.
 *
 * We use @Component to automatically register this bean during Spring application startup.
 * Netflix Zuul will execute this "pre" filter before sending the request on to PAS, allowing
 * this class to inject the configured PrizmDoc Cloud API key.
 *
 * See https://spring.io/guides/gs/routing-and-filtering/
 */
public class AddApiKeyRequestHeaderFilter extends ZuulFilter {

  private static final Logger log = LoggerFactory.getLogger(AddApiKeyRequestHeaderFilter.class);

  @Value("${prizmdoc.cloud.apiKey:#{null}}")
  private String apiKey;

  @Override
  public String filterType() {
    return "pre";
  }

  @Override
  public int filterOrder() {
    return 0;
  }

  @Override
  public boolean shouldFilter() {
    return true;
  }

  @Override
  public Object run() {
    RequestContext ctx = RequestContext.getCurrentContext();
    HttpServletRequest request = ctx.getRequest();

    if (apiKey != null) {
      ctx.addZuulRequestHeader("Acs-Api-Key", apiKey);
    }

    log.info("Proxying {} {}", request.getMethod(), request.getRequestURL());

    return null;
  }

}
