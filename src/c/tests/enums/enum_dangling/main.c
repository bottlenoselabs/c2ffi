#include <stdio.h>
#include "ffi_helper.h"

// NOTE: This enum is not found in any function, variable, or macro object; it's dangling.
enum enum_dangling {
    ENUM_IMPLICIT_VALUE0 = 0,
    ENUM_IMPLICIT_VALUE1 = 255
} enum_dangling;
