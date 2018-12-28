# CMakeList.txt : CMake project for CrossPlatformTest, include source and define
# project specific logic here.
#
cmake_minimum_required (VERSION 3.8)

# Add source to this project's executable.
add_executable (CrossPlatformTest "CrossPlatformTest.cpp" "CrossPlatformTest.h")

target_link_libraries(CrossPlatformTest
	${ALL_LIBS}
)
# Xcode and Visual working directories
set_target_properties(CrossPlatformTest PROPERTIES XCODE_ATTRIBUTE_CONFIGURATION_BUILD_DIR "${CMAKE_CURRENT_SOURCE_DIR}/CrossPlatformTest/")
create_target_launcher(CrossPlatformTest WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/CrossPlatformTest/")
create_default_target_launcher(CrossPlatformTest WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/CrossPlatformTest/") # tut 1 is not the default or people would complain that tut 2 doesn't work

SOURCE_GROUP(common REGULAR_EXPRESSION ".*/common/.*" )
SOURCE_GROUP(shaders REGULAR_EXPRESSION ".*/.*shader$" )

if (NOT ${CMAKE_GENERATOR} MATCHES "Xcode" )
add_custom_command(
   TARGET CrossPlatformTest POST_BUILD
   COMMAND ${CMAKE_COMMAND} -E copy "${CMAKE_CURRENT_BINARY_DIR}/${CMAKE_CFG_INTDIR}/CrossPlatformTest${CMAKE_EXECUTABLE_SUFFIX}" "${CMAKE_CURRENT_SOURCE_DIR}/CrossPlatformTest/"
)

elseif (${CMAKE_GENERATOR} MATCHES "Xcode" )

endif (NOT ${CMAKE_GENERATOR} MATCHES "Xcode" )

# TODO: Add tests and install targets if needed.
