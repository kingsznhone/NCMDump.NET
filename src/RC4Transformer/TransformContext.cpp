#include "pch.h"
#include "TransformContext.h"

class alignas(64) TransformContextImpl {
private:
	uint8_t i;
	uint8_t j;
	uint8_t SBox[256];
public:
	explicit TransformContextImpl(const uint8_t* keyBytes, size_t keyLength)
		: i(0), j(0)
	{
		for (int x = 0; x < 256; ++x) {
			SBox[x] = static_cast<uint8_t>(x);
		}
		uint8_t y = 0;
		for (int x = 0; x < 256; ++x) {
			y = (y + SBox[x] + keyBytes[x % keyLength]);
			std::swap(SBox[x], SBox[y]);
		}
	}

	bool Transform(uint8_t* dataBytes, size_t dataLength) {
		uint8_t local_i = i;
		uint8_t local_j = j;
		for (size_t m = 0; m < dataLength; ++m) {
			local_i = static_cast<uint8_t>(local_i + 1);
			uint8_t Si = SBox[local_i];
			local_j = static_cast<uint8_t>(local_i + Si);
			uint8_t Sj = SBox[local_j];

			dataBytes[m] ^= SBox[static_cast<uint8_t>(Si + Sj)];
		}
		i = local_i;
		j = local_j;
		return true;
	}
};

extern "C" {
	TransformContext* CreateTransformContext(const uint8_t* keyBytes, size_t key_bytes_len) {
		if (!keyBytes || key_bytes_len == 0) return nullptr;
		try {
			return reinterpret_cast<TransformContext*>(new TransformContextImpl(keyBytes, key_bytes_len));
		}
		catch (...) {
			return nullptr;
		}
	}

	void DestroyTransformContext(TransformContext* ctx) {
		delete reinterpret_cast<TransformContextImpl*>(ctx);
	}

	bool TransformData(TransformContext* ctx, uint8_t* dataBytes, size_t dataLength) {
		return reinterpret_cast<TransformContextImpl*>(ctx)->Transform(
			dataBytes, dataLength);
	}
}