#include <stdio.h>
#include <curl/curl.h>

CURLcode curl_download(const char *url, FILE *fp);
