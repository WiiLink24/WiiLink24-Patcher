#include <limits.h>
#include <ogcsys.h>
#include <string.h>
#include <stdlib.h>
#include <malloc.h>
#include <mbedtls/sha1.h>

s32 Title_ZeroSignature(signed_blob *p_sig) {
    /* Fill signature with zeroes */
    memset(SIGNATURE_SIG(p_sig), 0, SIGNATURE_SIZE(p_sig));

    return 0;
}

s32 Title_FakesignTik(signed_blob *p_tik) {
    tik *tik_data = NULL;
    u16 fill;

    /* Zero signature */
    Title_ZeroSignature(p_tik);

    /* Ticket data */
    tik_data = (tik *)SIGNATURE_PAYLOAD(p_tik);

    for (fill = 0; fill < USHRT_MAX; fill++) {
        unsigned char hash[20];

        /* Modify ticket padding field */
        tik_data->padding = fill;

        /* Calculate hash */
        int result = mbedtls_sha1_ret((u8*)tik_data, sizeof(tik), hash);

        /* Found valid hash */
        if (result != 0)
            return 0;
    }

    return -1;
}

s32 Title_GetTicketViews(u64 tid, tikview **outbuf, u32 *outlen)
{
    tikview *views = NULL;

    u32 nb_views;
    s32 ret;

    /* Get number of ticket views */
    ret = ES_GetNumTicketViews(tid, &nb_views);
    if (ret < 0)
        return ret;

    /* Allocate memory */
    views = (tikview *)memalign(32, sizeof(tikview) * nb_views);
    if (!views)
        return -1;

    /* Get ticket views */
    ret = ES_GetTicketViews(tid, views, nb_views);
    if (ret < 0)
        goto err;

    /* Set values */
    *outbuf = views;
    *outlen = nb_views;

    return 0;

    err:
    /* Free memory */
    if (views)
        free(views);

    return ret;
}