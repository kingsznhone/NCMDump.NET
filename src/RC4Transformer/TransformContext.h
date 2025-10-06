// TransformContext.h
#pragma once
#include "pch.h"

#ifdef _WIN32
#ifdef RC4TRANSFORMER_EXPORTS
#define API __declspec(dllexport)
#else
#define API __declspec(dllimport)
#endif
#else
#define API __attribute__((visibility("default")))
#endif

#ifdef __cplusplus
extern "C" {
#endif

	typedef struct TransformContext TransformContext;

	API TransformContext* CreateTransformContext(const uint8_t* keyBytes, size_t keyLength);

	API void DestroyTransformContext(TransformContext* ctx);

	API bool TransformData(TransformContext* ctx, uint8_t* data, size_t dataLength);

#ifdef __cplusplus
}
#endif
