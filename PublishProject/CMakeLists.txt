cmake_minimum_required(VERSION 3.21)
project(OccViewer)
set(CMAKE_CXX_STANDARD 20)

add_compile_options("$<$<C_COMPILER_ID:MSVC>:/utf-8>")
add_compile_options("$<$<CXX_COMPILER_ID:MSVC>:/utf-8>")

find_package(OpenCASCADE CONFIG REQUIRED PATHS ${OpenCASCADE_DIR})

set(OCCVIEWER_BIN_DIR "${CMAKE_CURRENT_LIST_DIR}/bin")
set(OCCVIEWER_LIB_DIR "${CMAKE_CURRENT_LIST_DIR}/lib")
set(OCCVIEWER_INC_DIR "${CMAKE_CURRENT_LIST_DIR}/include")

add_library(${PROJECT_NAME} INTERFACE)
target_include_directories(${PROJECT_NAME} INTERFACE ${OCCVIEWER_INC_DIR})
target_link_directories(${PROJECT_NAME} INTERFACE ${OCCVIEWER_LIB_DIR})
target_link_libraries(${PROJECT_NAME} INTERFACE ${OpenCASCADE_LIBRARIES})
target_link_libraries(${PROJECT_NAME} INTERFACE "OccViewer.ViewerProxy.lib")

install(DIRECTORY "${OCCVIEWER_BIN_DIR}/" TYPE BIN)
install(DIRECTORY "${OpenCASCADE_BINARY_DIR}$<$<CONFIG:Debug>:d>$<$<CONFIG:Release>:i>/" TYPE BIN)
