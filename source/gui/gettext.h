#ifndef _GETTEXT_H_
#define _GETTEXT_H_

#include <stdio.h>

bool LoadLanguage(char *language, size_t lang_size);
bool text_language();

/*
 * input msg = a text in ASCII
 * output = the translated msg in utf-8
 */
const char *gettext(const char *msg);

#endif /* _GETTEXT_H_ */
