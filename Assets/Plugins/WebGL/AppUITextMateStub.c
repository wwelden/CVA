// No-op stand-ins for App UI's TextMate syntax-highlighting library on WebGL.
//
// App UI (a dependency of the editor-only AI Assistant package) P/Invokes these
// via "__Internal" on WebGL, but its prebuilt libAppUITextMateLib.a was compiled
// against a different Emscripten and fails to link. The game never uses syntax
// highlighting, so WebGLBuild.cs excludes that archive and these stubs satisfy
// the linker instead. Every call reports "nothing loaded".
#include <stddef.h>
#include <stdint.h>

void* textmate_theme_load_from_file(const char* themePath) { return NULL; }
void* textmate_theme_load_from_json(const uint8_t* json) { return NULL; }
uint32_t textmate_theme_get_foreground(void* theme, const char* scope, uint32_t def) { return def; }
uint32_t textmate_theme_get_background(void* theme, const char* scope, uint32_t def) { return def; }
int32_t textmate_theme_get_font_style(void* theme, const char* scope, int32_t def) { return def; }
uint32_t textmate_theme_get_default_foreground(void* theme) { return 0; }
uint32_t textmate_theme_get_default_background(void* theme) { return 0; }
void textmate_theme_dispose(void* theme) {}

void* textmate_oniglib_create(void) { return NULL; }
void textmate_oniglib_dispose(void* onigLib) {}
void* textmate_registry_create(void* onigLib) { return NULL; }
void textmate_registry_dispose(void* registry) {}
int32_t textmate_registry_add_grammar_from_file(void* registry, const char* path) { return 0; }
int32_t textmate_registry_add_grammar_from_json(void* registry, const uint8_t* json) { return 0; }
void* textmate_registry_load_grammar(void* registry, const char* scopeName) { return NULL; }
int32_t textmate_registry_set_theme(void* registry, const uint8_t* json) { return 0; }
void* textmate_registry_get_color_map(void* registry) { return NULL; }

void* textmate_get_initial_state(void) { return NULL; }
void* textmate_tokenize_line(void* grammar, const char* line, void* prev) { return NULL; }
void* textmate_tokenize_line2(void* grammar, const char* line, void* prev) { return NULL; }
void* textmate_tokenize_line_utf16(void* grammar, const uint8_t* line, void* prev) { return NULL; }
void* textmate_tokenize_line2_utf16(void* grammar, const uint8_t* line, void* prev) { return NULL; }
void* textmate_tokenize_lines_utf16(void* grammar, void* lines, int32_t count, void* initial) { return NULL; }
void* textmate_tokenize_lines2_utf16(void* grammar, void* lines, int32_t count, void* initial) { return NULL; }
void textmate_free_tokenize_result(void* result) {}
void textmate_free_tokenize_result2(void* result) {}
void textmate_free_tokenize_lines_result(void* result) {}
void textmate_free_tokenize_lines_result2(void* result) {}
void textmate_free_color_map(void* colorMap) {}
void* textmate_grammar_get_scope_name(void* grammar) { return NULL; }
