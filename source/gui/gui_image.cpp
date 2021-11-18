/****************************************************************************
 * libwiigui
 *
 * Tantric 2009
 *
 * gui_image.cpp
 *
 * GUI class definitions
 ***************************************************************************/

#include "gui.h"

/**
 * Constructor for the GuiImage class.
 */
GuiImage::GuiImage() {
    image = NULL;
    width = 0;
    height = 0;
    imageangle = 0;
    tile = -1;
    imgType = IMAGE_DATA;
}

GuiImage::GuiImage(GuiImageData *img) {
    image = NULL;
    width = 0;
    height = 0;
    if (img) {
        image = img->GetImage();
        width = img->GetWidth();
        height = img->GetHeight();
    }
    imageangle = 0;
    tile = -1;
    imgType = IMAGE_DATA;
}

GuiImage::GuiImage(u8 *img, int w, int h) {
    image = img;
    width = w;
    height = h;
    imageangle = 0;
    tile = -1;
    imgType = IMAGE_TEXTURE;
}

GuiImage::GuiImage(int w, int h, GXColor c) {
    image = (u8 *)memalign(32, w * h << 2);
    width = w;
    height = h;
    imageangle = 0;
    tile = -1;
    imgType = IMAGE_COLOR;

    if (!image)
        return;

    int x, y;

    for (y = 0; y < h; ++y) {
        for (x = 0; x < w; ++x) {
            this->SetPixel(x, y, c);
        }
    }
    int len = w * h << 2;
    if (len % 32)
        len += (32 - len % 32);
    DCFlushRange(image, len);
}

/**
 * Destructor for the GuiImage class.
 */
GuiImage::~GuiImage() {
    if (imgType == IMAGE_COLOR && image)
        free(image);
}

u8 *GuiImage::GetImage() { return image; }

void GuiImage::SetImage(GuiImageData *img) {
    image = NULL;
    width = 0;
    height = 0;
    if (img) {
        image = img->GetImage();
        width = img->GetWidth();
        height = img->GetHeight();
    }
    imgType = IMAGE_DATA;
}

void GuiImage::SetImage(u8 *img, int w, int h) {
    image = img;
    width = w;
    height = h;
    imgType = IMAGE_TEXTURE;
}

void GuiImage::SetAngle(float a) { imageangle = a; }

void GuiImage::SetTile(int t) { tile = t; }

GXColor GuiImage::GetPixel(int x, int y) {
    if (!image || this->GetWidth() <= 0 || x < 0 || y < 0)
        return (GXColor){0, 0, 0, 0};

    u32 offset = (((y >> 2) << 4) * this->GetWidth()) + ((x >> 2) << 6) +
                 (((y % 4 << 2) + x % 4) << 1);
    GXColor color;
    color.a = *(image + offset);
    color.r = *(image + offset + 1);
    color.g = *(image + offset + 32);
    color.b = *(image + offset + 33);
    return color;
}

void GuiImage::SetPixel(int x, int y, GXColor color) {
    if (!image || this->GetWidth() <= 0 || x < 0 || y < 0)
        return;

    u32 offset = (((y >> 2) << 4) * this->GetWidth()) + ((x >> 2) << 6) +
                 (((y % 4 << 2) + x % 4) << 1);
    *(image + offset) = color.a;
    *(image + offset + 1) = color.r;
    *(image + offset + 32) = color.g;
    *(image + offset + 33) = color.b;
}

void GuiImage::ColorStripe(int y, GXColor color) {
    int x = 0;
    int thisWidth = this->GetWidth();

    for (; x < thisWidth; ++x) {
        SetPixel(x, y, color);
    }
}

/**
 * Draw the button on screen
 */
void GuiImage::Draw() {
    if (!image || !this->IsVisible() || tile == 0)
        return;

    float currScaleX = this->GetScaleX();
    float currScaleY = this->GetScaleY();
    int currLeft = this->GetLeft();
    int thisTop = this->GetTop();

    if (tile > 0) {
        int alpha = this->GetAlpha();
        for (int i = 0; i < tile; ++i) {
            Menu_DrawImg(currLeft + width * i, thisTop, width, height, image,
                         imageangle, currScaleX, currScaleY, alpha);
        }
    } else {
        Menu_DrawImg(currLeft, thisTop, width, height, image, imageangle,
                     currScaleX, currScaleY, this->GetAlpha());
    }

    this->UpdateEffects();
}
