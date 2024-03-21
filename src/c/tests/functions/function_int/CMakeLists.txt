cmake_minimum_required(VERSION 3.16)

project(function_int C)
set(CMAKE_C_STANDARD 11)

set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${PROJECT_SOURCE_DIR}/bin)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${PROJECT_SOURCE_DIR}/bin)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${PROJECT_SOURCE_DIR}/bin)

include_directories(function_int PRIVATE ../../../production/ffi_helper/include)

add_library(function_int SHARED ${SOURCE_FILES} main.c)
set_target_properties(function_int PROPERTIES LINKER_LANGUAGE C)
