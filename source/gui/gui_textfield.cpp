/****************************************************************************
 * libwiigui
 *
 * Spotlight 2021
 *
 * gui_textfield.cpp
 *
 * GUI class definitions
 ***************************************************************************/

#include "gui.h"

/**
 * Constructor for the GuiTextField class.
 */

GuiTextField::GuiTextField(wchar_t *content, u32 max) {
    width = 540;
    height = 400;
    selectable = true;
    focus = 0; // allow focus
    alignmentHor = ALIGN_CENTRE;
    alignmentVert = ALIGN_MIDDLE;
    swprintf(value, 255, L"%ls", content);
    max_len = max;

    keyTextbox = new GuiImageData(keyboard_textbox_png);
    keyTextboxImg = new GuiImage(keyTextbox);
    keyTextboxImg->SetAlignment(ALIGN_CENTRE, ALIGN_TOP);
    keyTextboxImg->SetPosition(0, 50);
    this->Append(keyTextboxImg);

    kbText = new GuiText("", 20, (GXColor){0, 0, 0, 0xff});
    kbText->SetWText(value);
    kbText->SetAlignment(ALIGN_CENTRE, ALIGN_TOP);
    kbText->SetPosition(0, 60);
    this->Append(kbText);
}

/**
 * Destructor for the GuiTextField class.
 */
GuiTextField::~GuiTextField() {
    delete kbText;
    delete keyTextbox;
}

wchar_t *GuiTextField::GetText() {
    if (!value) {
        return NULL;
    }

    return wcsdup(value);
}

void GuiTextField::SetText(wchar_t *newText) {
    if (!newText) {
        return;
    }

    swprintf(value, 255, L"%ls", newText);
    kbText->SetWText(value);
}

void GuiTextField::Update(GuiTrigger *t) {
    if (_elements.size() == 0 || (state == STATE_DISABLED && parentElement))
        return;

    for (u8 i = 0; i < _elements.size(); i++) {
        try {
            _elements.at(i)->Update(t);
        } catch (const std::exception &e) {
        }
    }
}
