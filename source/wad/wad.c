#include <limits.h>
#include <malloc.h>
#include <ogc/pad.h>
#include <ogcsys.h>
#include <stdio.h>
#include <string.h>

#include "title.h"

#define TITLE_ID(x, y) (((u64)(x) << 32) | (y))
#define BLOCK_SIZE 1024
#define round_up(x, n) (-(-(x) & -(n)))

// WAD Header Struct
typedef struct {
    // Header length
    u32 header_len;

    // WAD type
    u16 type;

    u16 padding;

    // Data length
    u32 certs_len;
    u32 crl_len;
    u32 tik_len;
    u32 tmd_len;
    u32 data_len;
    u32 footer_len;
} ATTRIBUTE_PACKED wadHeader;

// Set WAD buffer
static u8 wadBuffer[BLOCK_SIZE] ATTRIBUTE_ALIGN(32);

s32 Wad_ReadFile(FILE *fp, void *outbuf, u32 offset, u32 len) {
    s32 ret;

    // Move file pointer to offset
    fseek(fp, offset, SEEK_SET);

    // Read data
    ret = fread(outbuf, len, 1, fp);
    if (ret < 0)
        return ret;

    return 0;
}

s32 Wad_ReadAlloc(FILE *fp, void **outbuf, u32 offset, u32 len) {
    void *buffer = NULL;
    s32 ret;

    // Allocate memory
    buffer = memalign(32, len);
    if (!buffer)
        return -1;

    // Read file
    ret = Wad_ReadFile(fp, buffer, offset, len);
    if (ret < 0) {
        free(buffer);
        return ret;
    }

    // Set pointer
    *outbuf = buffer;

    return 0;
}

void Wad_FixTicket(signed_blob *p_tik) {
    u8 *data = (u8 *)p_tik;
    u8 *common_key = data + 0x1F1;

    // Check common key
    if (*common_key > 1)
        *common_key = 0;

    // Fakesign ticket
    Title_FakesignTik(p_tik);
}

s32 Wad_GetTitleID(FILE *fp, wadHeader *header, u64 *tid)
{
    signed_blob *p_tik    = NULL;
    tik         *tik_data = NULL;

    u32 offset = 0;
    s32 ret;

    /* Ticket offset */
    offset += round_up(header->header_len, 64);
    offset += round_up(header->certs_len,  64);
    offset += round_up(header->crl_len,    64);

    /* Read ticket */
    ret = Wad_ReadAlloc(fp, (void *)&p_tik, offset, header->tik_len);
    if (ret < 0)
        goto out;

    /* Ticket data */
    tik_data = (tik *)SIGNATURE_PAYLOAD(p_tik);

    /* Copy title ID */
    *tid = tik_data->titleid;

    out:
    /* Free memory */
    if (p_tik)
        free(p_tik);

    return ret;
}

// The install WAD function in its entirety
int install_WAD(FILE *fp) {
    wadHeader *header = NULL;
    signed_blob *p_certs = NULL, *p_crl = NULL, *p_tik = NULL, *p_tmd = NULL;

    tmd *tmd_data = NULL;

    u32 cnt, offset = 0;
    s32 ret;

    ret = Wad_ReadAlloc(fp, (void *)&header, offset, sizeof(wadHeader));
    if (ret >= 0)
        offset += round_up(header->header_len, 64);
    else
        goto err;

    // WAD certificates
    ret = Wad_ReadAlloc(fp, (void *)&p_certs, offset, header->certs_len);
    if (ret >= 0)
        offset += round_up(header->certs_len, 64);
    else
        goto err;

    // WAD crl
    if (header->crl_len) {
        ret = Wad_ReadAlloc(fp, (void *)&p_crl, offset, header->crl_len);
        if (ret < 0)
            goto err;
        else
            offset += round_up(header->crl_len, 64);
    }

    // WAD ticket
    ret = Wad_ReadAlloc(fp, (void *)&p_tik, offset, header->tik_len);
    if (ret < 0)
        goto err;
    else
        offset += round_up(header->tik_len, 64);

    // WAD TMD
    ret = Wad_ReadAlloc(fp, (void *)&p_tmd, offset, header->tmd_len);
    if (ret < 0)
        goto err;
    else
        offset += round_up(header->tmd_len, 64);

    // Get TMD data
    tmd_data = (tmd *)SIGNATURE_PAYLOAD(p_tmd);

    // Fakesign ticket
    Wad_FixTicket(p_tik);

    // Install ticket
    ret = ES_AddTicket(p_tik, header->tik_len, p_certs, header->certs_len,
                       p_crl, header->crl_len);
    if (ret < 0)
        goto err;

    // Install title
    ret = ES_AddTitleStart(p_tmd, header->tmd_len, p_certs, header->certs_len,
                           p_crl, header->crl_len);
    if (ret < 0)
        goto err;

    // Install contents to NAND
    for (cnt = 0; cnt < tmd_data->num_contents; cnt++) {
        tmd_content *content = &tmd_data->contents[cnt];

        u32 idx = 0, len;
        s32 cfd;

        // Encrypted content size
        len = round_up(content->size, 64);

        // Install content
        cfd = ES_AddContentStart(tmd_data->title_id, content->cid);
        if (cfd < 0) {
            ret = cfd;
            goto err;
        }

        // Install content data
        while (idx < len) {
            u32 size;

            // Data length
            size = (len - idx);
            if (size > BLOCK_SIZE)
                size = BLOCK_SIZE;

            // Read data
            ret = Wad_ReadFile(fp, &wadBuffer, offset, size);
            if (ret < 0)
                goto err;

            // Install data
            ret = ES_AddContentData(cfd, wadBuffer, size);
            if (ret < 0)
                goto err;

            // Increase variables
            idx += size;
            offset += size;
        }

        // Finish content installation
        ret = ES_AddContentFinish(cfd);
        if (ret < 0)
            goto err;
    }

    // Finish title install
    ret = ES_AddTitleFinish();
    if (ret >= 0) {
        goto out;
    }

    err:
        // Cancel install
        ES_AddTitleCancel();
        return ret;

    out:
    // Free memory
    if (header)
        free(header);
    if (p_certs)
        free(p_certs);
    if (p_crl)
        free(p_crl);
    if (p_tik)
        free(p_tik);
    if (p_tmd)
        free(p_tmd);

    return ret;
}

int Wad_Uninstall(FILE *fp) {
    wadHeader *header   = NULL;
    tikview   *viewData = NULL;

    u64 tid;
    u32 viewCnt;
    int ret;

    /* WAD header */
    ret = Wad_ReadAlloc(fp, (void *)&header, 0, sizeof(wadHeader));
    if (ret < 0) {
        goto out;
    }

    /* Get title ID */
    ret =  Wad_GetTitleID(fp, header, &tid);
    if (ret < 0) {
        goto out;
    }

    /* Get ticket views */
    ret = Title_GetTicketViews(tid, &viewData, &viewCnt);

    /* Delete tickets */
    if (ret >= 0) {
        u32 cnt;

        /* Delete all tickets */
        for (cnt = 0; cnt < viewCnt; cnt++) {
            ret = ES_DeleteTicket(&viewData[cnt]);
            if (ret < 0)
                break;
        }
    }

    /* Delete title contents */
    ret = ES_DeleteTitleContent(tid);

    /* Delete title */
    ret = ES_DeleteTitle(tid);

    out:
    /* Free memory */
    if (header)
        free(header);
    return ret;
}